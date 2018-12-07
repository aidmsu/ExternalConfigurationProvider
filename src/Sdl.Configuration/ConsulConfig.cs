using System;

namespace Sdl.Configuration
{
    /// <exclude />
    public class ConsulConfig
    {
        /// <summary>
        /// The default timespan to wait before the config request times out. Use <see cref="ConsulConfig.Timeout"/> to overwrite it.
        /// </summary>
        public readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(15);

        /// <exclude />
        public ConsulConfig()
        {
            UseCache = true;
            Timeout = DefaultTimeout;
        }

        private string _url;
        private string _environment;

        /// <summary>
        /// The absolute url where Consul is hosted.
        /// </summary>
        public string Url
        {
            get => _url;
            set
            {
                if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(Url));
                if (!Uri.TryCreate(value, UriKind.Absolute, out var address)) throw new ArgumentException("Bad url format.", nameof(Url));

                Address = address;
                _url = value;
            }
        }

        internal Uri Address { get; private set; }

        /// <summary>
        /// The Consul authentication token.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// The environment where app runs.
        /// </summary>
        public string Environment
        {
            get => _environment;
            set
            {
                if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(Environment));

                _environment = value;
            }
        }

        /// <summary>
        /// If provider caches settings. Default: true.
        /// </summary>
        public bool UseCache { get; set; }

        /// <summary>
        /// The timespan to wait before the config request times out.
        /// </summary>
        public TimeSpan Timeout { get; set; }
    }
}
