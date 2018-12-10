#if NETSTANDARD1_5
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ExternalConfiguration
{
    /// <exclude />
    public static class ConsulConfigurationServiceCollectionExtensions
    {
        /// <summary>
        /// Configure Consul configuration provider services.
        /// Service configs will be cached. To avoid caching use overloaded method."/>
        /// </summary>
        public static IServiceCollection AddConsulConfigurationProvider(this IServiceCollection services, string environment, Action<ConsulConfig> configuration)
        {
            return AddConsulConfigurationProvider(services, environment, true, configuration);
        }

        /// <summary>
        /// Configure Consul configuration provider services.
        /// </summary>
        public static IServiceCollection AddConsulConfigurationProvider(this IServiceCollection services, string environment, bool useCache, Action<ConsulConfig> configuration)
        {
            if (string.IsNullOrEmpty(environment)) throw new ArgumentNullException(nameof(environment));

            var config = new ConsulConfig();
            configuration(config);

            services.TryAddSingleton<IExternalConfigurationStore>(serviceProvider => new ConsulConfigurationStore(config));

            services.TryAddSingleton<IExternalConfigurationProvider>(serviceProvider => new ExternalConfigurationProvider(
                serviceProvider.GetRequiredService<IExternalConfigurationStore>(), 
                environment,
                useCache));

            return services;
        }
    }
}
#endif