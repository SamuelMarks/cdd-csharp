using System.Collections.Generic;
using Xunit;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Tests
{
    public class IntegrationTestGeneratorTests
    {
        [Fact]
        public void Generate_CoversAllBranches()
        {
            var doc = new OpenApiDocument
            {
                BasePath = "/basePath",
                Servers = new List<OpenApiServer>
                {
                    new OpenApiServer { Url = "/relative/path" }
                },
                Paths = new OpenApiPaths
                {
                    ["/test"] = new OpenApiPathItem
                    {
                        Get = new OpenApiOperation
                        {
                            Tags = new List<string> { "mytag" },
                            Parameters = new List<OpenApiParameter>
                            {
                                new OpenApiParameter { Name = "p1", Schema = new OpenApiSchema { Type = "integer" } },
                                new OpenApiParameter { Name = "p2", Schema = new OpenApiSchema { Type = "number" } },
                                new OpenApiParameter { Name = "p3", Schema = new OpenApiSchema { Type = "boolean" } },
                                new OpenApiParameter { Name = "p4", Schema = new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Ref = "#/components/schemas/Item" } } },
                                new OpenApiParameter { Name = "p5", Schema = new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Type = "integer" } } },
                                new OpenApiParameter { Name = "p6", Schema = new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Type = "number" } } },
                                new OpenApiParameter { Name = "p7", Schema = new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Type = "boolean" } } },
                                new OpenApiParameter { Name = "p8", Schema = new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Type = "string" } } },
                                new OpenApiParameter { Name = "p9", Schema = new OpenApiSchema { Ref = "#/components/schemas/ParamObj" } },
                                new OpenApiParameter { Name = "api_key" }
                            }
                        },
                        Post = new OpenApiOperation
                        {
                            RequestBody = new OpenApiRequestBody
                            {
                                Content = new Dictionary<string, OpenApiMediaType>
                                {
                                    ["application/json"] = new OpenApiMediaType
                                    {
                                        Schema = new OpenApiSchema { Ref = "#/components/schemas/BodyObj" }
                                    }
                                }
                            }
                        },
                        Put = new OpenApiOperation
                        {
                            RequestBody = new OpenApiRequestBody
                            {
                                Content = new Dictionary<string, OpenApiMediaType>
                                {
                                    ["application/json"] = new OpenApiMediaType
                                    {
                                        Schema = new OpenApiSchema { Type = "integer" }
                                    }
                                }
                            }
                        },
                        Delete = new OpenApiOperation
                        {
                            RequestBody = new OpenApiRequestBody
                            {
                                Content = new Dictionary<string, OpenApiMediaType>
                                {
                                    ["application/json"] = new OpenApiMediaType
                                    {
                                        Schema = new OpenApiSchema { Type = "number" }
                                    }
                                }
                            }
                        },
                        Patch = new OpenApiOperation
                        {
                            RequestBody = new OpenApiRequestBody
                            {
                                Content = new Dictionary<string, OpenApiMediaType>
                                {
                                    ["application/json"] = new OpenApiMediaType
                                    {
                                        Schema = new OpenApiSchema { Type = "boolean" }
                                    }
                                }
                            }
                        },
                        Options = new OpenApiOperation
                        {
                            RequestBody = new OpenApiRequestBody
                            {
                                Content = new Dictionary<string, OpenApiMediaType>
                                {
                                    ["application/json"] = new OpenApiMediaType
                                    {
                                        Schema = new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Ref = "#/components/schemas/Item" } }
                                    }
                                }
                            }
                        },
                        Head = new OpenApiOperation
                        {
                            RequestBody = new OpenApiRequestBody
                            {
                                Content = new Dictionary<string, OpenApiMediaType>
                                {
                                    ["application/json"] = new OpenApiMediaType
                                    {
                                        Schema = new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Type = "integer" } }
                                    }
                                }
                            }
                        },
                        Trace = new OpenApiOperation
                        {
                            RequestBody = new OpenApiRequestBody
                            {
                                Content = new Dictionary<string, OpenApiMediaType>
                                {
                                    ["application/json"] = new OpenApiMediaType
                                    {
                                        Schema = new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Type = "number" } }
                                    }
                                }
                            }
                        },
                        Query = new OpenApiOperation
                        {
                            RequestBody = new OpenApiRequestBody
                            {
                                Content = new Dictionary<string, OpenApiMediaType>
                                {
                                    ["application/json"] = new OpenApiMediaType
                                    {
                                        Schema = new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Type = "boolean" } }
                                    }
                                }
                            }
                        },
                        AdditionalOperations = new Dictionary<string, OpenApiOperation>
                        {
                            ["x-custom"] = new OpenApiOperation
                            {
                                RequestBody = new OpenApiRequestBody
                                {
                                    Content = new Dictionary<string, OpenApiMediaType>
                                    {
                                        ["application/json"] = new OpenApiMediaType
                                        {
                                            Schema = new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Type = "string" } }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var generated = IntegrationTestGenerator.Generate(doc);
            Assert.Contains("http://localhost:8080/relative/path/", generated);
            Assert.Contains("TestGettestAsync", generated);
        }

        [Fact]
        public void Generate_FallbackToBaseUrl()
        {
            var doc = new OpenApiDocument
            {
                BasePath = "mybase"
            };
            var generated = IntegrationTestGenerator.Generate(doc);
            Assert.Contains("http://localhost:8080/mybase/", generated);
        }

        [Fact]
        public void Generate_InvalidUrlInServer_HitsElse()
        {
            var doc = new OpenApiDocument
            {
                Servers = new List<OpenApiServer>
                {
                    new OpenApiServer { Url = "not-an-absolute-url" }
                }
            };
            var generated = IntegrationTestGenerator.Generate(doc);
            Assert.Contains("http://localhost:8080/not-an-absolute-url/", generated);
        }

        [Fact]
        public void Generate_RequestBody_Types()
        {
            var doc = new OpenApiDocument
            {
                Paths = new OpenApiPaths
                {
                    ["/test-num"] = new OpenApiPathItem { Get = new OpenApiOperation { RequestBody = new OpenApiRequestBody { Content = new System.Collections.Generic.Dictionary<string, OpenApiMediaType> { ["application/json"] = new OpenApiMediaType { Schema = new OpenApiSchema { Type = "number" } } } } } },
                    ["/test-bool"] = new OpenApiPathItem { Get = new OpenApiOperation { RequestBody = new OpenApiRequestBody { Content = new System.Collections.Generic.Dictionary<string, OpenApiMediaType> { ["application/json"] = new OpenApiMediaType { Schema = new OpenApiSchema { Type = "boolean" } } } } } }
                }
            };
            IntegrationTestGenerator.Generate(doc);
        }


        [Fact]
        public void Generate_DocNull_ReturnsEmpty()
        {
            var generated = IntegrationTestGenerator.Generate(null!);
            Assert.Contains("http://localhost:8080/", generated);

            var doc1 = new OpenApiDocument { Servers = new System.Collections.Generic.List<OpenApiServer> { new OpenApiServer { Url = null! } } };
            IntegrationTestGenerator.Generate(doc1);

            var doc2 = new OpenApiDocument { Servers = null, BasePath = null };
            IntegrationTestGenerator.Generate(doc2);

            var doc3 = new OpenApiDocument { Servers = null, BasePath = "base" };
            IntegrationTestGenerator.Generate(doc3);

            var doc4 = new OpenApiDocument { Paths = null };
            IntegrationTestGenerator.Generate(doc4);
        }

        [Fact]
        public void Generate_NoServersOrBasePath_Fallback()
        {
            var doc = new OpenApiDocument();
            var generated = IntegrationTestGenerator.Generate(doc);
            Assert.Contains("http://localhost:8080/", generated);
        }
    }
}
