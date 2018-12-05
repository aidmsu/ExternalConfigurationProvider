#if NETSTANDARD1_3
namespace Sdl.Configuration
{
    public class ConsulOptions
    {
        /// <summary>
        /// Consul url.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Consul authentication token.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// The environment where the app runs. Example: debug, staging, production.
        /// </summary>
        public string Environment { get; set; }
    }
}
#endif
