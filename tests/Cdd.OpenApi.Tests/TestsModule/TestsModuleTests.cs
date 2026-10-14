using System.Linq;
using Cdd.OpenApi.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Cdd.OpenApi.Tests.TestsModule
{
    public class TestsModuleTests
    {
        [Fact]
        public void ParseAndEmitTests_CoversAllModesAndVerbs()
        {
            var code = @"
            public class TestTests
            {
                public void GetUserTest() {}
                public void PostUserTest() {}
                public void PutUserTest() {}
                public void DeleteUserTest() {}
                public void PostTest() {}
                public void PutTest() {}
                public void DeleteTest() {}
                public void RawOp() {}
            }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().GetDescendantNodesSafe().OfType<ClassDeclarationSyntax>().First();

            var parsedPaths = global::Cdd.OpenApi.TestsModule.Parse.ToPaths(classNode);
            Assert.Equal(8, parsedPaths.Count);
            Assert.Equal("GetUser", parsedPaths["/getuser"].Get?.OperationId);
            Assert.Equal("PostUser", parsedPaths["/postuser"].Post?.OperationId);
            Assert.Equal("PutUser", parsedPaths["/putuser"].Put?.OperationId);
            Assert.Equal("DeleteUser", parsedPaths["/deleteuser"].Delete?.OperationId);
            Assert.Equal("Post", parsedPaths["/post"].Get?.OperationId);
            Assert.Equal("Put", parsedPaths["/put"].Get?.OperationId);
            Assert.Equal("Delete", parsedPaths["/delete"].Get?.OperationId);
            Assert.Equal("RawOp", parsedPaths["/rawop"].Get?.OperationId);

            var paths = new OpenApiPaths
            {
                ["/test"] = new OpenApiPathItem
                {
                    Get = new OpenApiOperation { OperationId = "GetItem" },
                    Post = new OpenApiOperation(),
                    Put = new OpenApiOperation(),
                    Delete = new OpenApiOperation()
                }
            };

            var testsStub = global::Cdd.OpenApi.TestsModule.Emit.ToTests("TestApiTests", paths, tests: false);
            var testsStubFormatted = testsStub.ToFormattedString();
            Assert.Equal("TestApiTests", testsStub.Identifier.Text);
            AssertHelper.ContainsNoWhitespace("public void GetItemTest()", testsStubFormatted);
            AssertHelper.ContainsNoWhitespace("public void PosttestTest()", testsStubFormatted);

            var testsFull = global::Cdd.OpenApi.TestsModule.Emit.ToTests("TestApiTests", paths, tests: true);
            var testsFullFormatted = testsFull.ToFormattedString();
            Assert.Contains("_api", testsFullFormatted);
            Assert.Contains("[Xunit.Fact]", testsFullFormatted);
            AssertHelper.ContainsNoWhitespace("public System.Threading.Tasks.Task GetItemTest()", testsFullFormatted);

            var clientTestsStub = global::Cdd.OpenApi.TestsModule.Emit.ToClientTests("TestClientTests", paths, tests: false);
            var clientTestsStubFormatted = clientTestsStub.ToFormattedString();
            Assert.Equal("TestClientTests", clientTestsStub.Identifier.Text);
            AssertHelper.ContainsNoWhitespace("public void GetItemAsyncTest()", clientTestsStubFormatted);

            var clientTestsFull = global::Cdd.OpenApi.TestsModule.Emit.ToClientTests("TestClientTests", paths, tests: true);
            var clientTestsFullFormatted = clientTestsFull.ToFormattedString();
            Assert.Contains("_client", clientTestsFullFormatted);
            Assert.Contains("[Xunit.Fact]", clientTestsFullFormatted);
            AssertHelper.ContainsNoWhitespace("public System.Threading.Tasks.Task GetItemAsyncTest()", clientTestsFullFormatted);
        }
    }
}
