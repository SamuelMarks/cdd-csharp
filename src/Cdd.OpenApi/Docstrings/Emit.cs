using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cdd.OpenApi.Docstrings
{
/// <summary>Auto-generated documentation for Emit.</summary>
    public static class Emit
    {
/// <summary>Auto-generated documentation for CreateSummary.</summary>
        public static SyntaxTriviaList CreateSummary(string summaryText)
        {
            if (string.IsNullOrWhiteSpace(summaryText)) return SyntaxFactory.TriviaList();
            
            var xml = $"/// <summary>\n/// {summaryText.Replace("\n", "\n/// ")}\n/// </summary>\n";
            return SyntaxFactory.ParseLeadingTrivia(xml);
        }

/// <summary>Auto-generated documentation for WithSummary.</summary>
        public static TNode WithSummary<TNode>(TNode node, string summaryText) where TNode : SyntaxNode
        {
            var summaryTrivia = CreateSummary(summaryText);
            var existingTrivia = node.GetLeadingTrivia();
            return node.WithLeadingTrivia(summaryTrivia.AddRange(existingTrivia));
        }
    }
}
