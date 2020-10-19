using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
        public bool ProcessProjects()
        {
            JObject projects = ReadJson(ReleaseType.Project);
            return true;
        }
        public bool ProcessReleases()
        {
            JObject releases = ReadJson(ReleaseType.Release);
            return true;
        }

        public bool ProcessEnvironments()
        {
            JObject environments = ReadJson(ReleaseType.Environment);
            return true;
        }

        public bool ProcessDeployments()
        {
            JObject deployments = ReadJson(ReleaseType.Deployment);
            return true;
        }

        public void LogError(string message, string[] args)
        {
            this.logger.LogError(message, args);
        }

        public JObject ReadJson(ReleaseType rtype)
        {
            string json = null;
            switch (rtype)
            {
                case ReleaseType.Deployment:
                    json = File.ReadAllText("~\\pipelines\\deployments.json");
                    break;
                case ReleaseType.Environment:
                    json = File.ReadAllText("~\\pipelines\\environments.json");
                    break;
                case ReleaseType.Project:
                    json = File.ReadAllText("~\\pipelines\\projects.json");
                    break;
                case ReleaseType.Release:
                    json = File.ReadAllText("~\\pipelines\\releases.json");
                    break;
            }
            
            return JObject.Parse(json);
        }
    }
}
