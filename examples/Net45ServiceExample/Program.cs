using System;
using ExternalConfiguration;

namespace Net45ServiceExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var consulUrl = Properties.Settings.Default.ConsulUrl;
            var consulToken = Properties.Settings.Default.ConsulToken;

            var consulConfig = new ConsulConfig
            {
                Url = consulUrl,
                Token = consulToken
            };

            var store = new ConsulConfigurationStore(consulConfig.Url, consulConfig.Token);
            var provider = new ExternalConfigurationProvider(store, "debug");

            var mangoSettingsTask = provider.GetServiceSettingsAsync<MangoConfig>("mango");

            var mangoSettings = mangoSettingsTask.Result;

            Console.WriteLine($"ApiKey : {mangoSettings.ApiKey}");
            Console.WriteLine($"ApiSignature : {mangoSettings.ApiSignature}");

            Console.ReadLine();
        }

        public class MangoConfig
        {
            public string ApiKey { get; set; }
            public string ApiSignature { get; set; }
        }
    }
}
