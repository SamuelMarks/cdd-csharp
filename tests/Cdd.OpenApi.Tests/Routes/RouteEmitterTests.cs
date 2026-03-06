using Xunit;
using Cdd.OpenApi.Models;
using Cdd.OpenApi.Routes;
using System.Collections.Generic;

namespace Cdd.OpenApi.Tests.Routes
{
    public class RouteEmitterTests
    {
        [Fact]
        public void ToInterface_ValidPaths_GeneratesCorrectInterface()
        {
            var paths = new OpenApiPaths
            {
                ["/pets"] = new OpenApiPathItem
                {
                    Get = new OpenApiOperation
                    {
                        OperationId = "GetPets",
                        Summary = "List all pets",
                        Parameters = new List<OpenApiParameter>
                        {
                            new OpenApiParameter { Name = "limit", In = "query", Schema = new OpenApiSchema { Type = "integer" } }
                        }
                    },
                    Post = new OpenApiOperation
                    {
                        OperationId = "CreatePet"
                    }
                }
            };

            var interfaceNode = Cdd.OpenApi.Routes.Emit.ToInterface("IPetsApi", paths);
            var code = interfaceNode.ToFullString();

            Assert.Contains("public interface IPetsApi", code);
            Assert.Contains("/// <summary>", code);
            Assert.Contains("/// List all pets", code);
            Assert.Contains("[HttpGet(\"/pets\")]", code);
            Assert.Contains("void GetPets([FromQuery] int limit);", code);

            Assert.Contains("[HttpPost(\"/pets\")]", code);
            Assert.Contains("void CreatePet();", code);
        }

        [Fact]
        public void ToInterface_GeneratesMethodNameWhenOperationIdMissing()
        {
            var paths = new OpenApiPaths
            {
                ["/store/order"] = new OpenApiPathItem
                {
                    Put = new OpenApiOperation()
                }
            };

            var interfaceNode = Cdd.OpenApi.Routes.Emit.ToInterface("IStoreApi", paths);
            var code = interfaceNode.ToFullString();

            Assert.Contains("void Putstoreorder();", code);
            Assert.Contains("[HttpPut(\"/store/order\")]", code);
        }

        [Fact]
        public void MapTypeToCSharp_MapsNumberToDouble()
        {
            var paths = new OpenApiPaths
            {
                ["/test"] = new OpenApiPathItem
                {
                    Get = new OpenApiOperation
                    {
                        OperationId = "TestMethod",
                        Parameters = new List<OpenApiParameter>
                        {
                            new OpenApiParameter { Name = "weight", Schema = new OpenApiSchema { Type = "number" } },
                            new OpenApiParameter { Name = "flag", Schema = new OpenApiSchema { Type = "boolean" } },
                            new OpenApiParameter { Name = "unknown", Schema = new OpenApiSchema { Type = "xyz" } }
                        }
                    }
                }
            };

            var interfaceNode = Cdd.OpenApi.Routes.Emit.ToInterface("ITestApi", paths);
            var code = interfaceNode.ToFullString();

            Assert.Contains("void TestMethod(double weight, bool flag, string unknown);", code);
        }

        [Fact]
        public void ToInterface_FullCoverage()
        {
            var paths = new OpenApiPaths
            {
                ["/full"] = new OpenApiPathItem
                {
                    Query = new OpenApiOperation
                    {
                        OperationId = "QueryFull",
                        Parameters = new List<OpenApiParameter>
                        {
                            new OpenApiParameter 
                            { 
                                Name = "param1", 
                                Description = "Param1 desc",
                                In = "path", 
                                Deprecated = true, 
                                AllowEmptyValue = true,
                                Example = "hello",
                                Examples = new Dictionary<string, OpenApiExample> { { "ex1", new OpenApiExample { Value = "val1" } } },
                                Style = "simple",
                                Explode = true,
                                AllowReserved = true,
                                Content = new Dictionary<string, OpenApiMediaType> { { "application/json", new OpenApiMediaType { Schema = new OpenApiSchema { Type = "string" } } } }
                            },
                            new OpenApiParameter 
                            { 
                                Name = "param2", 
                                In = "query", 
                                Example = 123,
                                Schema = new OpenApiSchema { Type = "integer" }
                            }
                        },
                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse
                            {
                                Description = "OK",
                                Content = new Dictionary<string, OpenApiMediaType> { { "application/json", new OpenApiMediaType { Schema = new OpenApiSchema { Type = "string" } } } },
                                Headers = new Dictionary<string, OpenApiHeader>
                                {
                                    ["X-RateLimit"] = new OpenApiHeader
                                    {
                                        Description = "Rate limit",
                                        Required = true,
                                        Deprecated = true,
                                        Example = "500",
                                        Examples = new Dictionary<string, OpenApiExample> { { "ex1", new OpenApiExample { Value = "100" } } },
                                        Style = "simple",
                                        Explode = true,
                                        Schema = new OpenApiSchema { Type = "integer" },
                                        Content = new Dictionary<string, OpenApiMediaType> { { "application/json", new OpenApiMediaType { Schema = new OpenApiSchema { Type = "integer" } } } }
                                    }
                                }
                            }
                        },
                        Servers = new List<OpenApiServer>
                        {
                            new OpenApiServer { Url = "http://server1", Description = "Server 1" }
                        },
                        Callbacks = new Dictionary<string, OpenApiCallback>
                        {
                            ["myCb"] = new OpenApiCallback
                            {
                                ["{$request.body#/url}"] = new OpenApiPathItem { Post = new OpenApiOperation { Description = "Cb Post" } }
                            }
                        }
                    },
                    AdditionalOperations = new Dictionary<string, OpenApiOperation>
                    {
                        ["PURGE"] = new OpenApiOperation { OperationId = "PurgeFull" }
                    }
                }
            };

            var interfaceNode = Cdd.OpenApi.Routes.Emit.ToInterface("IFullApi", paths);
            var code = interfaceNode.ToFullString();

            Assert.NotNull(code);
            Assert.Contains("HttpQuery", code);
            Assert.Contains("HttpPurge", code);
            Assert.Contains("server url", code);
            Assert.Contains("callback name", code);
            Assert.Contains("AllowReserved", code);
            Assert.Contains("application/json:integer", code);
        }
    }
}
