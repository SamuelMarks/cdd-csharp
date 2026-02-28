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
    }
}
