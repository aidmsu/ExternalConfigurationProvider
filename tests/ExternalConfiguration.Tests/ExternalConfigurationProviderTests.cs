using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ExternalConfiguration;
using Xunit;
using Moq;

namespace ExternalConfiguration.Tests
{
    public class ExternalConfigurationProviderTests
    {
        private readonly string _correctUrl = "http://localhost";
        private readonly Mock<IExternalConfigurationStore> _mockStore = new Mock<IExternalConfigurationStore>();

        [Fact]
        public void Ctor_ThrowsException_WhenStoreIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new ExternalConfigurationProvider(null, "dev"));

            Assert.Equal("store", exception.ParamName);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Ctor_ThrowsException_WhenEnvironmentIsNotSpecified(string env)
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new ExternalConfigurationProvider(_mockStore.Object, env));

            Assert.Equal("environment", exception.ParamName);
        }

        [Theory]
        [InlineData("debug")]
        [InlineData("dev")]
        [InlineData("staging")]
        public void Ctor_SetsEnvironment(string environment)
        {
            var configurationProvider = new ExternalConfigurationProvider(_mockStore.Object, environment);

            Assert.Equal(environment, configurationProvider.Environment);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void GetServiceSettingsAsync_ThrowsException_WhenServiceIsNotSpecified(string service)
        {
            var provider = CreateProvider(_correctUrl, "token", "dev", true);

            var exception = Assert.ThrowsAsync<ArgumentNullException>(() => provider.GetServiceSettingsAsync(service, "hosting"));

            Assert.Equal("service", exception.Result.ParamName);
        }

        [Fact]
        public void GetConsulServiceKey_ReturnsCorrectKey_WhenServiceAndHostingAreSpecified()
        {
            var key = ExternalConfigurationProvider.GetFullServiceName("dev", "mango", "azure");

            Assert.Equal("dev/azure/mango/", key);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void GetConsulServiceKey_ReturnsCorrectKey_WhenHostinIsNotSpecified(string hosting)
        {
            var key = ExternalConfigurationProvider.GetFullServiceName("dev", "mango", hosting);

            Assert.Equal("dev/mango/", key);
        }

        [Fact]
        public async Task GetServiceSettingsAsync_HandleNullResponse()
        {
            StoreShouldReturn(null);

            var provider = CreateProvider(_correctUrl, "token", "debug", false);

            var config = await provider.GetServiceSettingsAsync("mango");

            Assert.Null(config);
        }

        [Fact]
        public async Task GetServiceSettingsAsync_HandleEmpty()
        { 
            StoreShouldReturn(new Dictionary<string, string>());

            var provider = CreateProvider(_correctUrl, "token", "debug", false);

            var config = await provider.GetServiceSettingsAsync("mango");

            Assert.Null(config);
        }

        [Fact]
        public async Task GetServiceSettingsAsync_ReturnTheSameAsStore()
        {
            StoreShouldReturn(new Dictionary<string, string> { { "key1", "value1" } });

            var provider = CreateProvider(_correctUrl, "token", "debug", false);

            var config = await provider.GetServiceSettingsAsync("mango");

            Assert.NotNull(config);
            Assert.Single(config);
            Assert.Contains("value1", config["key1"]);
        }

        [Fact]
        public async Task GetServiceSettingsAsync_UseCache_WhenSettingsIsAlreadyInCache()
        {
            var provider = CreateProvider(_correctUrl, "token", "debug", true);

            provider.ServiceSettingsCache["debug/mango/"] = new Dictionary<string, string> {{"key1", "cachedValue"}};
            StoreShouldReturn(new Dictionary<string, string> { { "key1", "consulValue" } });

            var config = await provider.GetServiceSettingsAsync("mango");

            Assert.NotNull(config);
            Assert.Single(config);
            Assert.Contains("cachedValue", config["key1"]);
        }

        [Fact]
        public async Task GetServiceSettingsAsync_IgnoreCache_WhenUseCacheIsFalse()
        {
            var provider = CreateProvider(_correctUrl, "token", "debug", false);

            provider.ServiceSettingsCache["debug/mango/"] = new Dictionary<string, string> { { "key1", "cachedValue" } };
            StoreShouldReturn(new Dictionary<string, string> { { "key1", "consulValue" } });

            var config = await provider.GetServiceSettingsAsync("mango");

            Assert.NotNull(config);
            Assert.Single(config);
            Assert.Contains("consulValue", config["key1"]);
        }

        [Fact]
        public async Task GetServiceSettingsAsync_GetsSettingsFromConsul_WhenUseCacheIsTrueButCacheDoesntContainValue()
        {
            var provider = CreateProvider(_correctUrl, "token", "debug", true);

            provider.ServiceSettingsCache["debug/mango1/"] = new Dictionary<string, string> { { "key1", "cachedValue" } };
            StoreShouldReturn(new Dictionary<string, string> { { "key1", "consulValue" } });

            var config = await provider.GetServiceSettingsAsync("mango");

            Assert.NotNull(config);
            Assert.Single(config);
            Assert.Contains("consulValue", config["key1"]);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void GetServiceSettingsAsyncT_ThrowsException_WhenServiceIsNotSpecified(string service)
        {
            var provider = CreateProvider(_correctUrl, "token", "dev", true);

            var exception = Assert.ThrowsAsync<ArgumentNullException>(() => provider.GetServiceSettingsAsync<MangoConfig>(service, "hosting"));

            Assert.Equal("service", exception.Result.ParamName);
        }

        [Fact]
        public async Task GetServiceSettingsAsyncT_ReturnNullIfKeyIsNotFound()
        {
            StoreShouldReturn(new Dictionary<string, string>());

            var provider = CreateProvider(_correctUrl, "token", "debug", false);

            var config = await provider.GetServiceSettingsAsync<MangoConfig>("mango");

            Assert.Null(config);
        }

        [Fact]
        public async Task GetServiceSettingsAsyncT_ConvertConsulResponseToSettingsObject()
        {
            StoreShouldReturn(new Dictionary<string, string>
            {
                { "ApiKey", "secretKey" },
                { "ApiSignature", "secretSignature" }
            });

            var provider = CreateProvider(_correctUrl, "token", "debug", false);

            var config = await provider.GetServiceSettingsAsync<MangoConfig>("mango");

            Assert.NotNull(config);
            Assert.Equal("secretKey", config.ApiKey);
            Assert.Equal("secretSignature", config.ApiSignature);
        }

        [Fact]
        public async Task GetServiceSettingsAsyncT_HandleDifficultCase()
        {
            StoreShouldReturn(new Dictionary<string, string>
            {
                { "Name", "custom" },
                { "Threshold", "2018" },
                { "DoubleValue", "42.56" },
                {
                    "EmailsArray",
                    @"
                    [
                        {
                            ""UserName"": ""aidmsu"",
                            ""SmtpPort"": 42
                        },
                        {
                            ""UserName"": ""aidmsu2"",
                            ""SmtpPort"": 4242
                        }
                    ]"
                },
                {
                    "Email",
                    @" 
                    {
                        ""UserName"": ""aidmsu3"",
                        ""SmtpPort"": 424242
                    }" }
            });

            var provider = CreateProvider(_correctUrl, "token", "debug", false);

            var config = await provider.GetServiceSettingsAsync<CustomConfig>("custom");

            Assert.NotNull(config);
            Assert.Equal("custom", config.Name);
            Assert.Equal(2018, config.Threshold);
            Assert.Equal(42.56, config.DoubleValue);
            Assert.NotNull(config.EmailsArray);
            Assert.Equal(2, config.EmailsArray.Length);
            Assert.Equal("aidmsu", config.EmailsArray[0].UserName);
            Assert.Equal(42, config.EmailsArray[0].SmtpPort);
            Assert.Equal("aidmsu2", config.EmailsArray[1].UserName);
            Assert.Equal(4242, config.EmailsArray[1].SmtpPort);
            Assert.NotNull(config.Email);
            Assert.Equal("aidmsu3", config.Email.UserName);
            Assert.Equal(424242, config.Email.SmtpPort);
        }

        [Fact]
        public async Task GetServiceSettingsAsyncT_IsNotCaseSensitive()
        {
            StoreShouldReturn(new Dictionary<string, string>
            {
                { "apiKey", "secretKey" },
                { "ApiSigNaturE", "secretSignature" }
            });

            var provider = CreateProvider(_correctUrl, "token", "debug", false);

            var config = await provider.GetServiceSettingsAsync<MangoConfig>("mango");

            Assert.NotNull(config);
            Assert.Equal("secretKey", config.ApiKey);
            Assert.Equal("secretSignature", config.ApiSignature);
        }

        [Fact]
        public async Task GetServiceSettingsAsyncT_ReturnsCorrectObject_WhenTHasPropertiesDifferingOnlyCase()
        {
            StoreShouldReturn(new Dictionary<string, string>
            {
                { "ApiKey", "secretKey" },
                { "ApiSignature", "secretSignature" }
            });

            var provider = CreateProvider(_correctUrl, "token", "debug", false);

            var config = await provider.GetServiceSettingsAsync<MangoConfigWithPropertiesDifferingOnlyCase>("mango");

            Assert.NotNull(config);
            Assert.Equal("secretKey", config.ApiKey);
            Assert.Equal("secretSignature", config.ApiSignature);
        }

        [Fact]
        public void GetPropertyValue_ReturnCorrect_WhenTypeIsInt32()
        {
            var value = ExternalConfigurationProvider.GetPropertyValue(typeof(Int32), "42");

            var intValue = Assert.IsType<Int32>(value);
            Assert.Equal(42, intValue);
        }

        [Fact]
        public void GetPropertyValue_ReturnCorrect_WhenTypeIsInt64()
        {
            var value = ExternalConfigurationProvider.GetPropertyValue(typeof(Int64), "42");

            var longValue = Assert.IsType<Int64>(value);
            Assert.Equal(42, longValue);
        }

        [Fact]
        public void GetPropertyValue_ReturnCorrect_WhenTypeIsDouble()
        {
            var value = ExternalConfigurationProvider.GetPropertyValue(typeof(Double), "42.42");

            var doubleValue = Assert.IsType<Double>(value);
            Assert.Equal(42.42, doubleValue);
        }

        [Fact]
        public void GetPropertyValue_ReturnCorrect_WhenTypeIsString()
        {
            var value = ExternalConfigurationProvider.GetPropertyValue(typeof(string), "custom value");

            var stringValue = Assert.IsType<string>(value);
            Assert.Equal("custom value", stringValue);
        }

        [Fact]
        public void GetPropertyValue_ReturnCorrect_WhenTypeIsCustomClass()
        {
            var value = ExternalConfigurationProvider.GetPropertyValue(typeof(EmailSettings), @"
{
    ""UserName"": ""aidmsu"",
    ""SmtpPort"": 25
}");

            var emailSettings = Assert.IsType<EmailSettings>(value);
            Assert.Equal("aidmsu", emailSettings.UserName);
            Assert.Equal(25, emailSettings.SmtpPort);
        }

        [Fact]
        public void GetPropertyValue_ReturnCorrect_WhenTypeIsArray()
        {
            var value = ExternalConfigurationProvider.GetPropertyValue(typeof(EmailSettings[]), @"[
{
    ""UserName"": ""aidmsu"",
    ""SmtpPort"": 25
},
{
    ""UserName"": ""aidmsu2"",
    ""SmtpPort"": 42
}]");

            var emailSettingsArray = Assert.IsType<EmailSettings[]>(value);
            Assert.Equal(2, emailSettingsArray.Length);
            Assert.Equal("aidmsu", emailSettingsArray[0].UserName);
            Assert.Equal(25, emailSettingsArray[0].SmtpPort);
            Assert.Equal("aidmsu2", emailSettingsArray[1].UserName);
            Assert.Equal(42, emailSettingsArray[1].SmtpPort);
        }

        [Fact]
        public void GetPropertyValue_ReturnCorrect_WhenTypeIsList()
        {
            var value = ExternalConfigurationProvider.GetPropertyValue(typeof(List<EmailSettings>), @"[
{
    ""UserName"": ""aidmsu"",
    ""SmtpPort"": 25
},
{
    ""UserName"": ""aidmsu2"",
    ""SmtpPort"": 42
}]");

            var emailSettingsArray = Assert.IsType<List<EmailSettings>>(value);
            Assert.Equal(2, emailSettingsArray.Count);
            Assert.Equal("aidmsu", emailSettingsArray[0].UserName);
            Assert.Equal(25, emailSettingsArray[0].SmtpPort);
            Assert.Equal("aidmsu2", emailSettingsArray[1].UserName);
            Assert.Equal(42, emailSettingsArray[1].SmtpPort);
        }

        private void StoreShouldReturn(Dictionary<string, string> keyValues)
        {
            _mockStore.Setup(store => store.GetServiceConfigAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(keyValues));
        }

        private ExternalConfigurationProvider CreateProvider(string url, string token, string environment, bool useCache)
        {
            var config = new ConsulConfig
            {
                Url = url,
                Token = token
            };

            return new ExternalConfigurationProvider(_mockStore.Object, environment, useCache);
        }

        private class MangoConfig
        {
            public string ApiKey { get; set; }
            public string ApiSignature { get; set; }
        }

        private class MangoConfigWithPropertiesDifferingOnlyCase
        {
            public string ApiKey { get; set; }
            public string apiKey { get; set; }
            public string ApiSignature { get; set; }
        }

        private class EmailSettings
        {
            public string UserName { get; set; }
            public int SmtpPort { get; set; }
        }

        private class CustomConfig
        {
            public string Name { get; set; }

            public int Threshold { get; set; }

            public double DoubleValue { get; set; }

            public EmailSettings[] EmailsArray { get; set; }

            public EmailSettings Email { get; set; }
        }

    }
}