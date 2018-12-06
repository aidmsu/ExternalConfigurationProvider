using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Consul;

namespace Sdl.Configuration
{
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
        private readonly Func<Uri, string, IConsulClient> _consulClientFactory;

        public ConsulConfigurationProvider(string url, string token, string environment) 
            : this(url, token, environment, DefaultConsulClientFactory)
        {
        }

        internal ConsulConfigurationProvider(string url, string token, string environment, Func<Uri, string, IConsulClient> consulClientFactory)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
            if (!Uri.TryCreate(url, UriKind.Absolute, out _address)) throw new ArgumentException("Bad url format.", nameof(url));
            if (string.IsNullOrEmpty(environment)) throw new ArgumentNullException(nameof(environment));

            _token = token;
            _environment = Normalize(environment);

            _consulClientFactory = consulClientFactory;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="hosting"></param>
        /// <returns></returns>
        public async Task<T> GetServiceConfigAsync<T>(string service, string hosting = null) where T : new()
        {
            var dictionary = await GetServiceConfigAsync(service, hosting);

            var config = new T();
            var configType = config.GetType().GetTypeInfo();

            var propertyBindingFlags = BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public;
            var properties = configType.GetProperties(propertyBindingFlags);

            foreach (var item in dictionary)
            {
                var matchedProperty =
                    properties.FirstOrDefault(p => p.Name.Equals(item.Key, StringComparison.Ordinal)) ??
                    properties.FirstOrDefault(p => p.Name.Equals(item.Key, StringComparison.OrdinalIgnoreCase));

                matchedProperty?.SetValue(config, item.Value, null);
            }

            return config;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="hosting"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, string>> GetServiceConfigAsync(string service, string hosting = null)
        {
            if (string.IsNullOrEmpty(service)) throw new ArgumentNullException(nameof(service));

            var servicePrefix = GetConsulServiceKey(_environment, service, hosting);

            using (var client = _consulClientFactory(_address, _token))
            {
                var kvPairResult = await client.KV.List(servicePrefix);

                var response = kvPairResult.Response;

                if (response == null || !response.Any()) return null;

                return response.ToDictionary(
                    kv => kv.Key.Replace($"{servicePrefix}", String.Empty), 
                    kv=> kv.Value == null ? string.Empty : Encoding.UTF8.GetString(kv.Value, 0, kv.Value.Length));
            }
        }

        internal static string GetConsulServiceKey(string environment, string service, string hosting)
        {
            environment = Normalize(environment);
            service = Normalize(service);
            hosting = Normalize(hosting);

            return string.IsNullOrEmpty(hosting)
                ? $"{environment}/{service}/"
                : $"{environment}/{hosting}/{service}/";
        }

        private static string Normalize(string value) => value?.ToLower();
    }
}
