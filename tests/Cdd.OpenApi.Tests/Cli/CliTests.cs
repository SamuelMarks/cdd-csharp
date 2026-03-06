using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using Cdd.OpenApi.CliModule;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Tests.Cli
{
    public class CliTests
    {
        [Fact]
        public void Parse_ToPaths_ExtractsParametersToPathItem()
        {
            var code = @"
            public class Program
            {
                public static void Main(string[] args)
                {
                    switch (args[0])
                    {
                        case ""init"":
                            int count = 5;
                            string format = ""json"";
                            break;
                    }
                }
            }";
            
            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var paths = Cdd.OpenApi.CliModule.Parse.ToPaths(classNode);
            
            var pathItem = paths["/init"];
            Assert.NotNull(pathItem);
            Assert.NotNull(pathItem.Parameters);
            Assert.Equal(2, pathItem.Parameters.Count);
            Assert.NotNull(pathItem.Get);

            var countParam = pathItem.Parameters.First(p => p.Name == "count");
            Assert.Equal("integer", countParam.Schema?.Type);

            var formatParam = pathItem.Parameters.First(p => p.Name == "format");
            Assert.Equal("string", formatParam.Schema?.Type);
        }

        [Fact]
        public void Parse_ToPaths_MapsHttpMethods()
        {
            var code = @"
            public class Program
            {
                public static void Main(string[] args)
                {
                    switch (args[0])
                    {
                        case ""put"":
                            break;
                        case ""post"":
                            break;
                        case ""delete"":
                            break;
                        case ""options"":
                            break;
                        case ""head"":
                            break;
                        case ""patch"":
                            break;
                        case ""trace"":
                            break;
                        case ""get"":
                            break;
                    }
                }
            }";
            
            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var paths = Cdd.OpenApi.CliModule.Parse.ToPaths(classNode);
            
            Assert.NotNull(paths["/put"].Put);
            Assert.NotNull(paths["/post"].Post);
            Assert.NotNull(paths["/delete"].Delete);
            Assert.NotNull(paths["/options"].Options);
            Assert.NotNull(paths["/head"].Head);
            Assert.NotNull(paths["/patch"].Patch);
            Assert.NotNull(paths["/trace"].Trace);
            Assert.NotNull(paths["/get"].Get);
        }

        [Fact]
        public void Parse_ToPaths_ExtractsDescriptionAndExample()
        {
            var code = @"
            public class Program
            {
                public static void Main(string[] args)
                {
                    switch (args[0])
                    {
                        case ""init"":
                            // The number of items
                            int count = 5;
                            // The format
                            string format = ""json"";
                            break;
                    }
                }
            }";
            
            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var paths = Cdd.OpenApi.CliModule.Parse.ToPaths(classNode);
            
            var pathItem = paths["/init"];
            var countParam = pathItem.Parameters.First(p => p.Name == "count");
            Assert.Equal("The number of items", countParam.Description);
            Assert.Equal(5, countParam.Example);

            var formatParam = pathItem.Parameters.First(p => p.Name == "format");
            Assert.Equal("The format", formatParam.Description);
            Assert.Equal("json", formatParam.Example);
        }

        [Fact]
        public void Parse_ToPaths_MapsQueryMethod()
        {
            var code = @"
            public class Program
            {
                public static void Main(string[] args)
                {
                    switch (args[0])
                    {
                        case ""query"":
                            break;
                    }
                }
            }";
            
            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var paths = Cdd.OpenApi.CliModule.Parse.ToPaths(classNode);
            
            Assert.NotNull(paths["/query"].Query);
        }

        [Fact]
        public void Emit_ToCli_GeneratesCliWithParameters()
        {
            var paths = new OpenApiPaths();
            paths.Add("/test", new OpenApiPathItem
            {
                Get = new OpenApiOperation
                {
                    OperationId = "test",
                    Summary = "Test operation",
                    Parameters = new System.Collections.Generic.List<OpenApiParameter>
                    {
                        new OpenApiParameter
                        {
                            Name = "count",
                            Description = "The number of items",
                            Schema = new OpenApiSchema { Type = "integer" },
                            Example = 5
                        },
                        new OpenApiParameter
                        {
                            Name = "format",
                            Description = "The format",
                            Schema = new OpenApiSchema { Type = "string" },
                            Example = "json"
                        }
                    }
                }
            });
            
            var node = Cdd.OpenApi.CliModule.Emit.ToCli("MyCli", paths);
            var code = node.ToFullString();
            Assert.Contains("string format = default;", code);
            Assert.Contains("format = args[++i];", code);
        }
    }
}
