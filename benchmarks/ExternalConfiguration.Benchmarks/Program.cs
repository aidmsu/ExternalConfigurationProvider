using System;
using BenchmarkDotNet.Running;

namespace ExternalConfiguration.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<ExternalConfigurationProviderBenchmarks>();
            Console.ReadLine();
        }
    }
}
