using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Cdd.OpenApi
{
    /// <summary>
    /// Utility class providing WASM-safe methods for formatting Roslyn syntax analysis.
    /// Used to avoid deep recursion limitations of the Mono Interpreter in V8.
    /// </summary>
    public static class WasmSafeFormatter
    {
        private static System.Collections.Generic.IEnumerable<SyntaxToken> GetTokens(SyntaxNode root)
        {
            var stack = new System.Collections.Generic.Stack<SyntaxNodeOrToken>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (current.IsToken)
                {
                    yield return current.AsToken();
                }
                else
                {
                    var children = current.AsNode()!.ChildNodesAndTokens();
                    foreach (var child in children.Reverse())
                    {
                        stack.Push(child);
                    }
                }
            }
        }

        /// <summary>Format</summary>
        public static string Format(SyntaxNode node)
        {
            var sb = new StringBuilder();
            int indent = 0;
            bool lastWasLetter = false;

            foreach (var token in GetTokens(node))
            {
                if (token.HasLeadingTrivia)
                {
                    foreach (var trivia in token.LeadingTrivia)
                    {
                        var triviaStr = trivia.ToString().Trim();
                        if (!string.IsNullOrEmpty(triviaStr))
                        {
                            sb.AppendLine();
                            sb.Append(new string(' ', indent * 4));
                            sb.AppendLine(triviaStr);
                            sb.Append(new string(' ', indent * 4));
                        }
                    }
                }

                string text = token.Text;
                if (string.IsNullOrEmpty(text)) continue;

                bool isLetter = char.IsLetterOrDigit(text[0]) || text[0] == '_';
                if (lastWasLetter && isLetter) sb.Append(" ");

                if (text == "{")
                {
                    if (token.Parent is Microsoft.CodeAnalysis.CSharp.Syntax.InterpolationSyntax)
                    {
                        sb.Append("{");
                    }
                    else
                    {
                        sb.AppendLine();
                        sb.Append(new string(' ', indent * 4));
                        sb.AppendLine("{");
                        indent++;
                        sb.Append(new string(' ', indent * 4));
                    }
                }
                else if (text == "}")
                {
                    if (token.Parent is Microsoft.CodeAnalysis.CSharp.Syntax.InterpolationSyntax)
                    {
                        sb.Append("}");
                    }
                    else
                    {
                        sb.AppendLine();
                        indent--;
                        if (indent < 0) indent = 0;
                        sb.Append(new string(' ', indent * 4));
                        sb.AppendLine("}");
                        sb.Append(new string(' ', indent * 4));
                    }
                }
                else if (text == ";")
                {
                    sb.AppendLine(";");
                    sb.Append(new string(' ', indent * 4));
                }
                else if (text == "[")
                {
                    sb.AppendLine();
                    sb.Append(new string(' ', indent * 4));
                    sb.Append("[");
                }
                else if (text == "]")
                {
                    sb.AppendLine("]");
                    sb.Append(new string(' ', indent * 4));
                }
                else if (text == "get" || text == "set")
                {
                    sb.Append(" " + text + " ");
                }
                else
                {
                    sb.Append(text);
                }

                lastWasLetter = char.IsLetterOrDigit(text[text.Length - 1]) || text[text.Length - 1] == '_';
            }
            return sb.ToString();
        }
    }
}
