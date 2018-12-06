using System;

#if NETSTANDARD1_5
namespace Sdl.Configuration
{
    /// <exclude />
    public class ConsulOptions
    {
        private string _url;
        private string _environment;

        /// <summary>
        /// The absolute url where Consul is hosted
        /// </summary>
        public string Url
        {
            get => _url;
            set
            {
                if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(Url));
                if (!Uri.TryCreate(value, UriKind.Absolute, out _)) throw new ArgumentException("Bad url format.", nameof(Url));

                _url = value;
            }
        }

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
    }
}
#endif
