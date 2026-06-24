using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Tests
{
    public class CodeGeneratorTests
    {


        [Fact]
        public void AddDocTags_AllFilled()
        {
            var doc = new OpenApiDocument
            {
                Paths = new OpenApiPaths { ["/"] = new OpenApiPathItem { Get = new OpenApiOperation() } },
                Self = "myself",
                JsonSchemaDialect = "dialect",
                Info = new OpenApiInfo
                {
                    TermsOfService = "terms",
                    License = new OpenApiLicense { Identifier = "MIT" }
                }
            };
            CodeGenerator.Generate(doc);
        }



        [Fact]
        public void AddDocTags_DocNull()
        {
            CodeGenerator.Generate(null!);
        }

        [Fact]
        public void AddDocTags_EmptyDoc()
        {
            CodeGenerator.Generate(new OpenApiDocument { Paths = new OpenApiPaths { ["/"] = new OpenApiPathItem { Get = new OpenApiOperation() } } });
        }

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
            CodeGenerator.Generate(doc);

            var doc2 = new OpenApiDocument
            {
                Info = new OpenApiInfo
                {
                    TermsOfService = "",
                    License = new OpenApiLicense { Identifier = "" }
                }
            };
            CodeGenerator.Generate(doc2);
        }

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
        }

        [Fact]
        public void AddDocTags_CoversBranches()
        {
            var doc = new OpenApiDocument
            {
                Paths = new OpenApiPaths { ["/"] = new OpenApiPathItem { Get = new OpenApiOperation() } },
                Self = "myself",
                JsonSchemaDialect = "dialect",
                Info = new OpenApiInfo
                {
                    TermsOfService = "terms",
                    License = new OpenApiLicense { Identifier = "MIT" }
                }
            };
            CodeGenerator.Generate(doc);

            var doc2 = new OpenApiDocument
            {
                Info = new OpenApiInfo()
            };
            CodeGenerator.Generate(doc2);

            CodeGenerator.Generate(null!);

            var doc3 = new OpenApiDocument { Paths = new OpenApiPaths() }; // Count = 0
            CodeGenerator.Generate(doc3);
        }
    }
}
