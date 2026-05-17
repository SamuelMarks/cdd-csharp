using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using Cdd.OpenApi.Clients;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Tests.Clients
{
    public class EmitTests
    {
        [Fact]
        public void ToClient_ShouldGenerateValidClient()
        {
            var paths = new OpenApiPaths
            {
                { "/users", new OpenApiPathItem
                    {
                        Get = new OpenApiOperation
                        {
                            OperationId = "GetUsers",
                            Summary = "Gets all users"
                        }
                    }
                },
                { "/users/{id}", new OpenApiPathItem
                    {
                        Post = new OpenApiOperation
                        {
                            OperationId = "CreateUser",
                            Parameters = new List<OpenApiParameter>
                            {
                                new OpenApiParameter { Name = "id", Schema = new OpenApiSchema { Type = "integer" } }
                            }
                        },
                        Put = new OpenApiOperation(), // no operation id
                        Delete = new OpenApiOperation(),
                        Options = new OpenApiOperation(),
                        Head = new OpenApiOperation(),
                        Patch = new OpenApiOperation(),
                        Trace = new OpenApiOperation()
                    }
                }
            };

            var classNode = Cdd.OpenApi.Clients.Emit.ToClient("TestClient", paths);
            var code = classNode.ToFullString();

            Assert.Contains("class TestClient", code);
            Assert.Contains("private readonly System.Net.Http.HttpClient _httpClient;", code);
            Assert.Contains("public TestClient(System.Net.Http.HttpClient httpClient)", code);
            Assert.Contains("GetUsersAsync()", code);
            Assert.Contains("CreateUserAsync(int id)", code);
            Assert.Contains("PutusersidAsync()", code);

            // Should contain mapping types correctly
            // Let's test all mapped types:
            var allTypesPaths = new OpenApiPaths
            {
                { "/types", new OpenApiPathItem
                    {
                        Get = new OpenApiOperation
                        {
                            Parameters = new List<OpenApiParameter>
                            {
                                new OpenApiParameter { Name = "a", Schema = new OpenApiSchema { Type = "integer" } },
                                new OpenApiParameter { Name = "b", Schema = new OpenApiSchema { Type = "number" } },
                                new OpenApiParameter { Name = "c", Schema = new OpenApiSchema { Type = "boolean" } },
                                new OpenApiParameter { Name = "d", Schema = new OpenApiSchema { Type = "string" } },
                                new OpenApiParameter { Name = "e", Schema = new OpenApiSchema { Type = "unknown" } }
                            }
                        }
                    }
                }
            };
            var allTypesNode = Cdd.OpenApi.Clients.Emit.ToClient("TypeClient", allTypesPaths);
            var typesCode = allTypesNode.ToFullString();
            Assert.Contains("int a", typesCode);
            Assert.Contains("double b", typesCode);
            Assert.Contains("bool c", typesCode);
            Assert.Contains("string d", typesCode);
            Assert.Contains("string e", typesCode);
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
                                Content = new Dictionary<string, OpenApiMediaType> { { "application/json", new OpenApiMediaType { Schema = new OpenApiSchema { Type = "string" } } } },
                                Schema = new OpenApiSchema { Type = "string" }
                            },
                            new OpenApiParameter
                            {
                                Name = "param2",
                                In = "query",
                                Example = "123",
                                Schema = new OpenApiSchema { Type = "integer" }
                            },
                            new OpenApiParameter
                            {
                                Name = "param3",
                                In = "query",
                                Example = "12.3",
                                Schema = new OpenApiSchema { Type = "number" }
                            },
                            new OpenApiParameter
                            {
                                Name = "param4",
                                In = "query",
                                Example = "true",
                                Schema = new OpenApiSchema { Type = "boolean" }
                            },
                            new OpenApiParameter
                            {
                                Name = "param5",
                                In = "query",
                                Example = "hello",
                                Schema = new OpenApiSchema { Items = new OpenApiSchema { Ref = "#/components/schemas/MyModel" } }
                            }
                        },
                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse
                            {
                                Description = "OK",
                                Content = new Dictionary<string, OpenApiMediaType> { { "application/json", new OpenApiMediaType { Schema = new OpenApiSchema { Ref = "#/components/schemas/MyModel" } } } },
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
                                },
                                Links = new Dictionary<string, OpenApiLink>
                                {
                                    ["GetUsers"] = new OpenApiLink
                                    {
                                        OperationId = "GetUsers",
                                        Description = "Gets users",
                                        Parameters = new Dictionary<string, object> { { "userId", "$response.body#/id" } },
                                        RequestBody = "foo",
                                        Server = new OpenApiServer { Url = "http://server1" }
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
                        },
                        RequestBody = new OpenApiRequestBody
                        {
                            Content = new Dictionary<string, OpenApiMediaType>
                            {
                                ["application/json"] = new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema { Ref = "#/components/schemas/MyReqModel" }
                                }
                            }
                        }
                    },
                    AdditionalOperations = new Dictionary<string, OpenApiOperation>
                    {
                        ["PURGE"] = new OpenApiOperation
                        {
                            OperationId = "PurgeFull",
                            Responses = new OpenApiResponses
                            {
                                ["200"] = new OpenApiResponse
                                {
                                    Content = new Dictionary<string, OpenApiMediaType> { { "application/json", new OpenApiMediaType { Schema = new OpenApiSchema { Type = "integer" } } } }
                                }
                            },
                            RequestBody = new OpenApiRequestBody
                            {
                                Content = new Dictionary<string, OpenApiMediaType>
                                {
                                    ["application/json"] = new OpenApiMediaType
                                    {
                                        Schema = new OpenApiSchema { Items = new OpenApiSchema { Ref = "#/components/schemas/MyReqModel" } }
                                    }
                                }
                            }
                        },
                        ["MKCOL"] = new OpenApiOperation
                        {
                            OperationId = "MkcolFull",
                            Responses = new OpenApiResponses
                            {
                                ["200"] = new OpenApiResponse
                                {
                                    Content = new Dictionary<string, OpenApiMediaType> { { "application/json", new OpenApiMediaType { Schema = new OpenApiSchema { Items = new OpenApiSchema { Ref = "#/components/schemas/MyReqModel" } } } } }
                                }
                            },
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
                        }
                    }
                }
            };

            var interfaceNode = Cdd.OpenApi.Clients.Emit.ToClient("IFullApi", paths);
            var code = interfaceNode.ToFullString();

            Assert.NotNull(code);
            Assert.Contains("QueryFullAsync", code);
            Assert.Contains("PurgeFullAsync", code);
        }
        [Fact]
        public void ToClient_ExplodeArrayParameters_Omitted()
        {
            var paths = new OpenApiPaths
            {
                ["/explode-test"] = new OpenApiPathItem
                {
                    Get = new OpenApiOperation
                    {
                        OperationId = "GetExplode",
                        Parameters = new List<OpenApiParameter>
                        {
                            new OpenApiParameter
                            {
                                Name = "arrayParam",
                                In = "query",
                                Explode = true,
                                Schema = new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Type = "string" } }
                            },
                            new OpenApiParameter
                            {
                                Name = "stringParam",
                                In = "query",
                                Explode = true,
                                Schema = new OpenApiSchema { Type = "string" }
                            }
                        }
                    }
                }
            };
            var classNode = Cdd.OpenApi.Clients.Emit.ToClient("ExplodeClient", paths);
            var code = classNode.ToFullString();

            // stringParam should have [Explode]
            Assert.Contains("[Explode] string stringParam", code);
            // arrayParam should NOT have [Explode]
            Assert.DoesNotContain("[Explode] System.Collections.Generic.List<string> arrayParam", code);
            Assert.Contains("System.Collections.Generic.List<string> arrayParam", code);
        }
        [Fact]
        public void ToClient_ArrayReturnsAndHeaders()
        {
            var paths = new OpenApiPaths
            {
                { "/array", new OpenApiPathItem
                    {
                        Get = new OpenApiOperation
                        {
                            OperationId = "GetArrayRef",
                            Parameters = new List<OpenApiParameter>
                            {
                                new OpenApiParameter { Name = "myHeader", In = "header", Schema = new OpenApiSchema { Type = "string" } }
                            },
                            Responses = new OpenApiResponses
                            {
                                ["200"] = new OpenApiResponse
                                {
                                    Content = new Dictionary<string, OpenApiMediaType>
                                    {
                                        ["application/json"] = new OpenApiMediaType
                                        {
                                            Schema = new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Ref = "#/components/schemas/MyModel" } }
                                        }
                                    }
                                }
                            }
                        },
                        Post = new OpenApiOperation
                        {
                            OperationId = "GetArrayType",
                            Responses = new OpenApiResponses
                            {
                                ["200"] = new OpenApiResponse
                                {
                                    Content = new Dictionary<string, OpenApiMediaType>
                                    {
                                        ["application/json"] = new OpenApiMediaType
                                        {
                                            Schema = new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Type = "integer" } }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var classNode = Cdd.OpenApi.Clients.Emit.ToClient("ArrayClient", paths);
            var code = classNode.ToFullString();

            Assert.Contains("System.Collections.Generic.List<MyModel>", code);
            Assert.Contains("System.Collections.Generic.List<int>", code);
            Assert.Contains("request.Headers.Add(\"myHeader\", myHeader.ToString());", code);
        }
    }
}
