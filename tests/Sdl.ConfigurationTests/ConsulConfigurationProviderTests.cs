using System;
using Sdl.Configuration;
using NUnit.Framework;

namespace Sdl.ConfigurationTests
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void Ctor_ThrowsException_WhenUrlIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new ConsulConfigurationProvider(null, "token", "debug"));

            Assert.AreEqual("url", exception.ParamName);
        }

        [Test]
        public void Ctor_ThrowsException_WhenUrlIsEmpty()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new ConsulConfigurationProvider(string.Empty, "token", "debug"));

            Assert.AreEqual("url", exception.ParamName);
        }

        [Test]
        public void Ctor_ThrowsException_WhenUrlIsBad()
        {
            var exception = Assert.Throws<ArgumentException>(() => new ConsulConfigurationProvider("localhost", "token", "debug"));

            Assert.AreEqual("url", exception.ParamName);
        }
    }
}