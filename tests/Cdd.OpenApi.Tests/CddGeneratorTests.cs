// <copyright file="CddGeneratorTests.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.IO;
using System.Linq;
using Xunit;
using Cdd.OpenApi;

namespace Cdd.OpenApi.Tests
{
    /// <summary>Verifies or exercises cdd generator tests.</summary>
    public class CddGeneratorTests
    {
        /// <summary>Verifies or exercises test cdd generator generates correctly.</summary>
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
      ""put"": {
        ""operationId"": ""updatePet"",
        ""responses"": { ""200"": { ""description"": ""OK"" } }
      },
      ""delete"": {
        ""operationId"": ""deletePet"",
        ""responses"": { ""204"": { ""description"": ""No Content"" } }
      },
      ""options"": {
        ""operationId"": ""optionsPet"",
        ""responses"": { ""200"": { ""description"": ""OK"" } }
      },
      ""head"": {
        ""operationId"": ""headPet"",
        ""responses"": { ""200"": { ""description"": ""OK"" } }
      },
      ""patch"": {
        ""operationId"": ""patchPet"",
        ""responses"": { ""200"": { ""description"": ""OK"" } }
      },
      ""trace"": {
        ""operationId"": ""tracePet"",
        ""responses"": { ""200"": { ""description"": ""OK"" } }
      },
      ""x-custom-method"": {
        ""operationId"": ""customMethod"",
        ""responses"": {
          ""200"": { ""description"": ""OK"" }
        }
      }
    },
    ""/more"": {
      ""get"": {
        ""operationId"": ""getMore"",
        ""parameters"": [
          { ""name"": ""param1"", ""in"": ""path"" },
          { ""name"": ""param2"", ""in"": ""query"" }
        ],
        ""responses"": {
          ""200"": { ""description"": ""OK"", ""content"": { ""text/plain"": {} } }
        }
      },
      ""post"": {
        ""operationId"": ""postMore"",
        ""requestBody"": {
          ""content"": { ""text/plain"": {} }
        },
        ""responses"": {
          ""200"": {
            ""content"": {
              ""application/json"": {
                ""schema"": { ""type"": ""array"", ""items"": { ""type"": ""string"" } }
              }
            }
          }
        }
      },
      ""put"": {
        ""operationId"": ""putMore"",
        ""requestBody"": {
          ""content"": {
            ""application/json"": {
              ""schema"": { ""type"": ""array"", ""items"": { ""type"": ""string"" } }
            }
          }
        },
        ""responses"": {
          ""200"": {
            ""content"": {
              ""application/json"": {
                ""schema"": { ""$ref"": ""#/components/schemas/PetNullProps"" }
              }
            }
          }
        }
      },
      ""patch"": {
        ""operationId"": ""patchMore"",
        ""responses"": {
          ""200"": {
            ""content"": {
              ""application/json"": {
                ""schema"": { ""$ref"": ""#/components/schemas/PetStringId"" }
              }
            }
          }
        }
      },
      ""delete"": {
        ""operationId"": ""deleteNoResponse""
      }
    },
    ""/even-more"": {
      ""get"": {
        ""operationId"": ""getEvenMore"",
        ""responses"": {
          ""200"": { ""description"": ""OK, no content"" }
        }
      },
      ""post"": {
        ""operationId"": ""postEvenMore"",
        ""requestBody"": {
          ""content"": {
            ""application/json"": {
              ""schema"": { ""type"": ""array"", ""items"": { ""$ref"": ""#/components/schemas/Pet"" } }
            }
          }
        },
        ""responses"": {
          ""200"": {
            ""content"": {
              ""application/json"": {
                ""schema"": { ""type"": ""array"", ""items"": { ""$ref"": ""#/components/schemas/Pet"" } }
              }
            }
          }
        }
      },
      ""put"": {
        ""operationId"": ""putEvenMore"",
        ""requestBody"": {
          ""content"": {
            ""application/json"": {
              ""schema"": { ""type"": ""array"" }
            }
          }
        },
        ""responses"": {
          ""404"": { ""description"": ""Not found"" }
        }
      },
      ""patch"": {
        ""operationId"": ""patchEvenMore"",
        ""requestBody"": {},
        ""responses"": {
          ""200"": {
            ""content"": {}
          }
        }
      },
      ""options"": {
        ""operationId"": ""optionsEvenMore"",
        ""requestBody"": {
          ""content"": {
            ""application/json"": {
              ""schema"": { ""type"": ""array"" }
            }
          }
        },
        ""responses"": {
          ""200"": {
            ""content"": {
              ""application/json"": {
                ""schema"": { ""type"": ""array"" }
              }
            }
          }
        }
      },
      ""head"": {
        ""operationId"": ""headEvenMore"",
        ""requestBody"": {
          ""content"": {
            ""application/json"": {
              ""schema"": { ""type"": ""array"", ""items"": {} }
            }
          }
        },
        ""responses"": {
          ""200"": {
            ""content"": {
              ""application/json"": {
                ""schema"": { ""type"": ""array"", ""items"": {} }
              }
            }
          }
        }
      },
      ""trace"": {
        ""operationId"": ""traceEvenMore"",
        ""requestBody"": {
          ""content"": {
            ""application/json"": {
              ""schema"": { ""type"": ""string"" }
            }
          }
        },
        ""responses"": {
          ""200"": {
            ""content"": {
              ""application/json"": {
                ""schema"": { ""type"": ""string"" }
              }
            }
          }
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
      },
      ""PetStringId"": {
        ""type"": ""object"",
        ""properties"": {
          ""id"": { ""type"": ""string"" }
        }
      },
      ""PetUpperId"": {
        ""type"": ""object"",
        ""properties"": {
          ""Id"": { ""type"": ""integer"" }
        }
      },
      ""PetUpperStringId"": {
        ""type"": ""object"",
        ""properties"": {
          ""Id"": { ""type"": ""string"" }
        }
      },
      ""PetNoId"": {
        ""type"": ""object"",
        ""properties"": {
          ""name"": { ""type"": ""string"" }
        }
      },
      ""PetNullProps"": {
        ""type"": ""object""
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

        /// <summary>Verifies or exercises test cdd generator no input paths throws.</summary>
        [Fact]
        public void Test_CddGenerator_NoInputPaths_Throws()
        {
            var config = new CddConfig();
            Assert.Throws<System.ArgumentException>(() => CddGenerator.GenerateAll(config));
        }

        /// <summary>Verifies or exercises test cdd generator invalid input path throws.</summary>
        [Fact]
        public void Test_CddGenerator_InvalidInputPath_Throws()
        {
            var config = new CddConfig { InputPath = "does_not_exist.json" };
            Assert.Throws<FileNotFoundException>(() => CddGenerator.GenerateAll(config));
        }

        /// <summary>Verifies or exercises test cdd generator input paths works.</summary>
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

        /// <summary>Verifies or exercises test cdd generator catch blocks.</summary>
        [Fact]
        public void Test_CddGenerator_CatchBlocks()
        {
            var tempFile = Path.GetTempFileName();
            var inputPath = Path.GetTempFileName();
            try
            {
                File.WriteAllText(inputPath, "{ \"openapi\": \"3.0.0\", \"paths\": {} }");

                var config = new CddConfig
                {
                    InputPath = inputPath,
                    OutputDir = tempFile,
                    NoGithubActions = false,
                    CreateComposableTestsAndMocks = true
                };

                var ex = Assert.Throws<Cdd.OpenApi.Errors.CddException>(() => CddGenerator.GenerateAll(config));
                Assert.Equal(Cdd.OpenApi.Errors.CddErrorCode.IoError, ex.ErrorCode);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
                if (File.Exists(inputPath)) File.Delete(inputPath);
            }
        }


        /// <summary>Verifies or exercises generate code generator branches.</summary>
        [Fact]
        public void Generate_CodeGenerator_Branches()
        {
            var resEmpty = CodeGenerator.Generate(new Cdd.OpenApi.Models.OpenApiDocument());
            Assert.Empty(resEmpty);

            var docWithNullPaths = new Cdd.OpenApi.Models.OpenApiDocument
            {
                Paths = null,
                Components = new Cdd.OpenApi.Models.OpenApiComponents
                {
                    Schemas = new System.Collections.Generic.Dictionary<string, Cdd.OpenApi.Models.OpenApiSchema>()
                }
            };
            var resNullPaths = CodeGenerator.Generate(docWithNullPaths, "Generated", GenerateType.Server, false);
            Assert.NotEmpty(resNullPaths);
            Assert.Contains(resNullPaths, r => r.FileName == "src/Data/AppDbContext.cs");

            var docWithEmptyPaths = new Cdd.OpenApi.Models.OpenApiDocument
            {
                Paths = new Cdd.OpenApi.Models.OpenApiPaths(),
                Components = new Cdd.OpenApi.Models.OpenApiComponents
                {
                    Schemas = new System.Collections.Generic.Dictionary<string, Cdd.OpenApi.Models.OpenApiSchema>()
                }
            };
            var resEmptyPaths = CodeGenerator.Generate(docWithEmptyPaths, "Generated", GenerateType.Server, false);
            Assert.NotEmpty(resEmptyPaths);
            Assert.Contains(resEmptyPaths, r => r.FileName == "src/Data/AppDbContext.cs");
        }


        /// <summary>Verifies or exercises test cdd generator generate all no info.</summary>
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
                Assert.True(System.IO.File.Exists(System.IO.Path.Combine(tempDir, "GeneratedProject.sln")));
                var csproj = System.IO.File.ReadAllText(System.IO.Path.Combine(tempDir, "src", "GeneratedProject", "GeneratedProject.csproj"));
                Assert.Contains("<PackageId>GeneratedProject</PackageId>", csproj);
            }
            finally
            {
                System.IO.Directory.Delete(tempDir, true);
            }
        }



        /// <summary>Verifies or exercises test cdd generator safe create directory empty path.</summary>
        [Fact]
        public void Test_CddGenerator_SafeCreateDirectory_EmptyPath()
        {
            var config = new CddConfig { OutputDir = "/", InputPaths = new System.Collections.Generic.List<string> { "spec.json" }, NoGithubActions = true, NoInstallablePackage = true };
            Assert.Throws<System.IO.FileNotFoundException>(() => CddGenerator.GenerateAll(config));
        }


        /// <summary>Verifies or exercises test cdd generator empty input paths.</summary>
        [Fact]
        public void Test_CddGenerator_EmptyInputPaths()
        {
            var config = new CddConfig { InputPaths = new System.Collections.Generic.List<string>() };
            Assert.Throws<System.ArgumentException>(() => CddGenerator.GenerateAll(config));
        }

        /// <summary>Verifies or exercises test cdd generator generate all empty input paths.</summary>
        [Fact]
        public void Test_CddGenerator_GenerateAll_EmptyInputPaths()
        {
            var config = new CddConfig { InputPaths = new System.Collections.Generic.List<string>() };
            Assert.Throws<System.ArgumentException>(() => CddGenerator.GenerateAll(config));
        }

        /// <summary>Verifies or exercises test cdd generator generate server branch.</summary>
        [Fact]
        public void Test_CddGenerator_GenerateServer_Branch()
        {
            var config = new CddConfig { OutputDir = "something", InputPaths = new System.Collections.Generic.List<string> { "spec.json" }, CreateComposableTestsAndMocks = false };
            Assert.Throws<System.IO.FileNotFoundException>(() => CddGenerator.GenerateServer(config));
        }

        /// <summary>Verifies or exercises test cdd generator null doc info.</summary>
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
                var escapedCsproj = System.IO.File.ReadAllText(System.IO.Path.Combine(tempDir, "src", "GeneratedProject", "GeneratedProject.csproj"));
                Assert.Contains("&lt;script&gt;", escapedCsproj);

                var config2 = new CddConfig { InputPaths = new System.Collections.Generic.List<string>() };
                Assert.Throws<System.ArgumentException>(() => CddGenerator.GenerateAll(config2));

                var specA = System.IO.Path.Combine(tempDir, "specA.json");
                var specB = System.IO.Path.Combine(tempDir, "specB.json");
                System.IO.File.WriteAllText(specA, @"{ ""openapi"": ""3.2.0"", ""self"": ""self-uri"", ""jsonSchemaDialect"": ""dialect-uri"", ""basePath"": ""/v1"", ""servers"": [{ ""url"": ""http://localhost"" }], ""components"": { ""securitySchemes"": { ""SecA"": { ""type"": ""apiKey"" } } }, ""paths"": { ""/a"": { ""get"": { ""operationId"": ""GetA"" } } } }");
                System.IO.File.WriteAllText(specB, @"{ ""openapi"": ""3.2.0"", ""components"": { ""schemas"": { ""ModelB"": { ""type"": ""object"", ""properties"": { ""id"": { ""type"": ""string"" } } } }, ""securitySchemes"": { ""SecB"": { ""type"": ""apiKey"" } } }, ""paths"": { ""/b"": { ""get"": { ""operationId"": ""GetB"" } } } }");
                var specC = System.IO.Path.Combine(tempDir, "specC.json");
                System.IO.File.WriteAllText(specC, @"{ ""openapi"": ""3.2.0"", ""components"": { ""schemas"": { ""ModelA"": { ""type"": ""object"", ""properties"": { ""id"": { ""type"": ""string"" } } } } } }");
                var mergedConfig = new CddConfig
                {
                    InputPaths = new System.Collections.Generic.List<string> { specA, specB, specC },
                    OutputDir = tempDir,
                    NoInstallablePackage = true,
                    NoGithubActions = true
                };
                CddGenerator.GenerateAll(mergedConfig);
                Assert.True(System.IO.File.Exists(System.IO.Path.Combine(tempDir, "src", "Models", "ModelA.cs")));
                Assert.True(System.IO.File.Exists(System.IO.Path.Combine(tempDir, "src", "Models", "ModelB.cs")));
            }
            finally
            {
                System.IO.Directory.Delete(tempDir, true);
            }
        }
        /// <summary>Verifies or exercises safe create directory with backslash.</summary>
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
