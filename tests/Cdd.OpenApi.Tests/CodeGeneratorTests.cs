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
            CodeGenerator.Generate(null);
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

            CodeGenerator.Generate(null);

            var doc3 = new OpenApiDocument { Paths = new OpenApiPaths() }; // Count = 0
            CodeGenerator.Generate(doc3);
        }
    }
}
