using Xunit;
using Cdd.OpenApi.Models;
using Microsoft.CodeAnalysis.CSharp;

namespace Cdd.OpenApi.Tests.Placeholders
{
    public class PlaceholderTests
    {
        [Fact]
        public void TestFunctions()
        {
            var code = @"
            public class TestFunc 
            { 
                /// <summary>
                /// Does a task
                /// </summary>
                public void DoTask() {} 
            }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var methodNode = tree.GetRoot().DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>().First();
            
            var op = global::Cdd.OpenApi.Functions.Parse.ParseFunction(methodNode);
            Assert.Equal("DoTask", op.OperationId);

            var emitted = global::Cdd.OpenApi.Functions.Emit.EmitFunction(op);
            Assert.Contains("DoTask", emitted.ToFullString());

            // Branch coverage
            var opWithoutId = new OpenApiOperation { OperationId = null, Summary = null };
            var emittedWithoutId = global::Cdd.OpenApi.Functions.Emit.EmitFunction(opWithoutId);
            Assert.Contains("DefaultFunction", emittedWithoutId.ToFullString());
        }

        [Fact]
        public void TestMocks()
        {
            var code = "public class TestMock { public void GetUser() {} }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>().First();
            
            var parsedPaths = global::Cdd.OpenApi.Mocks.Parse.ToPaths(classNode);
            Assert.NotEmpty(parsedPaths);

            var paths = new OpenApiPaths();
            paths.Add("/test", new OpenApiPathItem { Get = new OpenApiOperation(), Post = new OpenApiOperation(), Put = new OpenApiOperation(), Delete = new OpenApiOperation() });
            global::Cdd.OpenApi.Mocks.Emit.ToMock("TestMock", paths);
        }

        [Fact]
        public void TestTests()
        {
            var code = "public class TestTests { public void GetUserTest() {} }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>().First();

            var parsedPaths = global::Cdd.OpenApi.TestsModule.Parse.ToPaths(classNode);
            Assert.NotEmpty(parsedPaths);

            var paths = new OpenApiPaths();
            paths.Add("/test", new OpenApiPathItem { Get = new OpenApiOperation(), Post = new OpenApiOperation(), Put = new OpenApiOperation(), Delete = new OpenApiOperation() });
            global::Cdd.OpenApi.TestsModule.Emit.ToTests("TestTests", paths);
        }
    }
}