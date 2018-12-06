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
        public static IServiceCollection AddConsul(this IServiceCollection services, Action<ConsulOptions> configuration)
        {
            var options = new ConsulOptions();
            configuration(options);

            services.TryAddSingleton<IConfigurationProvider>(serviceProvider => new ConsulConfigurationProvider(options.Url, options.Token, options.Environment));

            return services;
        }
    }
}
#endif