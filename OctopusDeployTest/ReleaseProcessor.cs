using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OctopusDeployTest.Models;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Environment = OctopusDeployTest.Models.Environment;

namespace OctopusDeployTest
{
    public class ReleaseProcessor : IReleaseProcessor
    {
        
        private readonly ILogger<ReleaseProcessor> logger;
        public int NumKeep { get; set; }

        public ReleaseProcessor(ILogger<ReleaseProcessor> logger)
        {
            this.logger = logger;
        }
        
        public string ProcessReleases()
        {
            var environments = ReadJson<Environment[]>(ReleaseType.Environment);
            var projects = ReadJson<Project[]>(ReleaseType.Project);
            var releases = ReadJson<Release[]>(ReleaseType.Release);
            var deployments = ReadJson<Deployment[]>(ReleaseType.Deployment);


            // Project the needed moodels joined
            var releasedeployments = releases.Select(release => new
            {
                ReleaseId = release.Id,
                ProjectName = projects.FirstOrDefault(project => project.Id == release.ProjectId)?.Name,
                Version = release.Version ?? "unversioned",
                Deployments = deployments.Where(deployment => deployment.ReleaseId == release.Id)
                    .OrderBy(deployment => deployment.DeployedAt)
                    .Select(deployment => new
                    {
                        Environment = environments.FirstOrDefault(environment => environment.Id == deployment.EnvironmentId)?.Name,
                        Deployment = deployment
                    }),
                LatestDeploymentAt = deployments.Where(deployment => deployment.ReleaseId == release.Id)
                    .OrderBy(deployment => deployment.DeployedAt).FirstOrDefault()?.DeployedAt,
            });

            var intTotalStagingBytesReclaimed = 0;
            var intTotalProductionBytesReclaimed = 0;

            // Keep [NumKeep] Staging releases
            var stagingset = releasedeployments.Where(rd => rd.Deployments.Any(deployment => deployment.Environment == "Staging"))
                .Take(NumKeep);            

            // Delete logs and artifacts of each deleted Staging release
            foreach(var rd in stagingset)
            {
                foreach(var deployment in rd.Deployments.Where(deployment => deployment.Environment == "Staging"))
                {
                    int szlogs = deployment.Deployment.DeleteLogs();
                    int szartifacts = deployment.Deployment.DeleteArtifacts();
                    intTotalStagingBytesReclaimed += szlogs + szartifacts;
                }
            }

            var keptReleases = stagingset.Select(ss => new KeptRelease
            {
                ReleaseId = ss.ReleaseId,
                EnvironmentName = "Staging",
                ProjectName = ss.ProjectName,
                LastDeployDate = ss.LatestDeploymentAt??DateTime.MinValue,
                Version = ss.Version                
            });

            // Keep [NumKeep] Production releases
            var productionset = releasedeployments.Where(rd => rd.Deployments.Any(deployment => deployment.Environment == "Production"))
                .Take(NumKeep);

            foreach (var rd in productionset)
            {
                foreach (var deployment in rd.Deployments.Where(deployment => deployment.Environment == "Production"))
                {
                    int szlogs = deployment.Deployment.DeleteLogs();
                    int szartifacts = deployment.Deployment.DeleteArtifacts();
                    intTotalProductionBytesReclaimed += szlogs + szartifacts;
                }
            }
   
            keptReleases = keptReleases.Concat(productionset.Select(ss => new KeptRelease
            {
                ReleaseId = ss.ReleaseId,
                EnvironmentName = "Production",
                ProjectName = ss.ProjectName,
                LastDeployDate = ss.LatestDeploymentAt??DateTime.MinValue,
                Version = ss.Version
            }));

            LogMessage($"Deleted a total of {intTotalStagingBytesReclaimed} bytes from Staging deployments.");
            LogMessage($"Deleted a totoal of {intTotalStagingBytesReclaimed} bytes from Production deployments.");


            return JsonConvert.SerializeObject(keptReleases, Formatting.Indented);
        }

        public void LogError(string message, string[] args)
        {
            this.logger.LogError(message, args);
        }

        public void LogMessage(string message)
        {
            this.logger.LogInformation(message);
        }

        public T ReadJson<T>(ReleaseType rtype)
        {
            string json = null;
            switch (rtype)
            {
                case ReleaseType.Deployment:
                    json = File.ReadAllText("pipelines\\deployments.json");
                    break;
                case ReleaseType.Environment:
                    json = File.ReadAllText("pipelines\\environments.json");
                    break;
                case ReleaseType.Project:
                    json = File.ReadAllText("pipelines\\projects.json");
                    break;
                case ReleaseType.Release:
                    json = File.ReadAllText("pipelines\\releases.json");
                    break;
            }
            
            return JsonConvert.DeserializeObject<T>(json); ;
        }
    }
}
