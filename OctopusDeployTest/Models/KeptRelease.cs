using System;

namespace OctopusDeployTest.Models
{
    public class KeptRelease
    {
        public string ReleaseId { get; set; }
        public string ProjectName { get; set; }
        public string EnvironmentName { get; set; }
        public string Version { get; set; }
        public DateTime DeployedAt { get; set; }
    }
}
