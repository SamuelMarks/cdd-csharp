using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using Cdd.OpenApi.Docstrings;
using System.Linq;
using System.Collections.Generic;

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
            /// <mytag a=""b c"" c=""123"" unquoted=""abc"">
            ///  Tag text 1
            /// </mytag>
            /// <mytag e=""f! "">
            ///  Tag text 2
            /// </mytag>
            /// <server url=""s1/test"">desc1</server>
            /// <server url=""s2"">desc2</server>
            /// <server url=""s3"" unquoted=""xyz""><variable name=""var"" default=""def"" a=""text val"" unquotedvar=""qwe"">Some text<enum>1</enum><enum>2</enum></variable></server>
            /// <param name=""myParam"">Param desc</param>
            public class A {}
            ";

            var tree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>().First();

            var tags = Cdd.OpenApi.Docstrings.Parse.GetTagsWithAttributes(classNode, "mytag");
            var paramTags = Cdd.OpenApi.Docstrings.Parse.GetTagsWithAttributes(classNode, "param");
            Assert.Equal("myParam", paramTags.First().Attributes["name"]);
            Assert.Equal("Param desc", paramTags.First().Text);
            Assert.Equal(2, tags.Count());
            Assert.Equal("b c", tags.ElementAt(0).Attributes["a"]);
            Assert.Equal("123", tags.ElementAt(0).Attributes["c"]);
            Assert.Equal("abc", tags.ElementAt(0).Attributes["unquoted"]);
            Assert.Equal("Tag text 1", tags.ElementAt(0).Text);

            Assert.Equal("f! ", tags.ElementAt(1).Attributes["e"]);
            Assert.Equal("Tag text 2", tags.ElementAt(1).Text);

            var servers = Cdd.OpenApi.Docstrings.Parse.GetServers(classNode);
            Assert.Equal(3, servers.Count());
            Assert.Equal("s1/test", servers.ElementAt(0).Url);
            Assert.Equal("desc1", servers.ElementAt(0).Description);
            Assert.Equal("s2", servers.ElementAt(1).Url);
            Assert.Equal("desc2", servers.ElementAt(1).Description);
            Assert.Equal("s3", servers.ElementAt(2).Url);
            Assert.Single(servers.ElementAt(2).Variables);
        }

        [Fact]
        public void Emit_WithTag_NullOrEmptyText_ReturnsNode()
        {
            var classNode = SyntaxFactory.ClassDeclaration("TestClass");
            var classWithDocs = Cdd.OpenApi.Docstrings.Emit.WithTag(classNode, "mytag", null!);
            Assert.Same(classNode, classWithDocs);
        }

        [Fact]
        public void Emit_WithTagWithAttributes_NullOrEmptyTextAndNoAttributes_ReturnsNode()
        {
            var classNode = SyntaxFactory.ClassDeclaration("TestClass");

            // Null text, null attributes
            var result1 = Cdd.OpenApi.Docstrings.Emit.WithTag(classNode, "mytag", null!, null!);
            Assert.Same(classNode, result1);

            // Null text, empty attributes
            var emptyAttrs = new Dictionary<string, string>();
            var result2 = Cdd.OpenApi.Docstrings.Emit.WithTag(classNode, "mytag", emptyAttrs, null!);
            Assert.Same(classNode, result2);
        }

        [Fact]
        public void Emit_WithTagWithAttributes_NullOrEmptyTextWithAttributes_AddsTag()
        {
            var classNode = SyntaxFactory.ClassDeclaration("TestClass");

            var attrs = new Dictionary<string, string> { { "a", "b" } };
            var result = Cdd.OpenApi.Docstrings.Emit.WithTag(classNode, "mytag", attrs, null!);

            var code = result.ToFullString();
            Assert.Contains("<mytag a=\"b\">", code);
            Assert.Contains("</mytag>", code);
        }

        [Fact]
        public void Emit_CreateTagWithAttributes_NullOrEmptyTextAndNoAttributes_ReturnsEmpty()
        {
            var result1 = Cdd.OpenApi.Docstrings.Emit.CreateTagWithAttributes("mytag", null!, null!);
            Assert.Empty(result1);

            var emptyAttrs = new Dictionary<string, string>();
            var result2 = Cdd.OpenApi.Docstrings.Emit.CreateTagWithAttributes("mytag", emptyAttrs, null!);
            Assert.Empty(result2);

            var validAttrs = new Dictionary<string, string> { { "a", "b" } };
            var result3 = Cdd.OpenApi.Docstrings.Emit.CreateTagWithAttributes("mytag", validAttrs, null!);
            Assert.NotEmpty(result3);
        }
        [Fact]
        public void Parse_GetServers_EdgeCases()
        {
            var code = @"
            /// <server>Default server</server>
            /// <server url=""""><variable default=""def""><enum></enum></variable></server>
            /// <server url=""http://url""><variable name=""v"" description=""Desc""></variable></server>
            public class B {}
            ";

            var tree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(code);
            var classNode = tree.GetRoot().DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>().First();

            var servers = Cdd.OpenApi.Docstrings.Parse.GetServers(classNode).ToList();
            Assert.Equal(3, servers.Count);

            Assert.Equal("/", servers[0].Url);
            Assert.Equal("Default server", servers[0].Description);

            Assert.Equal("", servers[1].Url);
            var v1 = servers[1].Variables["var"];
            Assert.Equal("def", v1.Default);

            var v2 = servers[2].Variables["v"];
            Assert.Equal("Desc", v2.Description);
        }
    }
}
