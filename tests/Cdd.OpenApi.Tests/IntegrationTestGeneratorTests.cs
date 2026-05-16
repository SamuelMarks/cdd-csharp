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
    }
}
