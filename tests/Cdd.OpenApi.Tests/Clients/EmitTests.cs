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

            var interfaceNode = Cdd.OpenApi.Clients.Emit.ToClient("IFullApi", paths);
            var code = interfaceNode.ToFullString();

            Assert.NotNull(code);
            Assert.Contains("QueryFullAsync", code);
            Assert.Contains("PurgeFullAsync", code);
            Assert.Contains("server url", code);
            Assert.Contains("callback name", code);
            Assert.Contains("AllowReserved", code);
            Assert.Contains("application/json:integer", code);
        }
    }
}
