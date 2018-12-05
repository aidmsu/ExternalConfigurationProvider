using System;
using Sdl.Configuration;
using Xunit;

namespace Sdl.ConfigurationTests
{
    public class Tests
    {
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
    }
}