using System;
using System.Security.Cryptography;

namespace OctopusDeployTest.Models
{
    public class Deployment
    {
        public string Id { get; set; }
        public string EnvironmentId { get; set; }
        public string ReleaseId { get; set; }
        public DateTime DeployedAt { get; set; }

        public int DeleteLogs()
        {
            return RandomNumberGenerator.GetInt32(1024000000);
        }

        public int DeleteArtifacts()
        {
            return RandomNumberGenerator.GetInt32(1024000000);
        }
    }
}
