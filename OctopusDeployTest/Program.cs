using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace OctopusDeployTest
{
    class ReleaseRetention
    {
        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var processor = serviceProvider.GetService<ReleaseProcessor>();
            if (args.Count() >= 1 && int.TryParse(args[0], out int numKeep))
            {
                processor.LogMessage($"Welcome to the Release Retention Tool. It will remove release artifacts and logs that are older than the first {numKeep} staging or production releases.");
                processor.NumKeep = numKeep;
               
                string output = processor.ProcessReleases();
                processor.LogMessage($"The run was successful. Here are the kept releases:");
                processor.LogMessage(output);
            }
            else
            {
                processor.LogError("Bad argument. Please pass a single number indicating the number of release artifacts to keep.", args);
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure => configure.AddConsole())
                .AddTransient<ReleaseProcessor>();
        }
    }
}
