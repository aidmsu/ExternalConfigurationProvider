#if NETSTANDARD1_5
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Sdl.Configuration
{
    /// <exclude />
    public static class ConsulConfigurationServiceCollectionExtensions
    {
        /// <summary>
        /// Configure Consul configuration provider services.
        /// </summary>
        public static IServiceCollection AddConsulConfigurationProvider(this IServiceCollection services, Action<ConsulConfig> configuration)
        {
            var config = new ConsulConfig();
            configuration(config);

            services.TryAddSingleton<IConfigurationStore>(serviceProvider => new ConsulConfigurationStore(config));

            services.TryAddSingleton<IConfigurationProvider>(serviceProvider => new ExternalConfigurationProvider(
                serviceProvider.GetRequiredService<IConfigurationStore>(), 
                config));

            return services;
        }
    }
}
#endif