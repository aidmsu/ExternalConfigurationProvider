using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Consul;

namespace Sdl.Configuration
{
    /// <summary>
    /// Provides methods to get service settings from Consul.
    /// </summary>
    public class ConsulConfigurationProvider : IConfigurationProvider
    {
        private static readonly Func<Uri, string, IConsulClient> DefaultConsulClientFactory = (address, token) =>
        {
            return new ConsulClient(config =>
            {
                config.Address = address;
                config.Token = token;
            });
        };

        private readonly Uri _address;
        private readonly string _token;
        private readonly string _environment;
        private readonly bool _useCache;
        private readonly Func<Uri, string, IConsulClient> _consulClientFactory;

        internal readonly ConcurrentDictionary<string, Dictionary<string,string>> ServiceSettingsCache = new ConcurrentDictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// Initializes a new instance of the ConsulConfigurationProvider class that gets services settings stored in Consul.
        /// Settings are cached after getting from Consul. If you don't want cache settings use <see cref="ConsulConfigurationProvider(string, string, string, bool)"/>
        /// </summary>
        /// <param name="url">The absolute url where Consul is hosted.</param>
        /// <param name="token">The Consul authentication token.</param>
        /// <param name="environment">The environment where app runs.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the url is not specified.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the url is not absolute.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown when the environment is not specified.</exception>
        /// <example>
        /// <code>
        /// var provider = new ConsulConfigurationProvider("http://localhost:8500", "b1gs33cr3t", "staging");
        /// </code>
        /// </example>
        public ConsulConfigurationProvider(string url, string token, string environment) 
            : this(url, token, environment, true, DefaultConsulClientFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ConsulConfigurationProvider class that gets services settings stored in Consul.
        /// </summary>
        /// <param name="url">The absolute url where Consul is hosted.</param>
        /// <param name="token">The Consul authentication token.</param>
        /// <param name="environment">The environment where app runs.</param>
        /// <param name="useCache">If settings is cached.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the url is not specified.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the url is not absolute.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown when the environment is not specified.</exception>
        /// <example>
        /// <code>
        /// var provider = new ConsulConfigurationProvider("http://localhost:8500", "b1gs33cr3t", "staging", true);
        /// </code>
        /// </example>
        public ConsulConfigurationProvider(string url, string token, string environment, bool useCache)
            : this(url, token, environment, useCache, DefaultConsulClientFactory)
        {
        }

        internal ConsulConfigurationProvider(string url, string token, string environment, bool useCache, Func<Uri, string, IConsulClient> consulClientFactory)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
            if (!Uri.TryCreate(url, UriKind.Absolute, out _address)) throw new ArgumentException("Bad url format.", nameof(url));
            if (string.IsNullOrEmpty(environment)) throw new ArgumentNullException(nameof(environment));
            if (consulClientFactory == null) throw new ArgumentNullException(nameof(consulClientFactory));

            _token = token;
            _environment = environment;
            _useCache = useCache;

            _consulClientFactory = consulClientFactory;
        }

        /// <summary>
        /// Gets service settings from Consul and converts them to the specified .NET type.
        /// </summary>
        /// <param name="service">The service name.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the service is not specified.</exception>
        /// <example>
        /// <code>
        /// var settings = GetServiceConfigAsync&lt;RedisSettings&gt;("Redis");
        /// </code>
        /// </example>
        public Task<T> GetServiceConfigAsync<T>(string service, CancellationToken cancellationToken = default(CancellationToken)) where T : new()
        {
            return GetServiceConfigAsync<T>(service, null, cancellationToken);
        }

        /// <summary>
        /// Gets service settings from Consul and converts them to the specified .NET type.
        /// </summary>
        /// <param name="service">The service name.</param>
        /// <param name="hosting">The hosting where the service is hosted. Optional.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the service is not specified.</exception>
        /// <example>
        /// <code>
        /// var settings = GetServiceConfigAsync&lt;RedisSettings&gt;("Redis", "Azure");
        /// </code>
        /// </example>
        public async Task<T> GetServiceConfigAsync<T>(string service, string hosting, CancellationToken cancellationToken = default(CancellationToken)) where T : new()
        {
            var serviceDictionaryConfig = await GetServiceConfigAsync(service, hosting, cancellationToken);

            var config = new T();
            var configType = config.GetType().GetTypeInfo();

            var propertyBindingFlags = BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public;
            var properties = configType.GetProperties(propertyBindingFlags);

            foreach (var item in serviceDictionaryConfig)
            {
                var matchedProperty =
                    properties.FirstOrDefault(p => p.Name.Equals(item.Key, StringComparison.Ordinal)) ??
                    properties.FirstOrDefault(p => p.Name.Equals(item.Key, StringComparison.OrdinalIgnoreCase));

                matchedProperty?.SetValue(config, item.Value, null);
            }

            return config;
        }

        /// <summary>
        /// Gets service settings from Consul.
        /// </summary>
        /// <param name="service">The service name.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the service is not specified.</exception>
        /// <example>
        /// <code>
        /// var settings = GetServiceConfigAsync&lt;RedisSettings&gt;("Redis");
        /// </code>
        /// </example>
        public Task<Dictionary<string, string>> GetServiceConfigAsync(string service, CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetServiceConfigAsync(service, null, cancellationToken);
        }

        /// <summary>
        /// Gets service settings from Consul.
        /// </summary>
        /// <param name="service">The service name.</param>
        /// <param name="hosting">The hosting where the service is hosted. Optional.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the service is not specified.</exception>
        /// <example>
        /// <code>
        /// var settings = GetServiceConfigAsync&lt;RedisSettings&gt;("Redis", "azure");
        /// </code>
        /// </example>
        public async Task<Dictionary<string, string>> GetServiceConfigAsync(string service, string hosting, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(service)) throw new ArgumentNullException(nameof(service));

            var servicePrefix = GetConsulServiceKey(_environment, service, hosting);

            if (_useCache && ServiceSettingsCache.ContainsKey(servicePrefix)) return ServiceSettingsCache[servicePrefix];

            var settings = await GetServiceConfigAsyncFromConsul(servicePrefix, cancellationToken);

            return _useCache
                ? ServiceSettingsCache.GetOrAdd(servicePrefix, settings)
                : settings;
        }

        private async Task<Dictionary<string, string>> GetServiceConfigAsyncFromConsul(string servicePrefix, CancellationToken cancellationToken)
        {
            using (var client = _consulClientFactory(_address, _token))
            {
                var kvPairResult = await client.KV.List(servicePrefix, cancellationToken);

                var response = kvPairResult.Response;

                if (response == null || !response.Any()) return null;

                return response.ToDictionary(
                    kv => kv.Key.Replace(servicePrefix, String.Empty),
                    kv => kv.Value == null ? string.Empty : Encoding.UTF8.GetString(kv.Value, 0, kv.Value.Length));
            }
        }

        internal static string GetConsulServiceKey(string environment, string service, string hosting)
        {
            var serviceKey = string.IsNullOrEmpty(hosting)
                ? $"{environment}/{service}/"
                : $"{environment}/{hosting}/{service}/";

            return Normalize(serviceKey);
        }

        private static string Normalize(string value) => value?.ToLower();
    }
}
