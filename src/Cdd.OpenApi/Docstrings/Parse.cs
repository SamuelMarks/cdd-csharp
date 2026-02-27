using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cdd.OpenApi.Docstrings
{
    public static class Parse
    {
        public static string? GetSummary(SyntaxNode node)
        {
            var trivia = node.GetLeadingTrivia()
                .Select(i => i.GetStructure())
                .OfType<DocumentationCommentTriviaSyntax>()
                .FirstOrDefault();

            if (trivia == null) return null;

            var summaryElement = trivia.ChildNodes()
                .OfType<XmlElementSyntax>()
                .FirstOrDefault(e => e.StartTag.Name.ToString() == "summary");

            if (summaryElement == null) return null;

            var textLines = summaryElement.Content
                .OfType<XmlTextSyntax>()
                .SelectMany(t => t.TextTokens)
                .Select(t => t.ValueText.Trim())
                .Where(t => !string.IsNullOrEmpty(t));

            return string.Join(" ", textLines);
        }
    }
}
