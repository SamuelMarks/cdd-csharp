using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;

namespace Cdd.OpenApi.Docstrings
{
    /// <summary>Auto-generated documentation for Emit.</summary>
    public static class Emit
    {
        /// <summary>
        /// Creates a SyntaxTriviaList containing a structured XML doc comment for a given tag.
        /// </summary>
        /// <param name="tagName">The name of the XML tag.</param>
        /// <param name="text">The inner text for the tag.</param>
        /// <returns>A SyntaxTriviaList with the generated comment.</returns>
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

        /// <summary>
        /// Creates a SyntaxTriviaList containing a structured XML doc comment for a given tag, including attributes.
        /// </summary>
        /// <param name="tagName">The name of the XML tag.</param>
        /// <param name="attributes">A dictionary of attributes for the tag.</param>
        /// <param name="text">The inner text for the tag.</param>
        /// <returns>A SyntaxTriviaList with the generated comment.</returns>
        public static SyntaxTriviaList CreateTagWithAttributes(string tagName, IDictionary<string, string> attributes, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                if (attributes == null || attributes.Count == 0) return SyntaxFactory.TriviaList();
            }
            var attrs = string.Join(" ", attributes.Select(kvp => $"{kvp.Key}=\"{kvp.Value}\""));
            var openTag = $"<{tagName}>";
            if (!string.IsNullOrWhiteSpace(attrs))
            {
                openTag = $"<{tagName} {attrs}>";
            }

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

        /// <summary>
        /// Creates a SyntaxTriviaList for a summary tag.
        /// </summary>
        /// <param name="summaryText">The text of the summary.</param>
        /// <returns>A SyntaxTriviaList with the summary comment.</returns>
        public static SyntaxTriviaList CreateSummary(string summaryText)
        {
            return CreateTag("summary", summaryText);
        }

        /// <summary>
        /// Attaches a summary tag to a syntax node.
        /// </summary>
        /// <typeparam name="TNode">The type of the syntax node.</typeparam>
        /// <param name="node">The syntax node.</param>
        /// <param name="summaryText">The text of the summary.</param>
        /// <returns>The syntax node with the summary attached.</returns>
        public static TNode WithSummary<TNode>(TNode node, string summaryText) where TNode : SyntaxNode
        {
            return WithTag(node, "summary", summaryText);
        }

        /// <summary>
        /// Attaches an XML tag to a syntax node.
        /// </summary>
        /// <typeparam name="TNode">The type of the syntax node.</typeparam>
        /// <param name="node">The syntax node.</param>
        /// <param name="tagName">The name of the XML tag.</param>
        /// <param name="text">The inner text for the tag.</param>
        /// <returns>The syntax node with the tag attached.</returns>
        public static TNode WithTag<TNode>(TNode node, string tagName, string text) where TNode : SyntaxNode
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return node;
            }
            var tagTrivia = CreateTag(tagName, text);
            var existingTrivia = node.GetLeadingTrivia();
            return node.WithLeadingTrivia(tagTrivia.AddRange(existingTrivia));
        }

        /// <summary>
        /// Attaches an XML tag with attributes to a syntax node.
        /// </summary>
        /// <typeparam name="TNode">The type of the syntax node.</typeparam>
        /// <param name="node">The syntax node.</param>
        /// <param name="tagName">The name of the XML tag.</param>
        /// <param name="attributes">The attributes for the tag.</param>
        /// <param name="text">The inner text for the tag.</param>
        /// <returns>The syntax node with the tag attached.</returns>
        public static TNode WithTag<TNode>(TNode node, string tagName, IDictionary<string, string> attributes, string text) where TNode : SyntaxNode
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                if (attributes == null || attributes.Count == 0) return node;
            }
            var tagTrivia = CreateTagWithAttributes(tagName, attributes, text);
            var existingTrivia = node.GetLeadingTrivia();
            return node.WithLeadingTrivia(tagTrivia.AddRange(existingTrivia));
        }
    }
}
