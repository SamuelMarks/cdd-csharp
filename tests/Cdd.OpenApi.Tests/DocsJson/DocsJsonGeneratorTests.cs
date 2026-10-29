// <copyright file="DocsJsonGeneratorTests.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json;
using Xunit;
using Cdd.OpenApi.Models;
using Cdd.OpenApi.Models.Arazzo;
using Cdd.OpenApi.DocsJson;

namespace Cdd.OpenApi.Tests.DocsJson
{
    /// <summary>Verifies or exercises docs json generator tests.</summary>
    public class DocsJsonGeneratorTests
    {
        /// <summary>Verifies or exercises create sample document.</summary>
        /// <returns>The resulting value or task.</returns>
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
                                Tags = new List<string> { "myDocsTag" },
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

        /// <summary>Verifies or exercises generate with default flags includes all fields.</summary>
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

            Assert.Contains("var client = new MyDocsTagApiClient();", op.Code.WrapperStart);
            Assert.DoesNotContain("var client = new", op.Code.Snippet);
            Assert.Contains("int limit = 0;", op.Code.Snippet);
            Assert.Contains("var response = await client.getPetsAsync(limit);", op.Code.Snippet);
        }

        /// <summary>Verifies or exercises generate with no imports omits imports field.</summary>
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

        /// <summary>Verifies or exercises generate with no wrapping omits wrapper fields.</summary>
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
            Assert.DoesNotContain("var client = new", op.Code.Snippet);
            Assert.Contains("var response = await client.getPetsAsync(limit);", op.Code.Snippet);
        }

        /// <summary>Verifies or exercises generate with both no imports and no wrapping.</summary>
        [Fact]
        public void Generate_WithNoImportsAndNoWrapping_OmitsBoth()
        {
            var doc = CreateSampleDocument();
            var jsonStr = DocsJsonGenerator.Generate(doc, true, true);
            var parsed = JsonSerializer.Deserialize<List<DocsJsonOutput>>(jsonStr);

            Assert.NotNull(parsed);
            var op = Assert.Single(parsed[0].Operations);
            Assert.Null(op.Code.Imports);
            Assert.Null(op.Code.WrapperStart);
            Assert.Null(op.Code.WrapperEnd);
            Assert.Equal("int limit = 0;\nvar response = await client.getPetsAsync(limit);", op.Code.Snippet);
        }

        /// <summary>Verifies or exercises generate with request body adds dummy initialization.</summary>
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
                                Tags = new List<string>(),
                                RequestBody = new OpenApiRequestBody()
                            },
                            Put = new OpenApiOperation
                            {
                                OperationId = "updatePet",
                                Tags = new List<string> { string.Empty }
                            }
                        }
                    }
                }
            };

            var jsonStr = DocsJsonGenerator.Generate(doc, false, false);
            var parsed = JsonSerializer.Deserialize<List<DocsJsonOutput>>(jsonStr);

            Assert.NotNull(parsed);
            Assert.Equal(2, parsed[0].Operations.Count);
            var postOp = parsed[0].Operations.Find(o => o.Method == "POST")!;
            Assert.Contains("var requestBody = new object();", postOp.Code.Snippet);
            Assert.Contains("var response = await client.createPetAsync(requestBody);", postOp.Code.Snippet);
            Assert.Contains("var client = new DefaultApiClient();", postOp.Code.WrapperStart);

            var putOp = parsed[0].Operations.Find(o => o.Method == "PUT")!;
            Assert.Contains("var client = new DefaultApiClient();", putOp.Code.WrapperStart);
        }

        /// <summary>Verifies or exercises generate with webhooks and workflows.</summary>
        [Fact]
        public void Generate_WithWebhooksAndWorkflows_EmitsWebhookAndWorkflowSnippets()
        {
            var doc = new OpenApiDocument
            {
                Webhooks = new Dictionary<string, OpenApiPathItem>
                {
                    ["newPet"] = new OpenApiPathItem
                    {
                        Post = new OpenApiOperation
                        {
                            OperationId = "onNewPet",
                            Tags = new List<string> { "events" },
                            RequestBody = new OpenApiRequestBody()
                        }
                    }
                },
                Workflows = new List<ArazzoWorkflow>
                {
                    new ArazzoWorkflow
                    {
                        WorkflowId = "adopt-pet",
                        Inputs = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>
                            {
                                ["petId"] = new OpenApiSchema { Type = "string" }
                            }
                        }
                    },
                    new ArazzoWorkflow
                    {
                        WorkflowId = "ping-workflow",
                        Inputs = new OpenApiSchema { Type = "object" }
                    }
                }
            };

            var jsonStr = DocsJsonGenerator.Generate(doc, false, false);
            var parsed = JsonSerializer.Deserialize<List<DocsJsonOutput>>(jsonStr);

            Assert.NotNull(parsed);
            Assert.Equal(3, parsed[0].Operations.Count);

            var whOp = parsed[0].Operations[0];
            Assert.Equal("POST", whOp.Method);
            Assert.Equal("newPet", whOp.Path);
            Assert.Equal("onNewPet", whOp.OperationId);
            Assert.Contains("var client = new EventsApiClient();", whOp.Code.WrapperStart);
            Assert.Contains("var response = await client.onNewPetAsync(requestBody);", whOp.Code.Snippet);

            var wfOp = parsed[0].Operations[1];
            Assert.Equal("WORKFLOW", wfOp.Method);
            Assert.Equal("/workflows/adopt-pet", wfOp.Path);
            Assert.Equal("adopt-pet", wfOp.OperationId);
            Assert.NotNull(wfOp.Code.Imports);
            Assert.Contains("var workflows = new WorkflowsOrchestrator();", wfOp.Code.WrapperStart);
            Assert.Contains("var input = new AdoptPetInput();", wfOp.Code.Snippet);
            Assert.Contains("input.PetId = \"example\";", wfOp.Code.Snippet);
            Assert.Contains("var response = await workflows.AdoptPetAsync(input);", wfOp.Code.Snippet);
        }

        /// <summary>Verifies or exercises generate from arazzo document overload.</summary>
        [Fact]
        public void Generate_FromArazzoDocument_WithAndWithoutFlags()
        {
            var arazzoDoc = new ArazzoDocument
            {
                Workflows = new List<ArazzoWorkflow>
                {
                    new ArazzoWorkflow
                    {
                        WorkflowId = "checkoutOrder"
                    }
                }
            };

            var jsonWrapped = DocsJsonGenerator.Generate(arazzoDoc, false, false);
            var parsedWrapped = JsonSerializer.Deserialize<List<DocsJsonOutput>>(jsonWrapped);
            Assert.NotNull(parsedWrapped);
            var opWrapped = Assert.Single(parsedWrapped[0].Operations);
            Assert.NotNull(opWrapped.Code.Imports);
            Assert.NotNull(opWrapped.Code.WrapperStart);
            Assert.NotNull(opWrapped.Code.WrapperEnd);

            var jsonBare = DocsJsonGenerator.Generate(arazzoDoc, true, true);
            var parsedBare = JsonSerializer.Deserialize<List<DocsJsonOutput>>(jsonBare);
            Assert.NotNull(parsedBare);
            var opBare = Assert.Single(parsedBare[0].Operations);
            Assert.Null(opBare.Code.Imports);
            Assert.Null(opBare.Code.WrapperStart);
            Assert.Null(opBare.Code.WrapperEnd);
            Assert.Equal("var input = new CheckoutOrderInput();\nvar response = await workflows.CheckoutOrderAsync(input);", opBare.Code.Snippet);
        }

        /// <summary>Verifies or exercises generate without operation id uses fallback name.</summary>
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

        /// <summary>Verifies or exercises map type to csharp handles all types.</summary>
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

        /// <summary>Verifies or exercises generate empty paths returns empty operations.</summary>
        [Fact]
        public void Generate_EmptyPaths_ReturnsEmptyOperations()
        {
            var doc = new OpenApiDocument();
            var jsonStr = DocsJsonGenerator.Generate(doc, false, false);
            var parsed = JsonSerializer.Deserialize<List<DocsJsonOutput>>(jsonStr);

            Assert.NotNull(parsed);
            Assert.Empty(parsed[0].Operations);
        }
        /// <summary>Verifies or exercises generate parameters with null or empty name are ignored.</summary>
        [Fact]
        public void Generate_ParametersWithNullOrEmptyName_AreIgnored()
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
                                OperationId = "testEmptyParam",
                                Parameters = new List<OpenApiParameter>
                                {
                                    null!,
                                    new OpenApiParameter { Name = "" },
                                    new OpenApiParameter { Name = "validParam", Schema = new OpenApiSchema { Type = "string" } }
                                }
                            }
                        }
                    }
                }
            };
            var jsonStr = DocsJsonGenerator.Generate(doc, false, false);
            var parsed = JsonSerializer.Deserialize<List<DocsJsonOutput>>(jsonStr);
            var snippet = parsed![0].Operations[0].Code.Snippet;
            Assert.Contains("string validParam = \"example\";", snippet);
            Assert.DoesNotContain("null validParam", snippet);
        }
    }
}
