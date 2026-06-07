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
            var code = interfaceNode.ToFormattedString();

            AssertHelper.ContainsNoWhitespace("public interface IPetsApi", code);
            AssertHelper.ContainsNoWhitespace("/// <summary>", code);
            AssertHelper.ContainsNoWhitespace("/// List all pets", code);
            AssertHelper.ContainsNoWhitespace("[HttpGet(\"/pets\")]", code);
            AssertHelper.ContainsNoWhitespace("void GetPets([FromQuery] int limit);", code);

            AssertHelper.ContainsNoWhitespace("[HttpPost(\"/pets\")]", code);
            AssertHelper.ContainsNoWhitespace("void CreatePet();", code);

            // Assert MCP endpoints are generated
            Assert.Contains("McpSseEndpoint", code);
            Assert.Contains("McpMessageEndpoint", code);
        }

        [Fact]
        public void ToInterface_WithRequestBodyAndEmptySchema_GeneratesIActionResult()
        {
            var paths = new OpenApiPaths
            {
                ["/upload"] = new OpenApiPathItem
                {
                    Post = new OpenApiOperation
                    {
                        OperationId = "UploadData",
                        RequestBody = new OpenApiRequestBody
                        {
                            Content = new Dictionary<string, OpenApiMediaType>
                            {
                                ["application/json"] = new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema { Ref = "#/components/schemas/MyModel" }
                                }
                            }
                        },
                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse
                            {
                                Content = new Dictionary<string, OpenApiMediaType>
                                {
                                    ["application/json"] = new OpenApiMediaType { Schema = new OpenApiSchema() } // empty schema
                                }
                            },
                            ["201"] = new OpenApiResponse
                            {
                                Description = "Created" // no content
                            }
                        }
                    }
                }
            };

            var interfaceNode = Cdd.OpenApi.Routes.Emit.ToInterface("IUploadApi", paths);
            var code = interfaceNode.ToFormattedString();

            AssertHelper.ContainsNoWhitespace("System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.IActionResult> UploadData([FromBody] object body);", code);
        }

        [Fact]
        public void ToInterface_WithResponseNoContent_GeneratesIActionResult()
        {
            var paths = new OpenApiPaths
            {
                ["/nocontent"] = new OpenApiPathItem
                {
                    Get = new OpenApiOperation
                    {
                        OperationId = "GetNoContent",
                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse
                            {
                                Description = "OK"
                            }
                        }
                    }
                }
            };

            var interfaceNode = Cdd.OpenApi.Routes.Emit.ToInterface("INoContentApi", paths);
            var code = interfaceNode.ToFormattedString();

            AssertHelper.ContainsNoWhitespace("System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.IActionResult> GetNoContent();", code);
        }

        [Fact]
        public void ToInterface_WithRequestBodyAndNoType_GeneratesObject()
        {
            var paths = new OpenApiPaths
            {
                ["/upload"] = new OpenApiPathItem
                {
                    Post = new OpenApiOperation
                    {
                        OperationId = "UploadDataObj",
                        RequestBody = new OpenApiRequestBody
                        {
                            Content = new Dictionary<string, OpenApiMediaType>
                            {
                                ["application/json"] = new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema()
                                }
                            }
                        }
                    }
                }
            };

            var interfaceNode = Cdd.OpenApi.Routes.Emit.ToInterface("IUploadApi2", paths);
            var code = interfaceNode.ToFormattedString();

            AssertHelper.ContainsNoWhitespace("void UploadDataObj([FromBody] object body);", code);
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
            var code = interfaceNode.ToFormattedString();

            AssertHelper.ContainsNoWhitespace("void Putstoreorder();", code);
            AssertHelper.ContainsNoWhitespace("[HttpPut(\"/store/order\")]", code);
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
            var code = interfaceNode.ToFormattedString();

            AssertHelper.ContainsNoWhitespace("void TestMethod(double weight, bool flag, string unknown);", code);
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
            var code = interfaceNode.ToFormattedString();

            Assert.NotNull(code);
            AssertHelper.ContainsNoWhitespace("HttpQuery", code);
            AssertHelper.ContainsNoWhitespace("HttpPurge", code);
            AssertHelper.ContainsNoWhitespace("server url", code);
            AssertHelper.ContainsNoWhitespace("callback name", code);
            AssertHelper.ContainsNoWhitespace("AllowReserved", code);
            AssertHelper.ContainsNoWhitespace("application/json:integer", code);
        }
        [Fact]
        public void ToInterface_ExamplesParsing_Coverage()
        {
            var paths = new OpenApiPaths
            {
                ["/ex"] = new OpenApiPathItem
                {
                    Get = new OpenApiOperation
                    {
                        OperationId = "ExMethod",
                        Parameters = new List<OpenApiParameter>
                        {
                            new OpenApiParameter { Name = "p1", Example = "10", Schema = new OpenApiSchema { Type = "integer" } },
                            new OpenApiParameter { Name = "p2", Example = "10.5", Schema = new OpenApiSchema { Type = "number" } },
                            new OpenApiParameter { Name = "p3", Example = "invalid", Schema = new OpenApiSchema { Type = "integer" } },
                            new OpenApiParameter { Name = "p4", Example = "false", Schema = new OpenApiSchema { Type = "boolean" } },
                            new OpenApiParameter { Name = "p5", Example = "true", Schema = new OpenApiSchema { Type = "boolean" } },
                            new OpenApiParameter { Name = "p6", Example = null, Schema = new OpenApiSchema { Type = "string" } } // To hit the es == null branch but actually it will just not go into the if (p.Example != null) branch.
                        }
                    }
                }
            };
            var interfaceNode = Cdd.OpenApi.Routes.Emit.ToInterface("IExApi", paths);
            var code = interfaceNode.ToFormattedString();
            Assert.Contains("p1=10", code.Replace(" ", ""));
            Assert.Contains("p2=10.5", code.Replace(" ", ""));
            Assert.Contains("p4=false", code.Replace(" ", ""));
            Assert.Contains("p5=true", code.Replace(" ", ""));
        }

        [Fact]
        public void ToInterface_Emit_MissingBranches_Coverage()
        {
            var paths = new OpenApiPaths
            {
                ["/missing-branches"] = new OpenApiPathItem
                {
                    Get = new OpenApiOperation
                    {
                        OperationId = "GetMissing",
                        Responses = new OpenApiResponses
                        {
                            ["400"] = new OpenApiResponse
                            {
                                Content = new Dictionary<string, OpenApiMediaType>
                                {
                                    ["application/json"] = new OpenApiMediaType
                                    {
                                        Schema = new OpenApiSchema { Type = "string" }
                                    }
                                }
                            },
                            ["200"] = new OpenApiResponse
                            {
                                Content = new Dictionary<string, OpenApiMediaType>
                                {
                                    ["application/xml"] = new OpenApiMediaType
                                    {
                                        Schema = new OpenApiSchema { Type = "string" }
                                    }
                                }
                            }
                        },
                        Parameters = new List<OpenApiParameter>
                        {
                            new OpenApiParameter
                            {
                                Name = "arrayNoItems",
                                Schema = new OpenApiSchema { Type = "array", Items = null }
                            },
                            new OpenApiParameter
                            {
                                Name = "arrayItemsNoRefNoType",
                                Schema = new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Ref = null, Type = null } }
                            },
                            new OpenApiParameter
                            {
                                Name = "noTypeButItemsNoRef",
                                Schema = new OpenApiSchema { Type = null, Items = new OpenApiSchema { Ref = null } }
                            },
                            new OpenApiParameter
                            {
                                Name = "noTypeItemsNull",
                                Schema = new OpenApiSchema { Type = null, Items = null }
                            },
                            new OpenApiParameter
                            {
                                Name = "exampleNoSchema",
                                Example = "foo",
                                Schema = null
                            },
                            new OpenApiParameter
                            {
                                Name = "exampleBoolFalse",
                                Example = "false",
                                Schema = new OpenApiSchema { Type = "boolean" }
                            },
                            new OpenApiParameter
                            {
                                Name = "exampleDouble",
                                Example = "1.5",
                                Schema = new OpenApiSchema { Type = "number" }
                            },
                            new OpenApiParameter
                            {
                                Name = "exampleEmpty",
                                Examples = new Dictionary<string, OpenApiExample> { { "ex1", new OpenApiExample { Value = null } } }
                            },
                            new OpenApiParameter
                            {
                                Name = "explodeNoSchema",
                                Explode = true,
                                Schema = null
                            },
                            new OpenApiParameter
                            {
                                Name = "contentNoSchema",
                                Content = new Dictionary<string, OpenApiMediaType>
                                {
                                    { "application/json", new OpenApiMediaType { Schema = null } }
                                }
                            }
                        }
                    },
                    Post = new OpenApiOperation
                    {
                        OperationId = "PostMissing",
                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse
                            {
                                Content = new Dictionary<string, OpenApiMediaType>
                                {
                                    ["application/json"] = new OpenApiMediaType
                                    {
                                        Schema = new OpenApiSchema { Type = "array", Items = null }
                                    }
                                }
                            }
                        },
                        RequestBody = new OpenApiRequestBody
                        {
                            Content = new Dictionary<string, OpenApiMediaType>
                            {
                                ["application/json"] = new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Ref = "#/components/schemas/MyObj" } }
                                }
                            }
                        }
                    },
                    Put = new OpenApiOperation
                    {
                        OperationId = "PutMissing",
                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse
                            {
                                Content = new Dictionary<string, OpenApiMediaType>
                                {
                                    ["application/json"] = new OpenApiMediaType
                                    {
                                        Schema = new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Ref = null, Type = null } }
                                    }
                                }
                            }
                        },
                        RequestBody = new OpenApiRequestBody
                        {
                            Content = new Dictionary<string, OpenApiMediaType>
                            {
                                ["application/json"] = new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Ref = null, Type = null } }
                                }
                            }
                        }
                    },
                    Delete = new OpenApiOperation
                    {
                        OperationId = "DeleteMissing",
                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse
                            {
                                Content = null
                            }
                        },
                        RequestBody = new OpenApiRequestBody
                        {
                            Content = new Dictionary<string, OpenApiMediaType>
                            {
                                ["application/json"] = new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema { Type = null, Items = new OpenApiSchema { Ref = null } }
                                }
                            }
                        }
                    },
                    Options = new OpenApiOperation
                    {
                        OperationId = "OptionsMissing",
                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse
                            {
                                Content = new Dictionary<string, OpenApiMediaType>
                                {
                                    ["application/json"] = new OpenApiMediaType
                                    {
                                        Schema = new OpenApiSchema { Type = null, Items = null }
                                    }
                                }
                            }
                        },
                        RequestBody = new OpenApiRequestBody
                        {
                            Content = new Dictionary<string, OpenApiMediaType>
                            {
                                ["application/json"] = new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema { Type = null, Items = null }
                                }
                            }
                        },
                        Callbacks = new Dictionary<string, OpenApiCallback>
                        {
                            ["cb1"] = new OpenApiCallback { ["exp1"] = new OpenApiPathItem { Post = new OpenApiOperation { Description = "PostDesc" } } },
                            ["cb2"] = new OpenApiCallback { ["exp2"] = new OpenApiPathItem { Post = new OpenApiOperation(), Get = new OpenApiOperation { Description = "GetDesc" } } },
                            ["cb3"] = new OpenApiCallback { ["exp3"] = new OpenApiPathItem { Post = null, Get = null } }
                        }
                    },
                    Patch = new OpenApiOperation
                    {
                        OperationId = "PatchMissing",
                        Responses = new OpenApiResponses
                        {
                            ["400"] = new OpenApiResponse
                            {
                                Headers = new Dictionary<string, OpenApiHeader>
                                {
                                    ["X-Test-No-Schema"] = new OpenApiHeader
                                    {
                                        Content = new Dictionary<string, OpenApiMediaType>
                                        {
                                            ["application/json"] = new OpenApiMediaType { Schema = null }
                                        }
                                    }
                                },
                                Content = new Dictionary<string, OpenApiMediaType>
                                {
                                    ["application/json"] = new OpenApiMediaType
                                    {
                                        Schema = new OpenApiSchema { Type = "string" }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var interfaceNode = Cdd.OpenApi.Routes.Emit.ToInterface("IMissingBranchesApi", paths);
            var code = interfaceNode.ToFormattedString();
            Assert.Contains("GetMissing", code);
            Assert.Contains("PostMissing", code);
            Assert.Contains("PutMissing", code);
            Assert.Contains("DeleteMissing", code);
            Assert.Contains("OptionsMissing", code);
            Assert.Contains("PostDesc", code);
            Assert.Contains("GetDesc", code);
        }

        [Fact]
        public void ToInterface_EmptyCollections_Coverage()
        {
            var paths = new OpenApiPaths
            {
                ["/empty"] = new OpenApiPathItem
                {
                    Get = new OpenApiOperation
                    {
                        OperationId = "EmptyMethod",
                        Parameters = new List<OpenApiParameter>
                        {
                            new OpenApiParameter
                            {
                                Name = "p1",
                                Content = new Dictionary<string, OpenApiMediaType>(),
                                Examples = new Dictionary<string, OpenApiExample>()
                            }
                        },
                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse
                            {
                                Headers = new Dictionary<string, OpenApiHeader>
                                {
                                    ["X-Test"] = new OpenApiHeader
                                    {
                                        Examples = new Dictionary<string, OpenApiExample>(),
                                        Content = new Dictionary<string, OpenApiMediaType>()
                                    }
                                }
                            }
                        },
                        Servers = new List<OpenApiServer>
                        {
                            new OpenApiServer { Url = "http://empty", Description = null }
                        },
                        Callbacks = new Dictionary<string, OpenApiCallback>
                        {
                            ["emptyCb"] = new OpenApiCallback()
                        }
                    }
                }
            };
            var interfaceNode = Cdd.OpenApi.Routes.Emit.ToInterface("IEmptyApi", paths);
            var code = interfaceNode.ToFormattedString();
            Assert.Contains("EmptyMethod", code);
        }
    }
}
