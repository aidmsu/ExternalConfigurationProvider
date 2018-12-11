using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Consul;

namespace ExternalConfiguration
{
    /// <summary>
    /// Provides methods to get service settings from Consul.
    /// </summary>
    public class ConsulConfigurationStore : IExternalConfigurationStore
    {
        /// <summary>
        /// The default timespan to wait before the config request times out. Use <see cref="ConsulConfig.Timeout"/> to overwrite it.
        /// </summary>
        public readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(15);

        private static readonly Func<Uri, string, TimeSpan, IConsulClient> DefaultConsulClientFactory = (address, token, timeout) =>
        {
            return new ConsulClient(config =>
            {
                config.Address = address;
                config.Token = token;
                config.WaitTime = timeout;
            });
        };


        private readonly Uri _address;
        private readonly string _token;
        private readonly TimeSpan _timeout;
        private readonly Func<Uri, string, TimeSpan, IConsulClient> _consulClientFactory;

        /// <summary>
        /// Initializes a new instance of the ExternalConfigurationProvider class that gets services settings stored in Consul.
        /// Settings are cached after getting from Consul. If you don't want cache settings use <see cref="ExternalConfigurationProvider"/>
        /// </summary>
        /// <param name="url">The absolute url where Consul is hosted.</param>
        /// <param name="token">The Consul authentication token.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the options.Url is not specified.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the options.Url is not absolute.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown when the options.Environment is not specified.</exception>
        /// <example>
        /// <code>
        /// var provider = new ExternalConfigurationProvider("http://localhost:8500", "b1gs33cr3t", "staging");
        /// </code>
        /// </example>
        public ConsulConfigurationStore(string url, string token)
            : this(new ConsulConfig { Url = url, Token = token }, DefaultConsulClientFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ExternalConfigurationProvider class that gets services settings stored in Consul.
        /// </summary>
        /// <param name="config">Consul configuration.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the options.Url is not specified.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the options.Url is not absolute.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown when the options.Environment is not specified.</exception>
        /// <example>
        /// <code>
        /// var provider = new ExternalConfigurationProvider("http://localhost:8500", "b1gs33cr3t", "staging");
        /// </code>
        /// </example>
        public ConsulConfigurationStore(ConsulConfig config)
            : this(config, DefaultConsulClientFactory)
        {
        }

        internal ConsulConfigurationStore(ConsulConfig config, Func<Uri, string, TimeSpan, IConsulClient> consulClientFactory)
        {
            if (string.IsNullOrEmpty(config.Url)) throw new ArgumentNullException(nameof(config.Url));
            if (!Uri.TryCreate(config.Url, UriKind.Absolute, out _address)) throw new ArgumentException("Bad url format.", nameof(config.Url));
            if (consulClientFactory == null) throw new ArgumentNullException(nameof(consulClientFactory));

            _token = config.Token;
            _timeout = config.Timeout ?? DefaultTimeout;

            _consulClientFactory = consulClientFactory;
        }

        /// <summary>
        /// Gets service settings from Consul and converts them to the specified .NET type.
        /// </summary>
        /// <param name="environment">The environment where app runs.</param>
        /// <param name="service">The service name.</param>
        /// <param name="hosting">The hosting where the service is hosted. Optional.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the service is not specified.</exception>
        /// <example>
        /// <code>
        /// var settings = GetServiceSettingsAsync&lt;RedisSettings&gt;("Redis", "Azure");
        /// </code>
        /// </example>
        public async Task<Dictionary<string, string>> GetServiceConfigAsync(string environment, string service, string hosting, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(service)) throw new ArgumentNullException(nameof(service));

            var servicePrefix = GetStoreServiceKey(environment, service, hosting);

            using(var client = _consulClientFactory(_address, _token, _timeout))
            {
                var kvPairResult = await client.KV.List(servicePrefix, cancellationToken).ConfigureAwait(false);

                var response = kvPairResult.Response;

                if (response == null || !response.Any()) return null;

                return response.ToDictionary(
                    kv => kv.Key.Replace(servicePrefix, String.Empty),
                    kv => kv.Value == null ? string.Empty : Encoding.UTF8.GetString(kv.Value, 0, kv.Value.Length));
            }
        }

        private static string GetStoreServiceKey(string environment, string service, string hosting)
        {
            return string.IsNullOrEmpty(hosting)
                ? $"{environment}/{service}/"
                : $"{environment}/{hosting}/{service}/";
        }
    }
}