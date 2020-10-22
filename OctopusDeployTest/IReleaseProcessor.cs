using Newtonsoft.Json.Linq;
using OctopusDeployTest.Models;

namespace OctopusDeployTest
{
    public interface IReleaseProcessor
    {
        int NumKeep { get; set; }

        string ProcessReleases();  
        void LogError(string message, string[] args);
        T ReadJson<T>(ReleaseType rtype);
        void LogMessage(string message);
    }
}