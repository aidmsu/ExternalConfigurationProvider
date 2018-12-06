#if NETSTANDARD1_5
namespace Sdl.Configuration
{
    /// <exclude />
    public class ConsulOptions
    {
        /// <summary>
        /// The absolute url where Consul is hosted
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The Consul authentication token.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// The environment where app runs.
        /// </summary>
        public string Environment { get; set; }
    }
}
#endif
