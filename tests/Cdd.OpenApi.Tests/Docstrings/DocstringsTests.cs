using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using Cdd.OpenApi.Docstrings;
using System.Linq;

namespace Cdd.OpenApi.Tests.Docstrings
{
    public class DocstringsTests
    {
        [Fact]
        public void Parse_GetSummary_ExtractsTextCorrectly()
        {
            var code = @"
            /// <summary>
            /// This is a test summary.
            /// It spans multiple lines.
            /// </summary>
            public class TestClass {}";

            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var summary = Cdd.OpenApi.Docstrings.Parse.GetSummary(classNode);

            Assert.Equal("This is a test summary. It spans multiple lines.", summary);
        }

        [Fact]
        public void Parse_GetSummary_NoSummary_ReturnsNull()
        {
            var code = @"public class TestClass {}";
            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var summary = Cdd.OpenApi.Docstrings.Parse.GetSummary(classNode);

            Assert.Null(summary);
        }
        
        [Fact]
        public void Parse_GetSummary_NoSummaryElement_ReturnsNull()
        {
            var code = @"
            /// <remarks>
            /// Just remarks
            /// </remarks>
            public class TestClass {}";
            var tree = CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var summary = Cdd.OpenApi.Docstrings.Parse.GetSummary(classNode);

            Assert.Null(summary);
        }

        [Fact]
        public void Emit_CreateSummary_CreatesValidTrivia()
        {
            var trivia = Cdd.OpenApi.Docstrings.Emit.CreateSummary("A generated summary");
            var triviaString = trivia.ToFullString();

            Assert.Contains("<summary>", triviaString);
            Assert.Contains("A generated summary", triviaString);
            Assert.Contains("</summary>", triviaString);
        }
        
        [Fact]
        public void Emit_CreateSummary_NullOrEmpty_ReturnsEmptyList()
        {
            var trivia = Cdd.OpenApi.Docstrings.Emit.CreateSummary("");
            Assert.Empty(trivia);
        }

        [Fact]
        public void Emit_WithSummary_AttachesToNode()
        {
            var classNode = SyntaxFactory.ClassDeclaration("TestClass");
            var classWithDocs = Cdd.OpenApi.Docstrings.Emit.WithSummary(classNode, "New summary");

            var code = classWithDocs.ToFullString();
            Assert.Contains("/// <summary>", code);
            Assert.Contains("/// New summary", code);
            Assert.Contains("TestClass", code);
        }
    }
}
