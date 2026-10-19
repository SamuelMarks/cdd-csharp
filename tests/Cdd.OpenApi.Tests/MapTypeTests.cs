using Xunit;
using Cdd.OpenApi.Classes;

namespace Cdd.OpenApi.Tests
{
    public class MapTypeTests
    {
        [Fact]
        public void RouteParseMapType_HandlesShort()
        {
            // Just covering the fallback switch arms not hit
            var paths = Cdd.OpenApi.Routes.Parse.ToPaths(
                Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ClassDeclaration("C")
                .AddMembers(
                    Microsoft.CodeAnalysis.CSharp.SyntaxFactory.MethodDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("void"), "M")
                    .AddAttributeLists(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SingletonSeparatedList(
                        Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Attribute(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("HttpGet"))
                    )))
                    .AddParameterListParameters(
                        Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("p")).WithType(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName("short"))
                    )
                )
            );
            Assert.Equal("integer", paths["/"].Get?.Parameters?[0].Schema?.Type);
        }

        [Fact]
        public void RouteEmitMapType_HandlesOtherTypes()
        {
            var iface = Cdd.OpenApi.Routes.Emit.ToInterface("I", new Cdd.OpenApi.Models.OpenApiPaths
            {
                ["/"] = new Cdd.OpenApi.Models.OpenApiPathItem
                {
                    Get = new Cdd.OpenApi.Models.OpenApiOperation
                    {
                        OperationId = "M",
                        Parameters = new System.Collections.Generic.List<Cdd.OpenApi.Models.OpenApiParameter> {
                            new Cdd.OpenApi.Models.OpenApiParameter { Name = "p1", Schema = new Cdd.OpenApi.Models.OpenApiSchema { Type = "integer" } },
                            new Cdd.OpenApi.Models.OpenApiParameter { Name = "p2", Schema = new Cdd.OpenApi.Models.OpenApiSchema { Type = "boolean" } },
                            new Cdd.OpenApi.Models.OpenApiParameter { Name = "p3", Schema = new Cdd.OpenApi.Models.OpenApiSchema { Type = "string" } },
                            new Cdd.OpenApi.Models.OpenApiParameter { Name = "p4", Schema = new Cdd.OpenApi.Models.OpenApiSchema { Type = "unknown" } },
                        }
                    }
                }
            });
            var code = iface.ToFormattedString();
            AssertHelper.ContainsNoWhitespace("int p1", code);
            AssertHelper.ContainsNoWhitespace("bool p2", code);
            AssertHelper.ContainsNoWhitespace("string p3", code);
            AssertHelper.ContainsNoWhitespace("string p4", code);
        }

        [Fact]
        public void ClassEmitMapType_HandlesAllTypes()
        {
            var cls = Cdd.OpenApi.Classes.Emit.ToClass("C", new Cdd.OpenApi.Models.OpenApiSchema
            {
                Properties = new System.Collections.Generic.Dictionary<string, Cdd.OpenApi.Models.OpenApiSchema>
                {
                    ["p1"] = new Cdd.OpenApi.Models.OpenApiSchema { Type = "integer" },
                    ["p2"] = new Cdd.OpenApi.Models.OpenApiSchema { Type = "boolean" },
                    ["p3"] = new Cdd.OpenApi.Models.OpenApiSchema { Type = "string" },
                    ["p4"] = new Cdd.OpenApi.Models.OpenApiSchema { Type = "unknown" },
                }
            });
            var code = cls.ToFormattedString();
            AssertHelper.ContainsNoWhitespace("int? p1", code);
            AssertHelper.ContainsNoWhitespace("bool? p2", code);
            AssertHelper.ContainsNoWhitespace("string? p3", code);
            AssertHelper.ContainsNoWhitespace("object p4", code);
        }

        [Fact]
        public void TypeMapper_MapSchemaToCSharp_CoversAllFormatsAndConstructs()
        {
            Assert.Equal("object", Cdd.OpenApi.Common.TypeMapper.MapSchemaToCSharp(null));
            Assert.Equal("Pet", Cdd.OpenApi.Common.TypeMapper.MapSchemaToCSharp(new Cdd.OpenApi.Models.OpenApiSchema { Ref = "#/components/schemas/Pet" }));
            Assert.Equal("List<int>", Cdd.OpenApi.Common.TypeMapper.MapSchemaToCSharp(new Cdd.OpenApi.Models.OpenApiSchema { Type = "array", Items = new Cdd.OpenApi.Models.OpenApiSchema { Type = "integer" } }));
            Assert.Equal("Dictionary<string, string>", Cdd.OpenApi.Common.TypeMapper.MapSchemaToCSharp(new Cdd.OpenApi.Models.OpenApiSchema { Type = "object", AdditionalProperties = new Cdd.OpenApi.Models.OpenApiSchema { Type = "string" } }));
            Assert.Equal("object", Cdd.OpenApi.Common.TypeMapper.MapSchemaToCSharp(new Cdd.OpenApi.Models.OpenApiSchema { Type = "object" }));
            Assert.Equal("long", Cdd.OpenApi.Common.TypeMapper.MapSchemaToCSharp(new Cdd.OpenApi.Models.OpenApiSchema { Type = "integer", Format = "int64" }));
            Assert.Equal("int", Cdd.OpenApi.Common.TypeMapper.MapSchemaToCSharp(new Cdd.OpenApi.Models.OpenApiSchema { Type = "integer", Format = "int32" }));
            Assert.Equal("float", Cdd.OpenApi.Common.TypeMapper.MapSchemaToCSharp(new Cdd.OpenApi.Models.OpenApiSchema { Type = "number", Format = "float" }));
            Assert.Equal("decimal", Cdd.OpenApi.Common.TypeMapper.MapSchemaToCSharp(new Cdd.OpenApi.Models.OpenApiSchema { Type = "number", Format = "decimal" }));
            Assert.Equal("double", Cdd.OpenApi.Common.TypeMapper.MapSchemaToCSharp(new Cdd.OpenApi.Models.OpenApiSchema { Type = "number", Format = "double" }));
            Assert.Equal("Guid", Cdd.OpenApi.Common.TypeMapper.MapSchemaToCSharp(new Cdd.OpenApi.Models.OpenApiSchema { Type = "string", Format = "uuid" }));
            Assert.Equal("DateTimeOffset", Cdd.OpenApi.Common.TypeMapper.MapSchemaToCSharp(new Cdd.OpenApi.Models.OpenApiSchema { Type = "string", Format = "date-time" }));
            Assert.Equal("DateOnly", Cdd.OpenApi.Common.TypeMapper.MapSchemaToCSharp(new Cdd.OpenApi.Models.OpenApiSchema { Type = "string", Format = "date" }));
            Assert.Equal("TimeOnly", Cdd.OpenApi.Common.TypeMapper.MapSchemaToCSharp(new Cdd.OpenApi.Models.OpenApiSchema { Type = "string", Format = "time" }));
            Assert.Equal("Uri", Cdd.OpenApi.Common.TypeMapper.MapSchemaToCSharp(new Cdd.OpenApi.Models.OpenApiSchema { Type = "string", Format = "uri" }));
            Assert.Equal("byte[]", Cdd.OpenApi.Common.TypeMapper.MapSchemaToCSharp(new Cdd.OpenApi.Models.OpenApiSchema { Type = "string", Format = "byte" }));
            Assert.Equal("byte[]", Cdd.OpenApi.Common.TypeMapper.MapSchemaToCSharp(new Cdd.OpenApi.Models.OpenApiSchema { Type = "string", Format = "binary" }));
            Assert.Equal("string", Cdd.OpenApi.Common.TypeMapper.MapSchemaToCSharp(new Cdd.OpenApi.Models.OpenApiSchema { Type = "string", Format = "custom" }));
            Assert.Equal("bool", Cdd.OpenApi.Common.TypeMapper.MapSchemaToCSharp(new Cdd.OpenApi.Models.OpenApiSchema { Type = "boolean" }));

            var ifaceWithoutMcp = Cdd.OpenApi.Routes.Emit.ToInterface("INoMcp", new Cdd.OpenApi.Models.OpenApiPaths(), includeMcp: false);
            Assert.DoesNotContain("McpSseEndpoint", ifaceWithoutMcp.ToFormattedString());

            var docWithPaths = new Cdd.OpenApi.Models.OpenApiDocument
            {
                Paths = new Cdd.OpenApi.Models.OpenApiPaths { ["/p"] = new Cdd.OpenApi.Models.OpenApiPathItem { Get = new Cdd.OpenApi.Models.OpenApiOperation() } }
            };
            var sdkWithMcp = CodeGenerator.Generate(docWithPaths, "Generated", GenerateType.Sdk, tests: false, mcp: true);
            Assert.Contains(sdkWithMcp, f => f.FileName == "src/Cli/McpServer.cs");
        }

        [Fact]
        public void RouteParseMapType_HandlesDecimal_Double_Float_Bool()
        {
            var code = @"
            public class C {
                [HttpGet] public void M(decimal d, double db, float f, bool b, object o) {}
            }";
            var tree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(code);
            var classNode = System.Linq.Enumerable.First(System.Linq.Enumerable.OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>(tree.GetRoot().DescendantNodes()));

            var paths = Cdd.OpenApi.Routes.Parse.ToPaths(classNode);
            var p = paths["/"].Get?.Parameters;

            Assert.Equal("number", p?[0].Schema?.Type);
            Assert.Equal("number", p?[1].Schema?.Type);
            Assert.Equal("number", p?[2].Schema?.Type);
            Assert.Equal("boolean", p?[3].Schema?.Type);
            Assert.Equal("string", p?[4].Schema?.Type);
        }

        [Fact]
        public void ClassParseMapType_HandlesDecimal_Double_Float_Bool()
        {
            var code = @"
            public class C {
                public decimal D {get;set;}
                public double Db {get;set;}
                public float F {get;set;}
                public bool B {get;set;}
                public long L {get;set;}
                public System.Guid G {get;set;}
            }";
            var tree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(code);
            var classNode = System.Linq.Enumerable.First(System.Linq.Enumerable.OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>(tree.GetRoot().DescendantNodes()));

            var schema = Cdd.OpenApi.Classes.Parse.ToSchema(classNode);
            var p = schema.Properties;

            Assert.Equal("number", p?["D"].Type);
            Assert.Equal("decimal", p?["D"].Format);
            Assert.Equal("number", p?["Db"].Type);
            Assert.Null(p?["Db"].Format);
            Assert.Equal("number", p?["F"].Type);
            Assert.Equal("float", p?["F"].Format);
            Assert.Equal("boolean", p?["B"].Type);
            Assert.Equal("integer", p?["L"].Type);
            Assert.Equal("int64", p?["L"].Format);
            Assert.Equal("string", p?["G"].Type);
            Assert.Equal("uuid", p?["G"].Format);
        }
    }
}
