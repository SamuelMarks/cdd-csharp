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
                Assert.True(File.Exists(Path.Combine(tempDir, "src", "GeneratedProject", "GeneratedProject.csproj")));
                Assert.True(File.Exists(Path.Combine(tempDir, "GeneratedProject.sln")));
                Assert.True(File.Exists(Path.Combine(tempDir, "tests", "GeneratedProject.Tests", "GeneratedProject.Tests.csproj")));
                Assert.True(File.Exists(Path.Combine(tempDir, "tests", "GeneratedProject.Tests", "IntegrationTests.cs")));
                Assert.True(File.Exists(Path.Combine(tempDir, "README.md")));

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
                Assert.True(File.Exists(Path.Combine(tempDir, "src", "GeneratedProject", "GeneratedProject.csproj")));
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


        [Fact]
        public void Generate_CodeGenerator_Branches()
        {
            CodeGenerator.Generate(new Cdd.OpenApi.Models.OpenApiDocument());
        }


        [Fact]
        public void Test_CddGenerator_GenerateAll_NoInfo()
        {
            var tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
            System.IO.Directory.CreateDirectory(tempDir);
            try
            {
                var inputPath = System.IO.Path.Combine(tempDir, "spec.json");
                System.IO.File.WriteAllText(inputPath, @"{ ""openapi"": ""3.0.0"", ""paths"": {} }");

                var config = new CddConfig { InputPath = inputPath, OutputDir = tempDir, CreateComposableTestsAndMocks = true };
                CddGenerator.GenerateAll(config);
            }
            finally
            {
                System.IO.Directory.Delete(tempDir, true);
            }
        }



        [Fact]
        public void Test_CddGenerator_SafeCreateDirectory_EmptyPath()
        {
            var config = new CddConfig { OutputDir = "/", InputPaths = new System.Collections.Generic.List<string> { "spec.json" }, NoGithubActions = true, NoInstallablePackage = true };
            Assert.Throws<System.IO.FileNotFoundException>(() => CddGenerator.GenerateAll(config));
        }


        [Fact]
        public void Test_CddGenerator_EmptyInputPaths()
        {
            var config = new CddConfig { InputPaths = new System.Collections.Generic.List<string>() };
            Assert.Throws<System.ArgumentException>(() => CddGenerator.GenerateAll(config));
        }

        [Fact]
        public void Test_CddGenerator_GenerateAll_EmptyInputPaths()
        {
            var config = new CddConfig { InputPaths = new System.Collections.Generic.List<string>() };
            Assert.Throws<System.ArgumentException>(() => CddGenerator.GenerateAll(config));
        }

        [Fact]
        public void Test_CddGenerator_GenerateServer_Branch()
        {
            var config = new CddConfig { OutputDir = "something", InputPaths = new System.Collections.Generic.List<string> { "spec.json" }, CreateComposableTestsAndMocks = false };
            Assert.Throws<System.IO.FileNotFoundException>(() => CddGenerator.GenerateServer(config));
        }

        [Fact]
        public void Test_CddGenerator_NullDocInfo()
        {
            var tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
            System.IO.Directory.CreateDirectory(tempDir);
            try
            {
                var inputPath = System.IO.Path.Combine(tempDir, "spec.json");
                System.IO.File.WriteAllText(inputPath, @"{ ""openapi"": ""3.0.0"", ""paths"": {} }");

                var config = new CddConfig { InputPath = inputPath, OutputDir = tempDir };
                CddGenerator.GenerateAll(config);

                System.IO.File.WriteAllText(inputPath, @"{ ""openapi"": ""3.0.0"", ""info"": { ""title"": ""Title"" }, ""paths"": {} }");
                CddGenerator.GenerateAll(config);

                System.IO.File.WriteAllText(inputPath, @"{ ""openapi"": ""3.0.0"", ""info"": { ""contact"": { ""name"": ""Name"" } }, ""paths"": {} }");
                CddGenerator.GenerateAll(config);


                System.IO.File.WriteAllText(inputPath, @"{ ""openapi"": ""3.0.0"", ""info"": { ""title"": """", ""description"": ""   "", ""version"": """", ""contact"": { ""name"": ""   "" } }, ""paths"": {} }");
                CddGenerator.GenerateAll(config);

                System.IO.File.WriteAllText(inputPath, @"{ ""openapi"": ""3.0.0"", ""paths"": {} }");
                CddGenerator.GenerateAll(config);






                System.IO.File.WriteAllText(inputPath, @"{ ""openapi"": ""3.0.0"", ""info"": { ""title"": ""<script>"", ""description"": ""<script>"", ""version"": ""<script>"", ""contact"": { ""name"": ""Name <script>"" } }, ""paths"": {} }");
                CddGenerator.GenerateAll(config);


                var config2 = new CddConfig { InputPaths = new System.Collections.Generic.List<string>() };
                try { CddGenerator.GenerateAll(config2); } catch { }
            }
            finally
            {
                System.IO.Directory.Delete(tempDir, true);
            }
        }
        [Fact]
        public void SafeCreateDirectory_WithBackslash()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            // Create a path that starts with \ just for coverage of SafeCreateDirectory
            var config = new CddConfig
            {
                InputPath = tempDir, // We'll just put a valid thing so it parses
                OutputDir = tempDir.Replace("/", "\\"),
                NoInstallablePackage = true,
                NoGithubActions = true
            };
            File.WriteAllText(tempDir, "{ \"openapi\": \"3.0.0\", \"paths\": {} }");
            try
            {
                CddGenerator.GenerateAll(config);
            }
            finally
            {
                if (File.Exists(tempDir)) File.Delete(tempDir);
            }
        }
    }
}
