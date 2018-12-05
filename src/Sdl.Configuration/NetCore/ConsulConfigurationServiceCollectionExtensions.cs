#if NETSTANDARD1_3
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Sdl.Configuration
{

    public static class ConsulConfigurationServiceCollectionExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddConsul(this IServiceCollection services)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("consulsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("content/consulsettings.json", optional: true, reloadOnChange: true);

            var configuration = builder.Build();

            var consulUrl = configuration.GetSection("Consul:Url").Value;
            var consulToken = configuration.GetSection("Consul:Token").Value;

            return AddConsul(services, options =>
            {
                options.Token = consulToken;
                options.Url = consulUrl;

                // TODO: Pass environment.
                options.Environment = "Debug";
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddConsul(this IServiceCollection services, Action<ConsulOptions> configuration)
        {
            services.TryAddSingleton<IConfigurationProvider>(serviceProvider =>
            {
                var options = new ConsulOptions();
                configuration(options);

                return new ConsulConfigurationProvider(options.Url, options.Token, options.Environment);
            });

            return services;
        }
    }
}
#endif