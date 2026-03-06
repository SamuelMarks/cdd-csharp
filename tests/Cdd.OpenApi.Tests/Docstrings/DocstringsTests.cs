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

        [Fact]
        public void Parse_FullCoverage()
        {
            var code = @"
            /// <mytag a=""b"" c=""d"">
            ///  Tag text 1
            /// </mytag>
            /// <mytag e=""f"">
            ///  Tag text 2
            /// </mytag>
            /// <server url=""s1"">desc1</server>
            /// <server url=""s2"">desc2</server>
            public class A {}
            ";

            var tree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>().First();

            var tags = Cdd.OpenApi.Docstrings.Parse.GetTagsWithAttributes(classNode, "mytag");
            Assert.Equal(2, tags.Count());
            Assert.Equal("b", tags.ElementAt(0).Attributes["a"]);
            Assert.Equal("d", tags.ElementAt(0).Attributes["c"]);
            Assert.Equal("Tag text 1", tags.ElementAt(0).Text);
            
            Assert.Equal("f", tags.ElementAt(1).Attributes["e"]);
            Assert.Equal("Tag text 2", tags.ElementAt(1).Text);

            var servers = Cdd.OpenApi.Docstrings.Parse.GetServers(classNode);
            Assert.Equal(2, servers.Count());
            Assert.Equal("s1", servers.ElementAt(0).Url);
            Assert.Equal("desc1", servers.ElementAt(0).Description);
            Assert.Equal("s2", servers.ElementAt(1).Url);
            Assert.Equal("desc2", servers.ElementAt(1).Description);
        }
    }
}
