﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using OctopusDeployTest.Models;

namespace OctopusDeployTest.Tests
{
    [TestClass]
    public class ReleaseProcessorTests
    {

        [TestMethod]
        public void TestJsonReadKeepOne()
        {
            // arrange
            var serviceCollection = new ServiceCollection();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var processor = serviceProvider.GetService<ReleaseProcessor>();
            processor.NumKeep = 1;

            var expected = new[]
            {
                new KeptRelease
                {
                    ReleaseId = "Release-7",
                    ProjectName = "Pet Shop",
                    EnvironmentName = "Staging",
                    Version = "1.0.3"
                },
                new KeptRelease
                {
                    ReleaseId = "Release-2",
                    ProjectName = "Random Quotes",
                    EnvironmentName = "Staging",
                    Version = "1.0.1"
                },
                new KeptRelease
                {
                    ReleaseId = "Release-8",
                    ProjectName = "Project - 3",
                    EnvironmentName = "Staging",
                    Version = "2.0.0"
                },
                new KeptRelease
                {
                    ReleaseId = "Release-8",
                    ProjectName = "Pet Shop",
                    EnvironmentName = "Production",
                    Version = "1.0.2"
                },
                new KeptRelease
                {
                    ReleaseId = "Release-1",
                    ProjectName = "Random Quotes",
                    EnvironmentName = "Production",
                    Version = "1.0.0"
                }
            };

            // act
            var resulted = processor.ProcessReleases();
            var actual = JsonConvert.SerializeObject(expected, Formatting.Indented);

            // assert
            Assert.AreEqual(expected, actual);
        }
    }
}
