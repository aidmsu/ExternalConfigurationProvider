using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Consul;
using Sdl.Configuration;
using Xunit;
using Moq;

namespace Sdl.ConfigurationTests
{
    public class Tests
    {
        private readonly string _correctUrl = "http://localhost";
        private readonly Mock<IConsulClient> _mockConsulClient = new Mock<IConsulClient>();

        [Fact]
        public void Ctor_ThrowsException_WhenUrlIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new ConsulConfigurationProvider(null, "token", "debug"));

            Assert.Equal("url", exception.ParamName);
        }

        [Fact]
        public void Ctor_ThrowsException_WhenUrlIsEmpty()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new ConsulConfigurationProvider(string.Empty, "token", "debug"));

            Assert.Equal("url", exception.ParamName);
        }

        [Fact]
        public void Ctor_ThrowsException_WhenUrlIsBad()
        {
            var exception = Assert.Throws<ArgumentException>(() => new ConsulConfigurationProvider("localhost", "token", "debug"));

            Assert.Equal("url", exception.ParamName);
        }

        [Fact]
        public void Ctor_ThrowsException_WhenEnvironmentIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new ConsulConfigurationProvider(_correctUrl, "token", null));

            Assert.Equal("environment", exception.ParamName);
        }

        [Fact]
        public void Ctor_ThrowsException_WhenEnvironmentIsEmpty()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new ConsulConfigurationProvider(_correctUrl, "token", null));

            Assert.Equal("environment", exception.ParamName);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void GetServiceConfigAsync_ThrowsException_WhenServiceIsNotSpecified(string service)
        {
            var provider = new ConsulConfigurationProvider(_correctUrl, "token", "dev");

            var exception = Assert.ThrowsAsync<ArgumentNullException>(() => provider.GetServiceConfigAsync(service, "hosting"));

            Assert.Equal("service", exception.Result.ParamName);
        }

        [Fact]
        public void GetConsulServiceKey_ReturnsCorrectKey_WhenServiceAndHostingAreSpecified()
        {
            var key = ConsulConfigurationProvider.GetConsulServiceKey("dev", "mango", "azure");

            Assert.Equal("dev/azure/mango/", key);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void GetConsulServiceKey_ReturnsCorrectKey_WhenHostinIsNotSpecified(string hosting)
        {
            var key = ConsulConfigurationProvider.GetConsulServiceKey("dev", "mango", hosting);

            Assert.Equal("dev/mango/", key);
        }

        [Theory]
        [InlineData("production", "telephony", "azurE")]
        [InlineData("Production", "telephonY", "Azure")]
        [InlineData("productioN", "TelephonY", "AZURE")]
        [InlineData("PRODUCTION", "TELEPHONY", "AzurE")]
        public void GetConsulServiceKey_ReturnsNormalizedKey(string env, string service, string hosting)
        {
            var key = ConsulConfigurationProvider.GetConsulServiceKey(env, service, hosting);

            Assert.Equal("production/azure/telephony/", key);
        }

        [Fact]
        public async Task GetServiceConfigAsync_HandleNullResponse()
        {
            ConsulClientShouldReturn(null);

            var provider = new ConsulConfigurationProvider(_correctUrl, "token", "debug", (address, token) => _mockConsulClient.Object);

            var config = await provider.GetServiceConfigAsync("mango");

            Assert.Null(config);
        }

        [Fact]
        public async Task GetServiceConfigAsync_HandleEmpty()
        { 
            ConsulClientShouldReturn(new Dictionary<string, string>());

            var provider = new ConsulConfigurationProvider(_correctUrl, "token", "debug", (address, token) => _mockConsulClient.Object);

            var config = await provider.GetServiceConfigAsync("mango");

            Assert.Null(config);
        }

        [Fact]
        public async Task GetServiceConfigAsync_ConvertConsulResponseToSettingsDictionary()
        {
            ConsulClientShouldReturn(new Dictionary<string, string>{ { "debug/mango/key1", "value1" } });

            var provider = new ConsulConfigurationProvider(_correctUrl, "token", "debug", (address, token) => _mockConsulClient.Object);

            var config = await provider.GetServiceConfigAsync("mango");

            Assert.NotNull(config);
            Assert.Single(config);
            Assert.Contains("value1", config["key1"]);
        }

        private void ConsulClientShouldReturn(IEnumerable<KeyValuePair<string, string>> keyValues)
        {
            var kvPairs = keyValues?.Select(kv => new KVPair(kv.Key) {Value = Encoding.UTF8.GetBytes(kv.Value)}).ToArray();

            _mockConsulClient.Setup(client => client.KV.List(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new QueryResult<KVPair[]> { Response = kvPairs }));
        }
    }
}