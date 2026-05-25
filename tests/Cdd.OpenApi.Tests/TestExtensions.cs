using Microsoft.CodeAnalysis;
namespace Cdd.OpenApi.Tests
{
    public static class TestExtensions
    {
        public static string ToFormattedString(this Microsoft.CodeAnalysis.SyntaxNode node)
        {
            return node.NormalizeWhitespace().ToFullString();
        }
    }
}
