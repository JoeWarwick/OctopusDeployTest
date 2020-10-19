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
            var processor = serviceProvider.GetService<IReleaseProcessor>();
            if (args.Count() >= 1 && int.TryParse(args[0], out int numKeep))
            {
                processor.NumKeep = numKeep;
                processor.ProcessProjects();
                processor.ProcessDeployments();
                processor.ProcessEnvironments();
                processor.ProcessReleases();
            }
            else
            {
                processor.LogError("Bad argument. Please pass a single number indicating the number of release artifacts to keep.", args);
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure => configure.AddConsole())
                .AddTransient<IReleaseProcessor>();
        }
    }
}
