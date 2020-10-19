using Newtonsoft.Json.Linq;

namespace OctopusDeployTest
{
    public interface IReleaseProcessor
    {
        int NumKeep { get; set; }

        bool ProcessProjects();  
        void LogError(string message, string[] args);
        JObject ReadJson(ReleaseType rtype);
    }
}