// <copyright file="MocksTests.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Linq;
using Cdd.OpenApi.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Cdd.OpenApi.Tests.Mocks
{
    /// <summary>Verifies or exercises mocks tests.</summary>
    public class MocksTests
    {
        /// <summary>Verifies or exercises parse and emit mocks covers all modes and verbs.</summary>
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
                public void PatchUser() {}
                public void OptionsUser() {}
                public void HeadUser() {}
                public void TraceUser() {}
                public void QueryUser() {}
                public void Post() {}
                public void Put() {}
                public void Delete() {}
                public void Patch() {}
                public void Options() {}
                public void Head() {}
                public void Trace() {}
                public void Query() {}
            }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().GetDescendantNodesSafe().OfType<ClassDeclarationSyntax>().First();

            var parsedPaths = global::Cdd.OpenApi.Mocks.Parse.ToPaths(classNode);
            Assert.Equal(17, parsedPaths.Count);
            Assert.Equal("GetUser", parsedPaths["/getuser"].Get?.OperationId);
            Assert.Equal("PostUser", parsedPaths["/postuser"].Post?.OperationId);
            Assert.Equal("PutUser", parsedPaths["/putuser"].Put?.OperationId);
            Assert.Equal("DeleteUser", parsedPaths["/deleteuser"].Delete?.OperationId);
            Assert.Equal("PatchUser", parsedPaths["/patchuser"].Patch?.OperationId);
            Assert.Equal("OptionsUser", parsedPaths["/optionsuser"].Options?.OperationId);
            Assert.Equal("HeadUser", parsedPaths["/headuser"].Head?.OperationId);
            Assert.Equal("TraceUser", parsedPaths["/traceuser"].Trace?.OperationId);
            Assert.Equal("QueryUser", parsedPaths["/queryuser"].Query?.OperationId);
            Assert.Equal("Post", parsedPaths["/post"].Get?.OperationId);
            Assert.Equal("Put", parsedPaths["/put"].Get?.OperationId);
            Assert.Equal("Delete", parsedPaths["/delete"].Get?.OperationId);
            Assert.Equal("Patch", parsedPaths["/patch"].Get?.OperationId);
            Assert.Equal("Options", parsedPaths["/options"].Get?.OperationId);
            Assert.Equal("Head", parsedPaths["/head"].Get?.OperationId);
            Assert.Equal("Trace", parsedPaths["/trace"].Get?.OperationId);
            Assert.Equal("Query", parsedPaths["/query"].Get?.OperationId);

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
