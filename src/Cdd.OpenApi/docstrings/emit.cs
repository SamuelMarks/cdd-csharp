using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;

namespace Cdd.OpenApi.Docstrings
{
    /// <summary>Auto-generated documentation for Emit.</summary>
    public static class Emit
    {
        /// <summary>Auto-generated documentation for CreateTag.</summary>
        public static SyntaxTriviaList CreateTag(string tagName, string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return SyntaxFactory.TriviaList();
            
            var xml = $"/// <{tagName}>\n/// {text.Replace("\n", "\n/// ")}\n/// </{tagName}>\n";
            return SyntaxFactory.ParseLeadingTrivia(xml);
        }

        /// <summary>Auto-generated documentation for CreateTagWithAttributes.</summary>
        public static SyntaxTriviaList CreateTagWithAttributes(string tagName, IDictionary<string, string> attributes, string text)
        {
            if (string.IsNullOrWhiteSpace(text) && (attributes == null || attributes.Count == 0)) return SyntaxFactory.TriviaList();
            var attrs = string.Join(" ", attributes.Select(kvp => $"{kvp.Key}=\"{kvp.Value}\""));
            var openTag = string.IsNullOrWhiteSpace(attrs) ? $"<{tagName}>" : $"<{tagName} {attrs}>";
            var textBody = string.IsNullOrWhiteSpace(text) ? "" : $"\n/// {text.Replace("\n", "\n/// ")}";
            var xml = $"/// {openTag}{textBody}\n/// </{tagName}>\n";
            return SyntaxFactory.ParseLeadingTrivia(xml);
        }

        /// <summary>Auto-generated documentation for CreateSummary.</summary>
        public static SyntaxTriviaList CreateSummary(string summaryText)
        {
            return CreateTag("summary", summaryText);
        }

        /// <summary>Auto-generated documentation for WithSummary.</summary>
        public static TNode WithSummary<TNode>(TNode node, string summaryText) where TNode : SyntaxNode
        {
            return WithTag(node, "summary", summaryText);
        }

        /// <summary>Auto-generated documentation for WithTag.</summary>
        public static TNode WithTag<TNode>(TNode node, string tagName, string text) where TNode : SyntaxNode
        {
            if (string.IsNullOrWhiteSpace(text)) return node;
            var tagTrivia = CreateTag(tagName, text);
            var existingTrivia = node.GetLeadingTrivia();
            return node.WithLeadingTrivia(tagTrivia.AddRange(existingTrivia));
        }

        /// <summary>Auto-generated documentation for WithTag with attributes.</summary>
        public static TNode WithTag<TNode>(TNode node, string tagName, IDictionary<string, string> attributes, string text) where TNode : SyntaxNode
        {
            if (string.IsNullOrWhiteSpace(text) && (attributes == null || attributes.Count == 0)) return node;
            var tagTrivia = CreateTagWithAttributes(tagName, attributes, text);
            var existingTrivia = node.GetLeadingTrivia();
            return node.WithLeadingTrivia(tagTrivia.AddRange(existingTrivia));
        }
    }
}
