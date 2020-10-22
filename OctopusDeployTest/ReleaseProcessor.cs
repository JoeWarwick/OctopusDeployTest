using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OctopusDeployTest.Models;
using System.IO;
using System.Linq;

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


            // Project the needed moodels into a denormalised set
            var releasedeployments = releases.OrderByDescending(release => release.Created)
                .SelectMany(release => deployments.Where(deployment => deployment.ReleaseId == release.Id)
                .Select(deployment => new
                {
                    ReleaseId = release.Id,
                    ProjectName = projects.FirstOrDefault(project => project.Id == release.ProjectId)?.Name ?? release.ProjectId,
                    Environment = environments.FirstOrDefault(environment => environment.Id == deployment.EnvironmentId)?.Name,
                    Version = release.Version ?? "unversioned",
                    Deployment = deployment
                }));

            var intTotalStagingBytesReclaimed = 0;
            var intTotalProductionBytesReclaimed = 0;

            // Keep [NumKeep] Staging releases
            // ** delete non listed projects? E.G. Project-3 is not existent in the list of projects so when the project name is not found. Should it's deployment will also be deleted?
            var stagingset = releasedeployments.Where(rd => rd.Environment == "Staging")
                .GroupBy(rd => rd.ProjectName)
                .SelectMany(rd => rd.Take(NumKeep));            

            // Delete logs and artifacts of each deleted Staging release
            foreach(var rd in stagingset)
            {
                int szlogs = rd.Deployment.DeleteLogs();
                int szartifacts = rd.Deployment.DeleteArtifacts();
                intTotalStagingBytesReclaimed += szlogs + szartifacts;
            }
            
            // Keep [NumKeep] Production releases
            var productionset = releasedeployments.Where(rd => rd.Environment == "Production")
                .GroupBy(rd => rd.ProjectName)
                .SelectMany(rd => rd.Take(NumKeep));

            foreach (var rd in productionset)
            {
                int szlogs = rd.Deployment.DeleteLogs();
                int szartifacts = rd.Deployment.DeleteArtifacts();
                intTotalProductionBytesReclaimed += szlogs + szartifacts;
            }

            var keptReleases = stagingset.Select(ss => new KeptRelease
            {
                ReleaseId = ss.ReleaseId,
                EnvironmentName = ss.Environment,
                ProjectName = ss.ProjectName,
                Version = ss.Version,
                DeployedAt = ss.Deployment.DeployedAt

            }).Concat(productionset.Select(ps => new KeptRelease
            {
                ReleaseId = ps.ReleaseId,
                EnvironmentName = ps.Environment,
                ProjectName = ps.ProjectName,
                Version = ps.Version,
                DeployedAt = ps.Deployment.DeployedAt
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
