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

namespace Sdl.Configuration.Tests
{
    public class ConsulConfigurationStoreTests
    {
        private readonly string _correctUrl = "http://localhost";
        private readonly Mock<IConsulClient> _mockConsulClient = new Mock<IConsulClient>();

        [Fact]
        public void Ctor_ThrowsException_WhenConsulClientFactoryIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new ConsulConfigurationStore(new ConsulConfig
            {
                Url = _correctUrl,
                Environment = "dev"
            }, null));

            Assert.Equal("consulClientFactory", exception.ParamName);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void GetServiceConfigAsync_ThrowsException_WhenServiceIsNotSpecified(string service)
        {
            var provider = CreateConsulStore(_correctUrl, "token", "dev", true);

            var exception = Assert.ThrowsAsync<ArgumentNullException>(() => provider.GetServiceConfigAsync("debug", service, "hosting"));

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
            ConsulClientShouldReturn(null);

            var provider = CreateConsulStore(_correctUrl, "token", "debug", false);

            var config = await provider.GetServiceConfigAsync("debug", "mango", null);

            Assert.Null(config);
        }

        [Fact]
        public async Task GetServiceConfigAsync_HandleEmpty()
        { 
            ConsulClientShouldReturn(new Dictionary<string, string>());

            var provider = CreateConsulStore(_correctUrl, "token", "debug", false);

            var config = await provider.GetServiceConfigAsync("debug", "mango", null);

            Assert.Null(config);
        }

        [Fact]
        public async Task GetServiceConfigAsync_ConvertConsulResponseToSettingsDictionary()
        {
            ConsulClientShouldReturn(new Dictionary<string, string> { { "debug/mango/key1", "value1" } });

            var provider = CreateConsulStore(_correctUrl, "token", "debug", false);

            var config = await provider.GetServiceConfigAsync("debug", "mango", null);

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

        private ConsulConfigurationStore CreateConsulStore(string url, string token, string environment, bool useCache)
        {
            var config = new ConsulConfig
            {
                Url = url,
                Token = token,
                Environment = environment,
                UseCache = useCache
            };

            return new ConsulConfigurationStore(config, (address, t, timeout) => _mockConsulClient.Object);
        }
    }
}