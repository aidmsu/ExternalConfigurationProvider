using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ExternalConfiguration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NetCoreServiceExample
{
    class Program
    {
        private static IExternalConfigurationProvider _configurationProvider;

        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var configuration = builder.Build();

            var consulUrl = configuration.GetSection("Consul:url").Value;
            var consulToken = configuration.GetSection("Consul:token").Value;

            var serviceProvider = new ServiceCollection()
                .AddConsulConfigurationProvider("debug", consulConfig =>
                {
                    consulConfig.Url = consulUrl;
                    consulConfig.Token = consulToken;
                })
                .BuildServiceProvider();

            _configurationProvider = serviceProvider.GetService<IExternalConfigurationProvider>();

            var mangoKey = configuration.GetSection("Consul:MangoKey").Value;

            var mangoConfig = await _configurationProvider.GetServiceSettingsAsync(mangoKey);

            LogConfig(mangoConfig, "mango");

            Console.WriteLine();
            Console.ReadLine();
        }

        private static void LogConfig(Dictionary<string, string> config, string serviceName)
        {
            Console.WriteLine(serviceName);
            Console.WriteLine();

            if (config == null)
            {
                Console.WriteLine("Config was not found.");
                return;
            }

            foreach (var settings in config)
            {
                Console.WriteLine($"{settings.Key} {settings.Value}");
            }
        }
    }
}
