using System;

namespace OctopusDeployTest.Models
{
    public class KeptRelease
    {
        public string ReleaseId { get; set; }
        public string ProjectName { get; set; }
        public string EnvironmentName { get; set; }
        public DateTime LastDeployDate { get; set; }
        public string Version { get; set; }
    }
}
