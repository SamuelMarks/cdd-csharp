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
            var iface = Cdd.OpenApi.Routes.Emit.ToInterface("I", new Cdd.OpenApi.Models.OpenApiPaths { 
                ["/"] = new Cdd.OpenApi.Models.OpenApiPathItem { 
                    Get = new Cdd.OpenApi.Models.OpenApiOperation {
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
            var code = iface.ToFullString();
            Assert.Contains("int p1", code);
            Assert.Contains("bool p2", code);
            Assert.Contains("string p3", code);
            Assert.Contains("string p4", code);
        }
        
        [Fact]
        public void ClassEmitMapType_HandlesAllTypes()
        {
            var cls = Cdd.OpenApi.Classes.Emit.ToClass("C", new Cdd.OpenApi.Models.OpenApiSchema {
                Properties = new System.Collections.Generic.Dictionary<string, Cdd.OpenApi.Models.OpenApiSchema> {
                    ["p1"] = new Cdd.OpenApi.Models.OpenApiSchema { Type = "integer" },
                    ["p2"] = new Cdd.OpenApi.Models.OpenApiSchema { Type = "boolean" },
                    ["p3"] = new Cdd.OpenApi.Models.OpenApiSchema { Type = "string" },
                    ["p4"] = new Cdd.OpenApi.Models.OpenApiSchema { Type = "unknown" },
                }
            });
            var code = cls.ToFullString();
            Assert.Contains("int? p1", code);
            Assert.Contains("bool? p2", code);
            Assert.Contains("string? p3", code);
            Assert.Contains("object p4", code);
        }
    }
}
