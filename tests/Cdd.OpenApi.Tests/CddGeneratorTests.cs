using System.IO;
using System.Linq;
using Xunit;
using Cdd.OpenApi;

namespace Cdd.OpenApi.Tests
{
    public class CddGeneratorTests
    {
        [Fact]
        public void Test_CddGenerator_GeneratesCorrectly()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            try
            {
                var inputPath = Path.Combine(tempDir, "spec.json");
                File.WriteAllText(inputPath, @"
{
  ""openapi"": ""3.0.0"",
  ""info"": {
    ""title"": ""API"",
    ""version"": ""1.0.0""
  },
  ""servers"": [
    { ""url"": ""https://api.example.com/v1"" }
  ],
  ""paths"": {
    ""/pets"": {
      ""get"": {
        ""operationId"": ""getPets"",
        ""parameters"": [
          { ""name"": ""limit"", ""in"": ""query"", ""schema"": { ""type"": ""integer"" }, ""example"": 10 },
          { ""name"": ""isActive"", ""in"": ""query"", ""schema"": { ""type"": ""boolean"" }, ""examples"": { ""t"": { ""value"": true } } },
          { ""name"": ""tags"", ""in"": ""query"", ""schema"": { ""type"": ""array"", ""items"": { ""type"": ""string"" } } }
        ],
        ""responses"": {
          ""200"": { ""description"": ""OK"" }
        }
      },
      ""post"": {
        ""operationId"": ""createPet"",
        ""requestBody"": {
          ""content"": {
            ""application/json"": {
              ""schema"": { ""$ref"": ""#/components/schemas/Pet"" }
            }
          }
        },
        ""responses"": {
          ""201"": { ""description"": ""Created"" }
        },
        ""servers"": [
          { ""url"": ""https://custom-server.com"", ""description"": ""Custom"" }
        ],
        ""callbacks"": {
          ""myEvent"": {
            ""{$request.query.callbackUrl}"": {
              ""post"": {
                ""description"": ""Callback description"",
                ""responses"": { ""200"": { ""description"": ""OK"" } }
              }
            }
          }
        }
      },
      ""x-custom-method"": {
        ""operationId"": ""customMethod"",
        ""responses"": {
          ""200"": { ""description"": ""OK"" }
        }
      }
    }
  },
  ""components"": {
    ""schemas"": {
      ""Pet"": {
        ""type"": ""object"",
        ""properties"": {
          ""id"": { ""type"": ""integer"" }
        }
      }
    }
  }
}");

                var config = new CddConfig
                {
                    InputPath = inputPath,
                    OutputDir = tempDir,
                    NoGithubActions = false,
                    NoInstallablePackage = false,
                    CreateComposableTestsAndMocks = true
                };

                CddGenerator.GenerateAll(config);

                Assert.True(File.Exists(Path.Combine(tempDir, ".github", "workflows", "ci.yml")));
                Assert.True(File.Exists(Path.Combine(tempDir, "GeneratedProject.csproj")));
                Assert.True(File.Exists(Path.Combine(tempDir, "GeneratedProject.sln")));
                Assert.True(File.Exists(Path.Combine(tempDir, "Tests", "GeneratedProject.Tests.csproj")));
                Assert.True(File.Exists(Path.Combine(tempDir, "Tests", "IntegrationTests.cs")));

                CddGenerator.GenerateSdk(config);
                CddGenerator.GenerateSdkCli(config);
                CddGenerator.GenerateServer(config);

                var config2 = new CddConfig
                {
                    InputDir = tempDir,
                    OutputDir = tempDir
                };
                CddGenerator.GenerateAll(config2);
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void Test_CddGenerator_NoInputPaths_Throws()
        {
            var config = new CddConfig();
            Assert.Throws<System.ArgumentException>(() => CddGenerator.GenerateAll(config));
        }

        [Fact]
        public void Test_CddGenerator_InvalidInputPath_Throws()
        {
            var config = new CddConfig { InputPath = "does_not_exist.json" };
            Assert.Throws<FileNotFoundException>(() => CddGenerator.GenerateAll(config));
        }

        [Fact]
        public void Test_CddGenerator_InputPaths_Works()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            try
            {
                var inputPath = Path.Combine(tempDir, "spec.json");
                File.WriteAllText(inputPath, "{ \"openapi\": \"3.0.0\", \"paths\": {} }");

                var config = new CddConfig
                {
                    InputPaths = new System.Collections.Generic.List<string> { inputPath },
                    OutputDir = tempDir
                };

                CddGenerator.GenerateAll(config);
                Assert.True(File.Exists(Path.Combine(tempDir, "GeneratedProject.csproj")));
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void Test_CddGenerator_CatchBlocks()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                var inputPath = Path.GetTempFileName();
                File.WriteAllText(inputPath, "{ \"openapi\": \"3.0.0\", \"paths\": {} }");

                var config = new CddConfig
                {
                    InputPath = inputPath,
                    OutputDir = tempFile, // It's a file, so creating subdirs will throw and be caught
                    NoGithubActions = false,
                    CreateComposableTestsAndMocks = true
                };

                CddGenerator.GenerateAll(config);
                // Should not throw, catch blocks should be hit
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}



