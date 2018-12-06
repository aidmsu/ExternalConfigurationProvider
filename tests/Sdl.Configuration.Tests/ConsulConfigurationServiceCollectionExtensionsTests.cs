#if NETCOREAPP2_0
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Sdl.Configuration;
using Xunit;

namespace Sdl.ConfigurationTests
{
    public class ConsulConfigurationServiceCollectionExtensionsTests
    {
        private readonly Mock<IServiceCollection> _serviceCollection = new Mock<IServiceCollection>();

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void AddConsul_ThrowsException_WhenUrlIsNotSpecified(string url)
        {
            var exception = Assert.Throws<ArgumentNullException>(() => _serviceCollection.Object.AddConsul(options => { options.Url = url; }));

            Assert.Equal("Url", exception.ParamName);
        }

        [Theory]
        [InlineData("AnyRandomvalue")]
        [InlineData("/consul")]
        public void AddConsul_ThrowsException_WhenUrlIsBad(string url)
        {
            var exception = Assert.Throws<ArgumentException>(() => _serviceCollection.Object.AddConsul(options => { options.Url = url; }));

            Assert.Equal("Url", exception.ParamName);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void AddConsul_ThrowsException_WhenEnvironmentIsNotSpecified(string environment)
        {
            var exception = Assert.Throws<ArgumentNullException>(() => _serviceCollection.Object.AddConsul(options => { options.Environment = environment; }));

            Assert.Equal("Environment", exception.ParamName);
        }
    }
}
#endif