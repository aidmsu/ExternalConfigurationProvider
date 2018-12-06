﻿using System;
using Sdl.Configuration;

namespace Sdl.Net45ServiceExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var consulUrl = Properties.Settings.Default.ConsulUrl;
            var consulToken = Properties.Settings.Default.ConsulToken;

            var provider = new ConsulConfigurationProvider(consulUrl, consulToken, "debug");

            var mangoSettingsTask = provider.GetServiceConfigAsync<MangoConfig>("mango");

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