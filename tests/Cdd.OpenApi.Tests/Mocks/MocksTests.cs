using System.Linq;
using Cdd.OpenApi.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Cdd.OpenApi.Tests.Mocks
{
    public class MocksTests
    {
        [Fact]
        public void ParseAndEmitMocks_CoversAllModesAndVerbs()
        {
            var code = @"
            public class TestMock
            {
                public void GetUser() {}
                public void PostUser() {}
                public void PutUser() {}
                public void DeleteUser() {}
                public void Post() {}
                public void Put() {}
                public void Delete() {}
            }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().GetDescendantNodesSafe().OfType<ClassDeclarationSyntax>().First();

            var parsedPaths = global::Cdd.OpenApi.Mocks.Parse.ToPaths(classNode);
            Assert.Equal(7, parsedPaths.Count);
            Assert.Equal("GetUser", parsedPaths["/getuser"].Get?.OperationId);
            Assert.Equal("PostUser", parsedPaths["/postuser"].Post?.OperationId);
            Assert.Equal("PutUser", parsedPaths["/putuser"].Put?.OperationId);
            Assert.Equal("DeleteUser", parsedPaths["/deleteuser"].Delete?.OperationId);
            Assert.Equal("Post", parsedPaths["/post"].Get?.OperationId);
            Assert.Equal("Put", parsedPaths["/put"].Get?.OperationId);
            Assert.Equal("Delete", parsedPaths["/delete"].Get?.OperationId);

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

            var mockNode = global::Cdd.OpenApi.Mocks.Emit.ToMock("TestMock", paths, tests: false);
            var mockFormatted = mockNode.ToFormattedString();
            Assert.Equal("TestMock", mockNode.Identifier.Text);
            AssertHelper.ContainsNoWhitespace("public object GetItem()", mockFormatted);
            AssertHelper.ContainsNoWhitespace("public object Posttest()", mockFormatted);
            AssertHelper.ContainsNoWhitespace("public object Puttest()", mockFormatted);
            AssertHelper.ContainsNoWhitespace("public object Deletetest()", mockFormatted);

            var testableMockNode = global::Cdd.OpenApi.Mocks.Emit.ToMock("TestApiMock", paths, tests: true);
            var testableFormatted = testableMockNode.ToFormattedString();
            Assert.Equal("TestApiMock", testableMockNode.Identifier.Text);
            Assert.Contains("ITestApi", testableFormatted);
            AssertHelper.ContainsNoWhitespace("public void GetItem()", testableFormatted);
        }
    }
}
