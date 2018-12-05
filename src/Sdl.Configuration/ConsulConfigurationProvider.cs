using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Consul;

namespace LUV.Configuration
{
    public class ConsulConfigurationProvider : IConfigurationProvider
    {
        private readonly Uri _address;
        private readonly string _token;
        private readonly string _environment;

        public ConsulConfigurationProvider(string url, string token, string environment)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
            if (!Uri.TryCreate(url, UriKind.Absolute, out _address)) throw new ArgumentException("Bad url format.", nameof(url));
            if (string.IsNullOrEmpty(environment)) throw new ArgumentNullException(nameof(url));

            _token = token;
            _environment = Normalize(environment);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="hosting"></param>
        /// <returns></returns>
        public async Task<T> GetServiceConfigAsync<T>(string service, string hosting = null)
        {
            var servicePrefix = GetConsulKey(service, hosting);

            using (var client = GetConsulClient())
            {
                var kvPairResult = await client.KV.List(servicePrefix);

                var response = kvPairResult.Response;

                throw new NotImplementedException();
                //return response?.ToDictionary(
                //    kv => kv.Key.Replace($"{servicePrefix}/", String.Empty),
                //    kv => kv.Value == null ? string.Empty : Encoding.UTF8.GetString(kv.Value, 0, kv.Value.Length));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="hosting"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, string>> GetServiceConfigAsync(string service, string hosting = null)
        {
            var servicePrefix = GetConsulKey(service, hosting);

            using (var client = GetConsulClient())
            {
                var kvPairResult = await client.KV.List(servicePrefix);

                var response = kvPairResult.Response;

                return response?.ToDictionary(
                    kv => kv.Key.Replace($"{servicePrefix}/", String.Empty), 
                    kv=> kv.Value == null ? string.Empty : Encoding.UTF8.GetString(kv.Value, 0, kv.Value.Length));
            }
        }

        private ConsulClient GetConsulClient() => new ConsulClient(config =>
        {
            config.Address = _address;
            config.Token = _token;
        });

        private string GetConsulKey(string service, string hosting)
        {
            service = Normalize(service);
            hosting = Normalize(hosting);

            return string.IsNullOrEmpty(hosting)
                ? $"{_environment}/{service}/"
                : $"{_environment}/{hosting}/{service}/";
        }

        private string Normalize(string value) => value?.ToLower();
    }
}
