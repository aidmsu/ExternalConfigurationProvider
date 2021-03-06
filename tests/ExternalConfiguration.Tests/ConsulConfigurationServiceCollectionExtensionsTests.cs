﻿#if NETCOREAPP2_0
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using ExternalConfiguration;
using Xunit;

namespace ExternalConfiguration.Tests
{
    public class ConsulConfigurationServiceCollectionExtensionsTests
    {
        private readonly Mock<IServiceCollection> _serviceCollection = new Mock<IServiceCollection>();

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void AddConsulConfigurationProvider_ThrowsException_WhenUrlIsNotSpecified(string url)
        {
            var exception = Assert.Throws<ArgumentNullException>(() => _serviceCollection.Object.AddConsulConfigurationProvider(options => { options.Url = url; }));

            Assert.Equal("Url", exception.ParamName);
        }

        [Theory]
        [InlineData("AnyRandomvalue")]
        [InlineData("/consul")]
        public void AddConsulConfigurationProvider_ThrowsException_WhenUrlIsBad(string url)
        {
            var exception = Assert.Throws<ArgumentException>(() => _serviceCollection.Object.AddConsulConfigurationProvider(options => { options.Url = url; }));

            Assert.Equal("Url", exception.ParamName);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void AddConsulConfigurationProvider_ThrowsException_WhenEnvironmentIsNotSpecified(string environment)
        {
            var exception = Assert.Throws<ArgumentNullException>(() => _serviceCollection.Object.AddConsulConfigurationProvider(options => { options.Environment = environment; }));

            Assert.Equal("Environment", exception.ParamName);
        }
    }
}
#endif