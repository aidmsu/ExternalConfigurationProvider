using System;
using Sdl.Configuration;
using Xunit;

namespace Sdl.ConfigurationTests
{
    public class Tests
    {
        private readonly string _correctUrl = "http://localhost";

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

        [Fact]
        public void GetConsulKey_ReturnsCorrectKey_WhenServiceAndHostingAreSpecified()
        {
            var key = ConsulConfigurationProvider.GetConsulKey("dev", "mango", "azure");

            Assert.Equal("dev/azure/mango/", key);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void GetConsulKey_ReturnsCorrectKey_WhenHostinIsNotSpecified(string hosting)
        {
            var key = ConsulConfigurationProvider.GetConsulKey("dev", "mango", hosting);

            Assert.Equal("dev/mango/", key);
        }

        [Theory]
        [InlineData("production", "telephony", "azurE")]
        [InlineData("Production", "telephonY", "Azure")]
        [InlineData("productioN", "TelephonY", "AZURE")]
        [InlineData("PRODUCTION", "TELEPHONY", "AzurE")]
        public void GetConsulKey_ReturnsNormalizedKey(string env, string service, string hosting)
        {
            var key = ConsulConfigurationProvider.GetConsulKey(env, service, hosting);

            Assert.Equal("production/azure/telephony/", key);
        }
    }
}