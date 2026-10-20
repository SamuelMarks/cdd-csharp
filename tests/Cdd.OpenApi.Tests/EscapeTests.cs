// <copyright file="EscapeTests.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.IO;
using Xunit;
using Cdd.OpenApi;

namespace Cdd.OpenApi.Tests
{
    /// <summary>Verifies or exercises escape tests.</summary>
    public class EscapeTests
    {
        /// <summary>Verifies or exercises generate all no escape required.</summary>
        [Fact]
        public void GenerateAll_NoEscapeRequired()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            try
            {
                var inputPath = Path.Combine(tempDir, "spec.json");
                File.WriteAllText(inputPath, @"{ ""openapi"": ""3.0.0"", ""info"": { ""title"": ""Title"", ""description"": ""Desc"", ""version"": ""1.0"", ""contact"": { ""name"": ""Name"" } }, ""paths"": {} }");

                var config = new CddConfig { InputPath = inputPath, OutputDir = tempDir, NoGithubActions = true, NoInstallablePackage = false };
                CddGenerator.GenerateAll(config);

                var csprojPath = Path.Combine(tempDir, "src", "GeneratedProject", "GeneratedProject.csproj");
                Assert.True(File.Exists(csprojPath));
                var csproj = File.ReadAllText(csprojPath);
                Assert.Contains("<PackageId>Title</PackageId>", csproj);
                Assert.Contains("<Description>Desc</Description>", csproj);
                Assert.Contains("<Authors>Name</Authors>", csproj);
                Assert.True(File.Exists(Path.Combine(tempDir, "README.md")));
                Assert.True(File.Exists(Path.Combine(tempDir, "GeneratedProject.sln")));
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}
