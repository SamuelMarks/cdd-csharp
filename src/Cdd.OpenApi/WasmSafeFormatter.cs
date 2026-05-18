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
            var sb = new StringBuilder();
            int indent = 0;
            bool lastWasLetter = false;

            foreach (var token in node.DescendantTokens())
            {
                if (token.HasLeadingTrivia)
                {
                    foreach (var trivia in token.LeadingTrivia)
                    {
                        var triviaStr = trivia.ToFullString().Trim();
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
                    sb.AppendLine();
                    sb.Append(new string(' ', indent * 4));
                    sb.AppendLine("{");
                    indent++;
                    sb.Append(new string(' ', indent * 4));
                }
                else if (text == "}")
                {
                    sb.AppendLine();
                    indent--;
                    if (indent < 0) indent = 0;
                    sb.Append(new string(' ', indent * 4));
                    sb.AppendLine("}");
                    sb.Append(new string(' ', indent * 4));
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
