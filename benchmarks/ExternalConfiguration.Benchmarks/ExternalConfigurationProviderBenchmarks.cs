using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Moq;

namespace ExternalConfiguration.Benchmarks
{
    public class ExternalConfigurationProviderBenchmarks
    {
        private ExternalConfigurationProvider _provider;

        [GlobalSetup]
        public void SetUp()
        {
            var store = new Mock<IExternalConfigurationStore>().Object;
            _provider = new ExternalConfigurationProvider(store, "staging", useCache: true);

            _provider.ServiceSettingsCache["staging/redis/"] = new Dictionary<string, string>{{"key1", "value1"}};
        }

        [Benchmark]
        public Task<Dictionary<string, string>> GetServiceSettingsAsyncFromCache()
        {
            return _provider.GetServiceSettingsAsync("redis");
        }
    }
}
