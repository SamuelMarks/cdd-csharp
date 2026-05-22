using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;

namespace Cdd.OpenApi.Docstrings
{
    /// <summary>Auto-generated documentation for Emit.</summary>
    public static class Emit
    {
        public static SyntaxTriviaList CreateTag(string tagName, string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return SyntaxFactory.TriviaList();

            var lines = text.Split('\n');
            var trivia = new List<SyntaxTrivia>();
            trivia.Add(SyntaxFactory.Comment($"/// <{tagName}>"));
            trivia.Add(SyntaxFactory.EndOfLine("\n"));
            foreach (var line in lines)
            {
                trivia.Add(SyntaxFactory.Comment($"/// {line}"));
                trivia.Add(SyntaxFactory.EndOfLine("\n"));
            }
            trivia.Add(SyntaxFactory.Comment($"/// </{tagName}>"));
            trivia.Add(SyntaxFactory.EndOfLine("\n"));
            return SyntaxFactory.TriviaList(trivia);
        }

        public static SyntaxTriviaList CreateTagWithAttributes(string tagName, IDictionary<string, string> attributes, string text)
        {
            if (string.IsNullOrWhiteSpace(text) && (attributes == null || attributes.Count == 0)) return SyntaxFactory.TriviaList();
            var attrs = string.Join(" ", attributes.Select(kvp => $"{kvp.Key}=\"{kvp.Value}\""));
            var openTag = string.IsNullOrWhiteSpace(attrs) ? $"<{tagName}>" : $"<{tagName} {attrs}>";

            var trivia = new List<SyntaxTrivia>();
            trivia.Add(SyntaxFactory.Comment($"/// {openTag}"));
            trivia.Add(SyntaxFactory.EndOfLine("\n"));
            if (!string.IsNullOrWhiteSpace(text))
            {
                var lines = text.Split('\n');
                foreach (var line in lines)
                {
                    trivia.Add(SyntaxFactory.Comment($"/// {line}"));
                    trivia.Add(SyntaxFactory.EndOfLine("\n"));
                }
            }
            trivia.Add(SyntaxFactory.Comment($"/// </{tagName}>"));
            trivia.Add(SyntaxFactory.EndOfLine("\n"));
            return SyntaxFactory.TriviaList(trivia);
        }

        public static SyntaxTriviaList CreateSummary(string summaryText)
        {
            return CreateTag("summary", summaryText);
        }

        public static TNode WithSummary<TNode>(TNode node, string summaryText) where TNode : SyntaxNode
        {
            return WithTag(node, "summary", summaryText);
        }

        public static TNode WithTag<TNode>(TNode node, string tagName, string text) where TNode : SyntaxNode
        {
            if (string.IsNullOrWhiteSpace(text)) return node;
            var tagTrivia = CreateTag(tagName, text);
            var existingTrivia = node.GetLeadingTrivia();
            return node.WithLeadingTrivia(tagTrivia.AddRange(existingTrivia));
        }

        public static TNode WithTag<TNode>(TNode node, string tagName, IDictionary<string, string> attributes, string text) where TNode : SyntaxNode
        {
            if (string.IsNullOrWhiteSpace(text) && (attributes == null || attributes.Count == 0)) return node;
            var tagTrivia = CreateTagWithAttributes(tagName, attributes, text);
            var existingTrivia = node.GetLeadingTrivia();
            return node.WithLeadingTrivia(tagTrivia.AddRange(existingTrivia));
        }
    }
}
