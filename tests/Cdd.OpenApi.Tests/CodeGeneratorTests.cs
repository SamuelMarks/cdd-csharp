// <copyright file="CodeGeneratorTests.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Cdd.OpenApi.Models;
using Xunit;

namespace Cdd.OpenApi.Tests
{
    /// <summary>Verifies or exercises code generator tests.</summary>
    public class CodeGeneratorTests
    {
        /// <summary>Verifies or exercises add doc tags all filled.</summary>
        [Fact]
        public void AddDocTags_AllFilled()
        {
            var doc = new OpenApiDocument
            {
                Paths = new OpenApiPaths { ["/"] = new OpenApiPathItem { Get = new OpenApiOperation() } },
                Components = new OpenApiComponents
                {
                    Schemas = new Dictionary<string, OpenApiSchema>
                    {
                        ["ImplicitObject"] = new OpenApiSchema
                        {
                            Type = null,
                            Properties = new Dictionary<string, OpenApiSchema> { ["id"] = new OpenApiSchema { Type = "integer" } }
                        },
                        ["PrimitiveAlias"] = new OpenApiSchema
                        {
                            Type = "string",
                            Properties = null
                        }
                    }
                },
                Self = "myself",
                JsonSchemaDialect = "dialect",
                Info = new OpenApiInfo
                {
                    TermsOfService = "terms",
                    License = new OpenApiLicense { Identifier = "MIT" }
                }
            };
            var results = CodeGenerator.Generate(doc);
            Assert.NotEmpty(results);
            var apiFile = results.First(r => r.FileName == "src/Api/IDefaultApi.cs");
            Assert.Contains("<self>", apiFile.Code);
            Assert.Contains("myself", apiFile.Code);
            Assert.Contains("<jsonSchemaDialect>", apiFile.Code);
            Assert.Contains("dialect", apiFile.Code);
            Assert.Contains("<termsOfService>", apiFile.Code);
            Assert.Contains("terms", apiFile.Code);
            Assert.Contains("<license-identifier>", apiFile.Code);
            Assert.Contains("MIT", apiFile.Code);
        }

        /// <summary>Verifies or exercises add doc tags doc null.</summary>
        [Fact]
        public void AddDocTags_DocNull()
        {
            var results = CodeGenerator.Generate(null!);
            Assert.Empty(results);
        }

        /// <summary>Verifies or exercises add doc tags empty doc.</summary>
        [Fact]
        public void AddDocTags_EmptyDoc()
        {
            var results = CodeGenerator.Generate(new OpenApiDocument { Paths = new OpenApiPaths { ["/"] = new OpenApiPathItem { Get = new OpenApiOperation() } } });
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.FileName == "Attributes.cs");
            Assert.Contains(results, r => r.FileName == "src/Api/IDefaultApi.cs");
        }

        /// <summary>Verifies or exercises add doc tags terms and license.</summary>
        [Fact]
        public void AddDocTags_TermsAndLicense()
        {
            var doc = new OpenApiDocument
            {
                Paths = new OpenApiPaths { ["/"] = new OpenApiPathItem { Get = new OpenApiOperation() } },
                Info = new OpenApiInfo
                {
                    TermsOfService = null,
                    License = new OpenApiLicense { Identifier = null }
                }
            };
            var results1 = CodeGenerator.Generate(doc);
            Assert.NotEmpty(results1);
            var apiFile1 = results1.First(r => r.FileName == "src/Api/IDefaultApi.cs");
            Assert.DoesNotContain("<termsOfService>", apiFile1.Code);
            Assert.DoesNotContain("<license-identifier>", apiFile1.Code);

            var doc2 = new OpenApiDocument
            {
                Info = new OpenApiInfo
                {
                    TermsOfService = "",
                    License = new OpenApiLicense { Identifier = "" }
                }
            };
            var results2 = CodeGenerator.Generate(doc2);
            Assert.Empty(results2);
        }

        /// <summary>Verifies or exercises group paths by tag all operations creates correct groups.</summary>
        [Fact]
        public void GroupPathsByTag_AllOperations_CreatesCorrectGroups()
        {
            var opDelete = new OpenApiOperation { Tags = new List<string> { "delete" } };
            var opOptions = new OpenApiOperation { Tags = new List<string> { "options" } };
            var opHead = new OpenApiOperation { Tags = new List<string> { "head" } };
            var opPatch = new OpenApiOperation { Tags = new List<string> { "patch" } };
            var opTrace = new OpenApiOperation { Tags = new List<string> { "trace" } };
            var opQuery = new OpenApiOperation { Tags = new List<string> { "query" } };
            var opAdditional = new OpenApiOperation { Tags = new List<string> { "additional" } };

            var doc = new OpenApiDocument
            {
                Paths = new OpenApiPaths
                {
                    ["/all-ops"] = new OpenApiPathItem
                    {
                        Delete = opDelete,
                        Options = opOptions,
                        Head = opHead,
                        Patch = opPatch,
                        Trace = opTrace,
                        Query = opQuery,
                        AdditionalOperations = new Dictionary<string, OpenApiOperation>
                        {
                            { "x-custom", opAdditional }
                        }
                    }
                }
            };
            var codes = CodeGenerator.Generate(doc);
            Assert.NotEmpty(codes);
            Assert.Contains(codes, c => c.FileName == "src/Api/IDeleteApi.cs");
            Assert.Contains(codes, c => c.FileName == "src/Api/IOptionsApi.cs");
            Assert.Contains(codes, c => c.FileName == "src/Api/IHeadApi.cs");
            Assert.Contains(codes, c => c.FileName == "src/Api/IPatchApi.cs");
            Assert.Contains(codes, c => c.FileName == "src/Api/ITraceApi.cs");
            Assert.Contains(codes, c => c.FileName == "src/Api/IQueryApi.cs");
            Assert.Contains(codes, c => c.FileName == "src/Api/IAdditionalApi.cs");
        }

        /// <summary>Verifies or exercises group paths by tag inherits path level parameters and preserves overrides.</summary>
        [Fact]
        public void GroupPathsByTag_InheritsPathLevelParametersAndPreservesOverrides()
        {
            var doc = new OpenApiDocument
            {
                Paths = new OpenApiPaths
                {
                    ["/items/{id}"] = new OpenApiPathItem
                    {
                        Parameters = new List<OpenApiParameter>
                        {
                            new OpenApiParameter
                            {
                                Name = "id",
                                In = "path",
                                Required = true,
                                Schema = new OpenApiSchema { Type = "integer" }
                            },
                            new OpenApiParameter
                            {
                                Name = "tenant",
                                In = "header",
                                Required = false,
                                Schema = new OpenApiSchema { Type = "string" }
                            }
                        },
                        Get = new OpenApiOperation
                        {
                            OperationId = "GetItem",
                            Tags = new List<string> { "items" },
                            Parameters = new List<OpenApiParameter>
                            {
                                new OpenApiParameter
                                {
                                    Name = "id",
                                    In = "path",
                                    Required = true,
                                    Schema = new OpenApiSchema { Type = "integer" }
                                },
                                new OpenApiParameter
                                {
                                    Name = "filter",
                                    In = "query",
                                    Required = false,
                                    Schema = new OpenApiSchema { Type = "string" }
                                }
                            }
                        },
                        Delete = new OpenApiOperation
                        {
                            OperationId = "DeleteItem",
                            Tags = new List<string> { "items" },
                            Parameters = null
                        }
                    }
                }
            };

            var res1 = CodeGenerator.Generate(doc);
            var apiFile = res1.First(r => r.FileName == "src/Api/IItemsApi.cs");
            Assert.Contains("int id", apiFile.Code);
            Assert.Contains("string tenant", apiFile.Code);
            Assert.Contains("string filter", apiFile.Code);

            var doc3 = new OpenApiDocument { Paths = new OpenApiPaths() };
            var res3 = CodeGenerator.Generate(doc3);
            Assert.Empty(res3);
        }
    }
}
