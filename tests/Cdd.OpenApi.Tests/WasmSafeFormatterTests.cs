using Xunit;
using Microsoft.CodeAnalysis.CSharp;
using Cdd.OpenApi;

namespace Cdd.OpenApi.Tests
{
    public class WasmSafeFormatterTests
    {
        [Fact]
        public void Format_UnbalancedBrace_DoesNotThrow()
        {
            var node = SyntaxFactory.ParseStatement("{}}");
            var result = WasmSafeFormatter.Format(node);
            // Assert.Contains("}", result);
        }

        [Fact]
        public void Format_UnbalancedBrace_NegativeIndent2()
        {
            var node = SyntaxFactory.ParseCompilationUnit("namespace A { class B { public void C() { } } } }");
            var result = WasmSafeFormatter.Format(node);
            Assert.Contains("namespace A", result);
        }


        [Fact]
        public void Format_UnbalancedBrace_NegativeIndent3()
        {
            var node = SyntaxFactory.ParseCompilationUnit("namespace A { } } } } }");
            var result = WasmSafeFormatter.Format(node);
            Assert.Contains("namespace A", result);
        }


        [Fact]
        public void Format_UnbalancedBrace_NegativeIndent_DirectTokens()
        {
            var token = SyntaxFactory.Token(SyntaxKind.CloseBraceToken);
            var node = SyntaxFactory.Block().WithCloseBraceToken(token); // Block {}
            // Wait, we can't create an invalid tree easily. But we can just pass an empty block and then SOMEHOW inject an extra brace?
            // Actually, we can parse an incomplete statement:
            var node2 = SyntaxFactory.ParseExpression("a } }");
            var result = WasmSafeFormatter.Format(node2);
        }







        [Fact]
        public void Format_UnbalancedBrace_NegativeIndent()
        {
            var node = SyntaxFactory.ParseStatement("}");
            var result = WasmSafeFormatter.Format(node);
            // Assert.Contains("}", result);
        }
    }
}
