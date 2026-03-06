using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cdd.OpenApi.Docstrings
{
/// <summary>Auto-generated documentation for Parse.</summary>
    public static class Parse
    {
/// <summary>Auto-generated documentation for GetSummary.</summary>
        public static string? GetSummary(SyntaxNode node)
        {
            return GetTag(node, "summary");
        }

/// <summary>Auto-generated documentation for GetTag.</summary>
        public static string? GetTag(SyntaxNode node, string tagName)
        {
            var trivia = node.GetLeadingTrivia()
                .Select(i => i.GetStructure())
                .OfType<DocumentationCommentTriviaSyntax>()
                .FirstOrDefault();

            if (trivia == null) return null;

            var element = trivia.ChildNodes()
                .OfType<XmlElementSyntax>()
                .FirstOrDefault(e => e.StartTag.Name.ToString() == tagName);

            if (element == null) return null;

            var textLines = element.Content
                .OfType<XmlTextSyntax>()
                .SelectMany(t => t.TextTokens)
                .Select(t => t.ValueText.Trim())
                .Where(t => !string.IsNullOrEmpty(t));

            return string.Join(" ", textLines);
        }

/// <summary>Auto-generated documentation for GetTagsWithAttributes.</summary>
        public static IEnumerable<(IDictionary<string, string> Attributes, string Text)> GetTagsWithAttributes(SyntaxNode node, string tagName)
        {
            var trivia = node.GetLeadingTrivia()
                .Select(i => i.GetStructure())
                .OfType<DocumentationCommentTriviaSyntax>()
                .FirstOrDefault();

            if (trivia == null) yield break;

            var elements = trivia.ChildNodes()
                .OfType<XmlElementSyntax>()
                .Where(e => e.StartTag.Name.ToString() == tagName);

            foreach (var element in elements)
            {
                var attrs = new Dictionary<string, string>();
                foreach (var attr in element.StartTag.Attributes.OfType<XmlNameAttributeSyntax>())
                {
                    attrs[attr.Name.ToString()] = attr.Identifier.Identifier.ValueText;
                }
                foreach (var attr in element.StartTag.Attributes.OfType<XmlTextAttributeSyntax>())
                {
                    attrs[attr.Name.ToString()] = string.Join("", attr.TextTokens.Select(t => t.ValueText));
                }

                var textLines = element.Content
                    .OfType<XmlTextSyntax>()
                    .SelectMany(t => t.TextTokens)
                    .Select(t => t.ValueText.Trim())
                    .Where(t => !string.IsNullOrEmpty(t));

                yield return (attrs, string.Join(" ", textLines));
            }
        }

        /// <summary>Auto-generated documentation for GetServers.</summary>
        public static IEnumerable<Cdd.OpenApi.Models.OpenApiServer> GetServers(SyntaxNode node)
        {
            var trivia = node.GetLeadingTrivia()
                .Select(i => i.GetStructure())
                .OfType<DocumentationCommentTriviaSyntax>()
                .FirstOrDefault();

            if (trivia == null) yield break;

            var elements = trivia.ChildNodes()
                .OfType<XmlElementSyntax>()
                .Where(e => e.StartTag.Name.ToString() == "server");

            foreach (var element in elements)
            {
                var attrs = new Dictionary<string, string>();
                foreach (var attr in element.StartTag.Attributes.OfType<XmlNameAttributeSyntax>())
                {
                    attrs[attr.Name.ToString()] = attr.Identifier.Identifier.ValueText;
                }
                foreach (var attr in element.StartTag.Attributes.OfType<XmlTextAttributeSyntax>())
                {
                    attrs[attr.Name.ToString()] = string.Join("", attr.TextTokens.Select(t => t.ValueText));
                }

                var textLines = element.Content
                    .OfType<XmlTextSyntax>()
                    .SelectMany(t => t.TextTokens)
                    .Select(t => t.ValueText.Trim())
                    .Where(t => !string.IsNullOrEmpty(t));

                var server = new Cdd.OpenApi.Models.OpenApiServer
                {
                    Url = attrs.TryGetValue("url", out var u) ? u : "/",
                    Description = string.Join(" ", textLines),
                    Name = attrs.TryGetValue("name", out var n) ? n : null,
                    Variables = new Dictionary<string, Cdd.OpenApi.Models.OpenApiServerVariable>()
                };

                // Find variables
                var varElements = element.Content.OfType<XmlElementSyntax>().Where(e => e.StartTag.Name.ToString() == "variable");
                foreach (var varElem in varElements)
                {
                    var varAttrs = new Dictionary<string, string>();
                    foreach (var attr in varElem.StartTag.Attributes.OfType<XmlNameAttributeSyntax>())
                    {
                        varAttrs[attr.Name.ToString()] = attr.Identifier.Identifier.ValueText;
                    }
                    foreach (var attr in varElem.StartTag.Attributes.OfType<XmlTextAttributeSyntax>())
                    {
                        varAttrs[attr.Name.ToString()] = string.Join("", attr.TextTokens.Select(t => t.ValueText));
                    }

                    var varName = varAttrs.TryGetValue("name", out var vn) ? vn : "var";
                    var enumElements = varElem.Content.OfType<XmlElementSyntax>().Where(e => e.StartTag.Name.ToString() == "enum");
                    
                    var varTextLines = varElem.Content
                        .OfType<XmlTextSyntax>()
                        .SelectMany(t => t.TextTokens)
                        .Select(t => t.ValueText.Trim())
                        .Where(t => !string.IsNullOrEmpty(t));

                    var varDesc = string.Join(" ", varTextLines);
                    if (string.IsNullOrEmpty(varDesc)) varDesc = varAttrs.TryGetValue("description", out var vd) ? vd : null;

                    var enums = new List<string>();
                    foreach (var enumElem in enumElements)
                    {
                        var eText = string.Join(" ", enumElem.Content.OfType<XmlTextSyntax>().SelectMany(t => t.TextTokens).Select(t => t.ValueText.Trim()).Where(t => !string.IsNullOrEmpty(t)));
                        if (!string.IsNullOrEmpty(eText)) enums.Add(eText);
                    }

                    server.Variables[varName] = new Cdd.OpenApi.Models.OpenApiServerVariable
                    {
                        Default = varAttrs.TryGetValue("default", out var d) ? d : "",
                        Description = varDesc,
                        Enum = enums.Any() ? enums : null
                    };
                }

                if (!server.Variables.Any()) server.Variables = null;

                yield return server;
            }
        }
    }
}

