using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sdl.Configuration;
using Xunit;
using Moq;

namespace Sdl.ConfigurationTests
{
    public class ExternalConfigurationProviderTests
    {
        private readonly string _correctUrl = "http://localhost";
        private readonly Mock<IExternalConfigurationStore> _mockStore = new Mock<IExternalConfigurationStore>();

        [Fact]
        public void Ctor_ThrowsException_WhenStoreIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new ExternalConfigurationProvider(null, new ConsulConfig
            {
                Url = _correctUrl,
                Environment = "dev"
            }));

            Assert.Equal("store", exception.ParamName);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void GetServiceConfigAsync_ThrowsException_WhenServiceIsNotSpecified(string service)
        {
            var provider = CreateProvider(_correctUrl, "token", "dev", true);

            var exception = Assert.ThrowsAsync<ArgumentNullException>(() => provider.GetServiceConfigAsync(service, "hosting"));

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
        public async Task GetServiceConfigAsync_HandleNullResponse()
        {
            StoreShouldReturn(null);

            var provider = CreateProvider(_correctUrl, "token", "debug", false);

            var config = await provider.GetServiceConfigAsync("mango");

            Assert.Null(config);
        }

        [Fact]
        public async Task GetServiceConfigAsync_HandleEmpty()
        { 
            StoreShouldReturn(new Dictionary<string, string>());

            var provider = CreateProvider(_correctUrl, "token", "debug", false);

            var config = await provider.GetServiceConfigAsync("mango");

            Assert.Null(config);
        }

        [Fact]
        public async Task GetServiceConfigAsync_ReturnTheSameAsStore()
        {
            StoreShouldReturn(new Dictionary<string, string> { { "key1", "value1" } });

            var provider = CreateProvider(_correctUrl, "token", "debug", false);

            var config = await provider.GetServiceConfigAsync("mango");

            Assert.NotNull(config);
            Assert.Single(config);
            Assert.Contains("value1", config["key1"]);
        }

        [Fact]
        public async Task GetServiceConfigAsync_UseCache_WhenSettingsIsAlreadyInCache()
        {
            var provider = CreateProvider(_correctUrl, "token", "debug", true);

            provider.ServiceSettingsCache["debug/mango/"] = new Dictionary<string, string> {{"key1", "cachedValue"}};
            StoreShouldReturn(new Dictionary<string, string> { { "key1", "consulValue" } });

            var config = await provider.GetServiceConfigAsync("mango");

            Assert.NotNull(config);
            Assert.Single(config);
            Assert.Contains("cachedValue", config["key1"]);
        }

        [Fact]
        public async Task GetServiceConfigAsync_IgnoreCache_WhenUseCacheIsFalse()
        {
            var provider = CreateProvider(_correctUrl, "token", "debug", false);

            provider.ServiceSettingsCache["debug/mango/"] = new Dictionary<string, string> { { "key1", "cachedValue" } };
            StoreShouldReturn(new Dictionary<string, string> { { "key1", "consulValue" } });

            var config = await provider.GetServiceConfigAsync("mango");

            Assert.NotNull(config);
            Assert.Single(config);
            Assert.Contains("consulValue", config["key1"]);
        }

        [Fact]
        public async Task GetServiceConfigAsync_GetsSettingsFromConsul_WhenUseCacheIsTrueButCacheDoesntContainValue()
        {
            var provider = CreateProvider(_correctUrl, "token", "debug", true);

            provider.ServiceSettingsCache["debug/mango1/"] = new Dictionary<string, string> { { "key1", "cachedValue" } };
            StoreShouldReturn(new Dictionary<string, string> { { "key1", "consulValue" } });

            var config = await provider.GetServiceConfigAsync("mango");

            Assert.NotNull(config);
            Assert.Single(config);
            Assert.Contains("consulValue", config["key1"]);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void GetServiceConfigAsyncT_ThrowsException_WhenServiceIsNotSpecified(string service)
        {
            var provider = CreateProvider(_correctUrl, "token", "dev", true);

            var exception = Assert.ThrowsAsync<ArgumentNullException>(() => provider.GetServiceConfigAsync<MangoConfig>(service, "hosting"));

            Assert.Equal("service", exception.Result.ParamName);
        }

        [Fact]
        public async Task GetServiceConfigAsyncT_ConvertConsulResponseToSettingsObject()
        {
            StoreShouldReturn(new Dictionary<string, string>
            {
                { "ApiKey", "secretKey" },
                { "ApiSignature", "secretSignature" }
            });

            var provider = CreateProvider(_correctUrl, "token", "debug", false);

            var config = await provider.GetServiceConfigAsync<MangoConfig>("mango");

            Assert.NotNull(config);
            Assert.Equal("secretKey", config.ApiKey);
            Assert.Equal("secretSignature", config.ApiSignature);
        }

        [Fact]
        public async Task GetServiceConfigAsyncT_IsNotCaseSensitive()
        {
            StoreShouldReturn(new Dictionary<string, string>
            {
                { "apiKey", "secretKey" },
                { "ApiSigNaturE", "secretSignature" }
            });

            var provider = CreateProvider(_correctUrl, "token", "debug", false);

            var config = await provider.GetServiceConfigAsync<MangoConfig>("mango");

            Assert.NotNull(config);
            Assert.Equal("secretKey", config.ApiKey);
            Assert.Equal("secretSignature", config.ApiSignature);
        }

        [Fact]
        public async Task GetServiceConfigAsyncT_ReturnsCorrectObject_WhenTHasPropertiesDifferingOnlyCase()
        {
            StoreShouldReturn(new Dictionary<string, string>
            {
                { "ApiKey", "secretKey" },
                { "ApiSignature", "secretSignature" }
            });

            var provider = CreateProvider(_correctUrl, "token", "debug", false);

            var config = await provider.GetServiceConfigAsync<MangoConfigWithPropertiesDifferingOnlyCase>("mango");

            Assert.NotNull(config);
            Assert.Equal("secretKey", config.ApiKey);
            Assert.Equal("secretSignature", config.ApiSignature);
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
                Token = token,
                Environment = environment,
                UseCache = useCache
            };

            return new ExternalConfigurationProvider(_mockStore.Object, config);
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
    }
}