namespace Cdd.OpenApi.Tests
{
    public static class TestExtensions
    {
        public static string ToFormattedString(this Microsoft.CodeAnalysis.SyntaxNode node)
        {
             return Cdd.OpenApi.WasmSafeFormatter.Format(node);
        }
    }
}
