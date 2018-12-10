﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ExternalConfiguration
{
    /// <summary>
    /// Provides methods to get service settings from external configuration store.
    /// </summary>
    public class ExternalConfigurationProvider : IExternalConfigurationProvider
    {
        private readonly string _environment;
        private readonly bool _useCache;

        private readonly IExternalConfigurationStore _store;

        internal readonly ConcurrentDictionary<string, Dictionary<string,string>> ServiceSettingsCache = new ConcurrentDictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// Initializes a new instance of the ExternalConfigurationProvider class that gets services settings from external store.
        /// </summary>
        /// <param name="store">External store.</param>
        /// <param name="config">Provider configuration.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the store is not specified.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown when the options.Environment is not specified.</exception>
        /// <example>
        /// <code>
        /// var provider = new ExternalConfigurationProvider("http://localhost:8500", "b1gs33cr3t", "staging");
        /// </code>
        /// </example>
        public ExternalConfigurationProvider(IExternalConfigurationStore store, ProviderConfig config)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            if (string.IsNullOrEmpty(config.Environment)) throw new ArgumentNullException(nameof(config.Environment));

            _useCache = config.UseCache;
            _environment = config.Environment;
        }

        /// <summary>
        /// Gets service settings from external store and converts them to the specified .NET type.
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
        /// Gets service settings from external store and converts them to the specified .NET type.
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
        /// Gets service settings from external store.
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
        /// Gets service settings from external store.
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

            var fullServiceName = GetFullServiceName(_environment, service, hosting);

            if (_useCache && ServiceSettingsCache.ContainsKey(fullServiceName)) return ServiceSettingsCache[fullServiceName];

            var settings = await _store.GetServiceConfigAsync(_environment, service, hosting, cancellationToken).ConfigureAwait(false);

            if (settings == null || !settings.Any()) return null;

            return _useCache
                ? ServiceSettingsCache.GetOrAdd(fullServiceName, settings)
                : settings;
        }

        internal static string GetFullServiceName(string environment, string service, string hosting)
        {
            return string.IsNullOrEmpty(hosting)
                ? $"{environment}/{service}/"
                : $"{environment}/{hosting}/{service}/";
        }
    }
}
