using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Cdd.OpenApi
{
    public static class WasmSafeFormatter
    {
        public static string Format(SyntaxNode node)
        {
            return node.NormalizeWhitespace().ToFullString();
        }
    }
}
