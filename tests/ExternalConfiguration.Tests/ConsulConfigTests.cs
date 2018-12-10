using System;
using ExternalConfiguration;
using Xunit;

namespace ExternalConfiguration.Tests
{
    public class ConsulConfigTests
    {
        [Fact]
        public void SetUrl_ThrowsException_WhenUrlIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new ConsulConfig { Url = null });

            Assert.Equal("Url", exception.ParamName);
        }

        [Fact]
        public void Ctor_ThrowsException_WhenUrlIsEmpty()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new ConsulConfig { Url = string.Empty });

            Assert.Equal("Url", exception.ParamName);
        }

        [Fact]
        public void Ctor_ThrowsException_WhenUrlIsBad()
        {
            var exception = Assert.Throws<ArgumentException>(() => new ConsulConfig { Url = "localhost" });

            Assert.Equal("Url", exception.ParamName);
        }
    }
}