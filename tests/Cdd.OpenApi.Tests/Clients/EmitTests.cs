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
            var code = classNode.ToFormattedString();

            AssertHelper.ContainsNoWhitespace("class TestClient", code);
            AssertHelper.ContainsNoWhitespace("private readonly System.Net.Http.HttpClient _httpClient;", code);
            AssertHelper.ContainsNoWhitespace("public TestClient(System.Net.Http.HttpClient httpClient)", code);
            AssertHelper.ContainsNoWhitespace("GetUsersAsync()", code);
            AssertHelper.ContainsNoWhitespace("CreateUserAsync(int id)", code);
            AssertHelper.ContainsNoWhitespace("PutusersidAsync()", code);

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
            var typesCode = allTypesNode.ToFormattedString();
            AssertHelper.ContainsNoWhitespace("int a", typesCode);
            AssertHelper.ContainsNoWhitespace("double b", typesCode);
            AssertHelper.ContainsNoWhitespace("bool c", typesCode);
            AssertHelper.ContainsNoWhitespace("string d", typesCode);
            AssertHelper.ContainsNoWhitespace("string e", typesCode);
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
            var code = interfaceNode.ToFormattedString();

            Assert.NotNull(code);
            AssertHelper.ContainsNoWhitespace("QueryFullAsync", code);
            AssertHelper.ContainsNoWhitespace("PurgeFullAsync", code);
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
            var code = classNode.ToFormattedString();

            // stringParam should have [Explode]
            AssertHelper.ContainsNoWhitespace("[Explode] string stringParam", code);
            // arrayParam should NOT have [Explode]
            Assert.DoesNotContain("[Explode] System.Collections.Generic.List<string> arrayParam", code);
            AssertHelper.ContainsNoWhitespace("System.Collections.Generic.List<string> arrayParam", code);
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
            var code = classNode.ToFormattedString();

            AssertHelper.ContainsNoWhitespace("System.Collections.Generic.List<MyModel>", code);
            AssertHelper.ContainsNoWhitespace("System.Collections.Generic.List<int>", code);
            AssertHelper.ContainsNoWhitespace("request.Headers.Add(\"myHeader\", myHeader.ToString());", code);
        }
        [Fact]
        public void ToClient_NullableAndEmpty_Coverage()
        {
            var paths = new OpenApiPaths
            {
                ["/empty"] = new OpenApiPathItem
                {
                    Get = new OpenApiOperation
                    {
                        OperationId = "GetEmpty",
                        Parameters = new List<OpenApiParameter>
                        {
                            new OpenApiParameter
                            {
                                Name = "p1",
                                Content = new Dictionary<string, OpenApiMediaType>(),
                                Examples = new Dictionary<string, OpenApiExample>(),
                                Schema = new OpenApiSchema { Items = new OpenApiSchema { Ref = "#/components/schemas/RefOnly" } }
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
                        RequestBody = new OpenApiRequestBody
                        {
                            Content = new Dictionary<string, OpenApiMediaType>
                            {
                                ["application/json"] = new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema { Items = new OpenApiSchema { Type = "integer" } }
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
                    },
                    Post = new OpenApiOperation
                    {
                        OperationId = "PostEmpty",
                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse
                            {
                                Content = new Dictionary<string, OpenApiMediaType>
                                {
                                    ["application/json"] = new OpenApiMediaType
                                    {
                                        Schema = new OpenApiSchema { Items = new OpenApiSchema { Ref = "#/components/schemas/ResRefOnly" } }
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
                                    Schema = new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Type = "string" } }
                                }
                            }
                        }
                    }
                }
            };
            var interfaceNode = Cdd.OpenApi.Clients.Emit.ToClient("IEmptyApi", paths);
            var code = interfaceNode.ToFormattedString();
            Assert.Contains("GetEmpty", code);
            Assert.Contains("PostEmpty", code);
        }
        [Fact]
        public void ToClient_FullTypes_Coverage()
        {
            var paths = new OpenApiPaths
            {
                ["/types"] = new OpenApiPathItem
                {
                    Get = new OpenApiOperation
                    {
                        OperationId = "GetTypes",
                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse
                            {
                                Content = new Dictionary<string, OpenApiMediaType>
                                {
                                    ["application/json"] = new OpenApiMediaType
                                    {
                                        Schema = new OpenApiSchema { Ref = "#/components/schemas/MyObj" }
                                    }
                                }
                            }
                        },
                        Parameters = new List<OpenApiParameter>
                        {
                            new OpenApiParameter { Name = "p1", Schema = new OpenApiSchema { Ref = "#/components/schemas/MyObj2" } },
                            new OpenApiParameter { Name = "p2", Schema = new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Ref = "#/components/schemas/MyObj3" } } },
                            new OpenApiParameter { Name = "p3", Schema = new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Type = "integer" } } },
                            new OpenApiParameter { Name = "p4", Schema = new OpenApiSchema { Type = "integer" } },
                            new OpenApiParameter { Name = "p5", Schema = new OpenApiSchema { Items = new OpenApiSchema { Ref = "#/components/schemas/MyObj4" } } }
                        }
                    }
                }
            };
            var interfaceNode = Cdd.OpenApi.Clients.Emit.ToClient("ITypesApi", paths);
            var code = interfaceNode.ToFormattedString();
            var noSpaceCode = code.Replace(" ", "");
            Assert.Contains("MyObj", noSpaceCode);
            Assert.Contains("MyObj2p1", noSpaceCode);
            Assert.Contains("List<MyObj3>p2", noSpaceCode);
            Assert.Contains("List<int>p3", noSpaceCode);
            Assert.Contains("intp4", noSpaceCode);
            Assert.Contains("List<MyObj4>p5", noSpaceCode);
        }
        [Fact]
        public void ToClient_Params_Coverage()
        {
            var paths = new OpenApiPaths
            {
                ["/params"] = new OpenApiPathItem
                {
                    Get = new OpenApiOperation
                    {
                        OperationId = "GetParams",
                        Parameters = new List<OpenApiParameter>
                        {
                            new OpenApiParameter { Name = "p1", Explode = true, Schema = new OpenApiSchema { Type = "string" } },
                            new OpenApiParameter { Name = "p2", Explode = false, Schema = new OpenApiSchema { Type = "string" } }
                        }
                    }
                }
            };
            var interfaceNode = Cdd.OpenApi.Clients.Emit.ToClient("IParamsApi", paths);
            var code = interfaceNode.ToFormattedString();
            Assert.Contains("GetParams", code);
            Assert.Contains("[Explode]", code); // p1 should have explode
        }
        [Fact]
        public void ToClient_Params_Edge_Coverage()
        {
            var paths = new OpenApiPaths
            {
                ["/params-edge"] = new OpenApiPathItem
                {
                    Get = new OpenApiOperation
                    {
                        OperationId = "GetParamsEdge",
                        Parameters = new List<OpenApiParameter>
                        {
                            new OpenApiParameter { Name = "p1", AllowEmptyValue = false, Schema = new OpenApiSchema { Type = "string" } },
                            new OpenApiParameter { Name = "p2", Deprecated = false, Schema = new OpenApiSchema { Type = "string" } },
                            new OpenApiParameter { Name = "p3", AllowReserved = false, Schema = new OpenApiSchema { Type = "string" } }
                        }
                    }
                }
            };
            var interfaceNode = Cdd.OpenApi.Clients.Emit.ToClient("IParamsEdgeApi", paths);
            var code = interfaceNode.ToFormattedString();
            Assert.Contains("GetParamsEdge", code);
        }
        [Fact]
        public void ToClient_FullTypes_ArrayRef_Coverage()
        {
            var paths = new OpenApiPaths
            {
                ["/types2"] = new OpenApiPathItem
                {
                    Get = new OpenApiOperation
                    {
                        OperationId = "GetTypes2",
                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse
                            {
                                Content = new Dictionary<string, OpenApiMediaType>
                                {
                                    ["application/json"] = new OpenApiMediaType
                                    {
                                        Schema = new OpenApiSchema { Items = new OpenApiSchema { Ref = "#/components/schemas/MyObj5" } }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            var interfaceNode = Cdd.OpenApi.Clients.Emit.ToClient("ITypes2Api", paths);
            var code = interfaceNode.ToFormattedString();
            Assert.Contains("List<MyObj5>", code);
        }
        [Fact]
        public void ToClient_FullTypes_ReqBody_Coverage()
        {
            var paths = new OpenApiPaths
            {
                ["/types3"] = new OpenApiPathItem
                {
                    Get = new OpenApiOperation
                    {
                        OperationId = "GetTypes3",
                        RequestBody = new OpenApiRequestBody
                        {
                            Content = new Dictionary<string, OpenApiMediaType>
                            {
                                ["application/json"] = new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema { Ref = "#/components/schemas/MyObj6" }
                                }
                            }
                        }
                    }
                }
            };
            var interfaceNode = Cdd.OpenApi.Clients.Emit.ToClient("ITypes3Api", paths);
            var code = interfaceNode.ToFormattedString();
            Assert.Contains("MyObj6 body", code);
        }
        [Fact]
        public void ToClient_Params_Examples_Content_Coverage()
        {
            var paths = new OpenApiPaths
            {
                ["/params2"] = new OpenApiPathItem
                {
                    Get = new OpenApiOperation
                    {
                        OperationId = "GetParams2",
                        Parameters = new List<OpenApiParameter>
                        {
                            new OpenApiParameter
                            {
                                Name = "p1",
                                Example = "hello",
                                Schema = new OpenApiSchema { Type = "string" }
                            },
                            new OpenApiParameter
                            {
                                Name = "p2",
                                Example = "true",
                                Schema = new OpenApiSchema { Type = "boolean" }
                            },
                            new OpenApiParameter
                            {
                                Name = "p3",
                                Example = "10",
                                Schema = new OpenApiSchema { Type = "integer" }
                            },
                            new OpenApiParameter
                            {
                                Name = "p4",
                                Example = "10.5",
                                Schema = new OpenApiSchema { Type = "number" }
                            },
                            new OpenApiParameter
                            {
                                Name = "p5",
                                Example = "invalid",
                                Schema = new OpenApiSchema { Type = "integer" }
                            }
                        }
                    }
                }
            };
            var interfaceNode = Cdd.OpenApi.Clients.Emit.ToClient("IParams2Api", paths);
            var code = interfaceNode.ToFormattedString();
            Assert.Contains("p1=\"hello\"", code.Replace(" ", ""));
            Assert.Contains("p2=true", code.Replace(" ", ""));
            Assert.Contains("p3=10", code.Replace(" ", ""));
            Assert.Contains("p4=10.5", code.Replace(" ", ""));
            Assert.Contains("p5=null", code.Replace(" ", ""));
        }
        [Fact]
        public void ToClient_Params_RefTypes_Coverage()
        {
            var paths = new OpenApiPaths
            {
                ["/params-ref"] = new OpenApiPathItem
                {
                    Get = new OpenApiOperation
                    {
                        OperationId = "GetParamsRef",
                        Parameters = new List<OpenApiParameter>
                        {
                            new OpenApiParameter { Name = "p1", Schema = new OpenApiSchema { Ref = "#/components/schemas/MyObjRef" } },
                            new OpenApiParameter { Name = "p2", Schema = new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Ref = "#/components/schemas/MyObjRefArray" } } },
                            new OpenApiParameter { Name = "p3", Schema = new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Type = "integer" } } },
                            new OpenApiParameter { Name = "p4", Schema = new OpenApiSchema { Type = "integer" } },
                            new OpenApiParameter { Name = "p5", Schema = new OpenApiSchema { Items = new OpenApiSchema { Ref = "#/components/schemas/MyObjRefItem" } } }
                        }
                    }
                }
            };
            var interfaceNode = Cdd.OpenApi.Clients.Emit.ToClient("IParamsRefApi", paths);
            var code = interfaceNode.ToFormattedString();
            var noSpaceCode = code.Replace(" ", "");
            Assert.Contains("MyObjRefp1", noSpaceCode);
            Assert.Contains("List<MyObjRefArray>p2", noSpaceCode);
            Assert.Contains("List<int>p3", noSpaceCode);
            Assert.Contains("intp4", noSpaceCode);
            Assert.Contains("List<MyObjRefItem>p5", noSpaceCode);
        }

        [Fact]
        public void ToClient_Emit_MissingBranches_Coverage()
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
                },
                ["/path-with-{unclosed"] = new OpenApiPathItem
                {
                    Get = new OpenApiOperation
                    {
                        OperationId = "GetUnclosed",
                        Parameters = new List<OpenApiParameter>
                        {
                            new OpenApiParameter { Name = "p1", Schema = new OpenApiSchema { Type = "string" } }
                        },
                        Responses = new OpenApiResponses()
                    }
                }
            };

            var classNode = Cdd.OpenApi.Clients.Emit.ToClient("MissingBranchesClient", paths);
            var code = classNode.ToFormattedString();
            Assert.Contains("GetMissing", code);
            Assert.Contains("PostMissing", code);
            Assert.Contains("PutMissing", code);
            Assert.Contains("DeleteMissing", code);
            Assert.Contains("OptionsMissing", code);
            Assert.Contains("PostDesc", code);
            Assert.Contains("GetDesc", code);
        }
    }
}
