// <copyright file="OpenApiExampleTests.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using Xunit;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Tests.Models
{
    /// <summary>Verifies or exercises open api example tests.</summary>
    public class OpenApiExampleTests
    {
        /// <summary>Verifies or exercises example properties can be set and got.</summary>
        [Fact]
        public void Example_Properties_CanBeSetAndGot()
        {
            var example = new OpenApiExample
            {
                Summary = "A summary",
                Description = "A description",
                Value = 42,
                DataValue = "structured_data",
                SerializedValue = "%7B%22a%22%3A1%7D",
                ExternalValue = "http://example.com/value.txt"
            };

            Assert.Equal("A summary", example.Summary);
            Assert.Equal("A description", example.Description);
            Assert.Equal(42, example.Value);
            Assert.Equal("structured_data", example.DataValue);
            Assert.Equal("%7B%22a%22%3A1%7D", example.SerializedValue);
            Assert.Equal("http://example.com/value.txt", example.ExternalValue);

            OpenApiAny strAny = "hello";
            OpenApiAny intAny = 42;
            OpenApiAny boolTrue = true;
            OpenApiAny boolFalse = false;
            OpenApiAny schemaAny = new OpenApiSchema { Type = "string" };

            Assert.Equal("hello", strAny.ToString());
            Assert.Equal(strAny.GetHashCode(), ((OpenApiAny)"hello").GetHashCode());
            Assert.True(strAny.Equals((object)(OpenApiAny)"hello"));
            Assert.False(strAny.Equals((object)"not_any"));
            Assert.False(strAny.Equals((OpenApiAny?)null));
            Assert.False(strAny.Equals(intAny));
            Assert.False(strAny.Equals(new OpenApiAny("hello", System.Text.Json.JsonValueKind.Number)));
            Assert.Equal("true", boolTrue.Value);
            Assert.Equal("false", boolFalse.Value);
            Assert.NotNull(schemaAny.Schema);

            var doc = new OpenApiDocument
            {
                Components = new OpenApiComponents
                {
                    Schemas = new System.Collections.Generic.Dictionary<string, OpenApiSchema>
                    {
                        ["Item"] = new OpenApiSchema
                        {
                            Type = "string",
                            Default = "def",
                            Example = 42,
                            Const = true
                        }
                    }
                }
            };
            var json = new Cdd.OpenApi.Emit.OpenApiEmitter().EmitJson(doc);
            var parsed = new Cdd.OpenApi.Parse.OpenApiParser().ParseJson(json);
            var item = parsed.Components!.Schemas!["Item"];
            Assert.Equal((OpenApiAny)"def", item.Default);
            Assert.Equal((OpenApiAny)42, item.Example);
            Assert.Equal((OpenApiAny)true, item.Const);

            OpenApiAny anyWithSchema1 = new OpenApiSchema { Type = "string" };
            OpenApiAny anyWithSchema2 = new OpenApiSchema { Type = "integer" };
            Assert.True(anyWithSchema1.Equals(anyWithSchema1));
            Assert.False(anyWithSchema1.Equals(anyWithSchema2));
            Assert.False(anyWithSchema1.Equals((object?)null));
            Assert.NotEqual(anyWithSchema1.GetHashCode(), anyWithSchema2.GetHashCode());
        }

        /// <summary>Verifies or exercises example data value and serialized value round trip across routes, clients, and cli.</summary>
        [Fact]
        public void Example_DataValueAndSerializedValue_RoundTrip()
        {
            var paths = new OpenApiPaths
            {
                ["/items"] = new OpenApiPathItem
                {
                    Get = new OpenApiOperation
                    {
                        OperationId = "GetItems",
                        Summary = "Get items",
                        Parameters = new System.Collections.Generic.List<OpenApiParameter>
                        {
                            new OpenApiParameter
                            {
                                Name = "filter",
                                In = "query",
                                Schema = new OpenApiSchema { Type = "string" },
                                Examples = new System.Collections.Generic.Dictionary<string, OpenApiExample>
                                {
                                    ["dataEx"] = new OpenApiExample { DataValue = "raw_val" },
                                    ["serEx"] = new OpenApiExample { SerializedValue = "raw%5Fval" },
                                    ["emptyEx"] = new OpenApiExample()
                                }
                            },
                            new OpenApiParameter
                            {
                                Name = "serOnly",
                                In = "query",
                                Schema = new OpenApiSchema { Type = "string" },
                                Examples = new System.Collections.Generic.Dictionary<string, OpenApiExample>
                                {
                                    ["serEx"] = new OpenApiExample { SerializedValue = "encoded_only" }
                                }
                            }
                        },
                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse
                            {
                                Description = "OK",
                                Headers = new System.Collections.Generic.Dictionary<string, OpenApiHeader>
                                {
                                    ["X-Rate"] = new OpenApiHeader
                                    {
                                        Description = "Rate header",
                                        Schema = new OpenApiSchema { Type = "string" },
                                        Examples = new System.Collections.Generic.Dictionary<string, OpenApiExample>
                                        {
                                            ["hData"] = new OpenApiExample { DataValue = "100" },
                                            ["hSer"] = new OpenApiExample { SerializedValue = "100%2Fsec" },
                                            ["hEmpty"] = new OpenApiExample()
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var interfaceNode = Cdd.OpenApi.Routes.Emit.ToInterface("IItemsApi", paths);
            var rawController = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ClassDeclaration("ItemsController")
                .AddMembers(interfaceNode.Members.ToArray());
            var controllerTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(rawController.ToFormattedString());
            var controllerNode = controllerTree.GetRoot().DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>().First();
            var extractedPaths = Cdd.OpenApi.Routes.Parse.ToPaths(controllerNode);
            var op = extractedPaths["/items"].Get!;
            Assert.Equal("raw_val", op.Parameters![0].Examples!["dataEx"].DataValue);
            Assert.Equal("raw%5Fval", op.Parameters![0].Examples!["serEx"].SerializedValue);
            Assert.Equal("", op.Parameters![0].Examples!["emptyEx"].Value);

            var respHeaders = op.Responses!["200"].Headers!["X-Rate"].Examples!;
            Assert.Equal("100", respHeaders["hData"].DataValue);
            Assert.Equal("100%2Fsec", respHeaders["hSer"].SerializedValue);
            Assert.Equal("", respHeaders["hEmpty"].Value);

            var cliNode = Cdd.OpenApi.CliModule.Emit.ToCli("ItemsCli", paths);
            var cliCode = cliNode.ToFullString();
            Assert.Contains("raw_val", cliCode);
            Assert.Contains("encoded_only", cliCode);
        }
    }
}
