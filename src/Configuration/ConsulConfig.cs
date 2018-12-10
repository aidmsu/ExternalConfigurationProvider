using System;

namespace Configuration
{
    /// <exclude />
    public class ConsulConfig : ProviderConfig
    {
        private string _url;

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
        /// The timespan to wait before the config request times out.
        /// </summary>
        public TimeSpan? Timeout { get; set; }
    }
}
