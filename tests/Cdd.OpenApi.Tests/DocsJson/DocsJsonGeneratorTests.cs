using System.Collections.Generic;
using System.Text.Json;
using Xunit;
using Cdd.OpenApi.Models;
using Cdd.OpenApi.DocsJson;

namespace Cdd.OpenApi.Tests.DocsJson
{
    public class DocsJsonGeneratorTests
    {
        private OpenApiDocument CreateSampleDocument()
        {
            return new OpenApiDocument
            {
                Paths = new OpenApiPaths
                {
                    {
                        "/pets",
                        new OpenApiPathItem
                        {
                            Get = new OpenApiOperation
                            {
                                OperationId = "getPets",
                                Parameters = new List<OpenApiParameter>
                                {
                                    new OpenApiParameter
                                    {
                                        Name = "limit",
                                        Schema = new OpenApiSchema { Type = "integer" }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        [Fact]
        public void Generate_WithDefaultFlags_IncludesAllFields()
        {
            var doc = CreateSampleDocument();
            var jsonStr = DocsJsonGenerator.Generate(doc, false, false);
            var parsed = JsonSerializer.Deserialize<List<DocsJsonOutput>>(jsonStr);

            Assert.NotNull(parsed);
            var output = Assert.Single(parsed);
            Assert.Equal("csharp", output.Language);
            var op = Assert.Single(output.Operations);
            Assert.Equal("GET", op.Method);
            Assert.Equal("/pets", op.Path);
            Assert.Equal("getPets", op.OperationId);
            
            Assert.NotNull(op.Code.Imports);
            Assert.NotNull(op.Code.WrapperStart);
            Assert.NotNull(op.Code.WrapperEnd);
            Assert.NotNull(op.Code.Snippet);
            
            Assert.Contains("int limit = 0;", op.Code.Snippet);
        }

        [Fact]
        public void Generate_WithNoImports_OmitsImportsField()
        {
            var doc = CreateSampleDocument();
            var jsonStr = DocsJsonGenerator.Generate(doc, true, false);
            var parsed = JsonSerializer.Deserialize<List<DocsJsonOutput>>(jsonStr);

            Assert.NotNull(parsed);
            var op = Assert.Single(parsed[0].Operations);
            Assert.Null(op.Code.Imports);
            Assert.NotNull(op.Code.WrapperStart);
        }

        [Fact]
        public void Generate_WithNoWrapping_OmitsWrapperFields()
        {
            var doc = CreateSampleDocument();
            var jsonStr = DocsJsonGenerator.Generate(doc, false, true);
            var parsed = JsonSerializer.Deserialize<List<DocsJsonOutput>>(jsonStr);

            Assert.NotNull(parsed);
            var op = Assert.Single(parsed[0].Operations);
            Assert.NotNull(op.Code.Imports);
            Assert.Null(op.Code.WrapperStart);
            Assert.Null(op.Code.WrapperEnd);
        }

        [Fact]
        public void Generate_WithRequestBody_AddsDummyInitialization()
        {
            var doc = new OpenApiDocument
            {
                Paths = new OpenApiPaths
                {
                    {
                        "/pets",
                        new OpenApiPathItem
                        {
                            Post = new OpenApiOperation
                            {
                                OperationId = "createPet",
                                RequestBody = new OpenApiRequestBody()
                            }
                        }
                    }
                }
            };

            var jsonStr = DocsJsonGenerator.Generate(doc, false, false);
            var parsed = JsonSerializer.Deserialize<List<DocsJsonOutput>>(jsonStr);

            Assert.NotNull(parsed);
            var op = Assert.Single(parsed[0].Operations);
            Assert.Contains("var requestBody = new object();", op.Code.Snippet);
            Assert.Contains("createPetAsync(requestBody)", op.Code.Snippet);
        }

        [Fact]
        public void Generate_WithoutOperationId_UsesFallbackName()
        {
            var doc = new OpenApiDocument
            {
                Paths = new OpenApiPaths
                {
                    {
                        "/some/route/{id}",
                        new OpenApiPathItem
                        {
                            Get = new OpenApiOperation()
                        }
                    }
                }
            };

            var jsonStr = DocsJsonGenerator.Generate(doc, false, false);
            var parsed = JsonSerializer.Deserialize<List<DocsJsonOutput>>(jsonStr);

            Assert.NotNull(parsed);
            var op = Assert.Single(parsed[0].Operations);
            Assert.Equal("GET", op.Method);
            Assert.Contains("GETsomerouteidAsync", op.Code.Snippet);
        }
        
        [Fact]
        public void MapTypeToCSharp_HandlesAllTypes()
        {
            var doc = new OpenApiDocument
            {
                Paths = new OpenApiPaths
                {
                    {
                        "/test",
                        new OpenApiPathItem
                        {
                            Get = new OpenApiOperation
                            {
                                OperationId = "testTypes",
                                Parameters = new List<OpenApiParameter>
                                {
                                    new OpenApiParameter { Name = "p1", Schema = new OpenApiSchema { Type = "number" } },
                                    new OpenApiParameter { Name = "p2", Schema = new OpenApiSchema { Type = "boolean" } },
                                    new OpenApiParameter { Name = "p3", Schema = new OpenApiSchema { Type = "string" } },
                                    new OpenApiParameter { Name = "p4", Schema = new OpenApiSchema { Type = "unknown" } },
                                    new OpenApiParameter { Name = "p5" }, // null schema
                                    new OpenApiParameter { Name = "p6", Schema = new OpenApiSchema { Type = "object" } }
                                }
                            }
                        }
                    }
                }
            };

            var jsonStr = DocsJsonGenerator.Generate(doc, false, false);
            var parsed = JsonSerializer.Deserialize<List<DocsJsonOutput>>(jsonStr);

            Assert.NotNull(parsed);
            var snippet = parsed[0].Operations[0].Code.Snippet;
            Assert.Contains("double p1 = 0.0;", snippet);
            Assert.Contains("bool p2 = false;", snippet);
            Assert.Contains("string p3 = \"example\";", snippet);
            Assert.Contains("object p4 = null;", snippet);
            Assert.Contains("object p5 = null;", snippet);
            Assert.Contains("object p6 = null;", snippet);
        }

        [Fact]
        public void Generate_EmptyPaths_ReturnsEmptyOperations()
        {
            var doc = new OpenApiDocument();
            var jsonStr = DocsJsonGenerator.Generate(doc, false, false);
            var parsed = JsonSerializer.Deserialize<List<DocsJsonOutput>>(jsonStr);

            Assert.NotNull(parsed);
            Assert.Empty(parsed[0].Operations);
        }
    }
}