using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Classes
{
    /// <summary>Auto-generated documentation for Emit.</summary>
    public static class Emit
    {
        /// <summary>Auto-generated documentation for ToClass.</summary>
        public static ClassDeclarationSyntax ToClass(string className, OpenApiSchema schema)
        {
            var classNode = SyntaxFactory.ClassDeclaration(className)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            if (!string.IsNullOrWhiteSpace(schema.Description))
            {
                classNode = Docstrings.Emit.WithSummary(classNode, schema.Description);
            }
            if (schema.Example != null)
            {
                classNode = Docstrings.Emit.WithTag(classNode, "example", schema.Example.ToString()!);
            }
            if (schema.Discriminator?.PropertyName != null)
            {
                classNode = Docstrings.Emit.WithTag(classNode, "discriminator", schema.Discriminator.PropertyName);
                if (schema.Discriminator.DefaultMapping != null)
                {
                    classNode = Docstrings.Emit.WithTag(classNode, "discriminator-defaultMapping", schema.Discriminator.DefaultMapping);
                }
                if (schema.Discriminator.Mapping != null && schema.Discriminator.Mapping.Any())
                {
                    var mappingStr = string.Join(", ", schema.Discriminator.Mapping.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
                    classNode = Docstrings.Emit.WithTag(classNode, "discriminator-mapping", mappingStr);
                }
            }
            if (schema.Xml != null)
            {
                if (schema.Xml.Name != null) classNode = Docstrings.Emit.WithTag(classNode, "xml-name", schema.Xml.Name);
                if (schema.Xml.Namespace != null) classNode = Docstrings.Emit.WithTag(classNode, "xml-namespace", schema.Xml.Namespace);
                if (schema.Xml.Prefix != null) classNode = Docstrings.Emit.WithTag(classNode, "xml-prefix", schema.Xml.Prefix);
                if (schema.Xml.NodeType != null) classNode = Docstrings.Emit.WithTag(classNode, "xml-nodeType", schema.Xml.NodeType);
                if (schema.Xml.Attribute != null) classNode = Docstrings.Emit.WithTag(classNode, "xml-attribute", schema.Xml.Attribute.Value.ToString().ToLower());
                if (schema.Xml.Wrapped != null) classNode = Docstrings.Emit.WithTag(classNode, "xml-wrapped", schema.Xml.Wrapped.Value.ToString().ToLower());
            }
            if (schema.ExternalDocs?.Url != null)
            {
                classNode = Docstrings.Emit.WithTag(classNode, "externalDocs", schema.ExternalDocs.Url);
            }

            if (schema.Properties != null)
            {
                foreach (var prop in schema.Properties)
                {
                    var propName = prop.Key;
                    var propSchema = prop.Value;
                    
                    var isRequired = schema.Required != null && schema.Required.Contains(propName);
                    var isKey = propName.Equals("id", System.StringComparison.OrdinalIgnoreCase);
                    
                    var csharpType = MapTypeToCSharp(propSchema.Type);
                    
                    if (!isRequired && csharpType != "object" && csharpType != "string")
                    {
                        csharpType += "?";
                    }
                    else if (!isRequired && csharpType == "string")
                    {
                        csharpType += "?";
                    }

                    var propNode = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName(csharpType), propName)
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddAccessorListAccessors(
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                        );

                    var attributes = new List<AttributeSyntax>();

                    if (isKey)
                    {
                        attributes.Add(SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Key")));
                    }
                    if (isRequired)
                    {
                        attributes.Add(SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Required")));
                    }

                    if (attributes.Any())
                    {
                        var attributeList = SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(attributes));
                        propNode = propNode.AddAttributeLists(attributeList);
                    }

                    if (!string.IsNullOrWhiteSpace(propSchema.Description))
                    {
                        propNode = Docstrings.Emit.WithSummary(propNode, propSchema.Description);
                    }
                    if (propSchema.Example != null)
                    {
                        propNode = Docstrings.Emit.WithTag(propNode, "example", propSchema.Example.ToString()!);
                    }
                    if (propSchema.Discriminator?.PropertyName != null)
                    {
                        propNode = Docstrings.Emit.WithTag(propNode, "discriminator", propSchema.Discriminator.PropertyName);
                        if (propSchema.Discriminator.DefaultMapping != null)
                        {
                            propNode = Docstrings.Emit.WithTag(propNode, "discriminator-defaultMapping", propSchema.Discriminator.DefaultMapping);
                        }
                        if (propSchema.Discriminator.Mapping != null && propSchema.Discriminator.Mapping.Any())
                        {
                            var mappingStr = string.Join(", ", propSchema.Discriminator.Mapping.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
                            propNode = Docstrings.Emit.WithTag(propNode, "discriminator-mapping", mappingStr);
                        }
                    }
                    if (propSchema.Xml != null)
                    {
                        if (propSchema.Xml.Name != null) propNode = Docstrings.Emit.WithTag(propNode, "xml-name", propSchema.Xml.Name);
                        if (propSchema.Xml.Namespace != null) propNode = Docstrings.Emit.WithTag(propNode, "xml-namespace", propSchema.Xml.Namespace);
                        if (propSchema.Xml.Prefix != null) propNode = Docstrings.Emit.WithTag(propNode, "xml-prefix", propSchema.Xml.Prefix);
                        if (propSchema.Xml.NodeType != null) propNode = Docstrings.Emit.WithTag(propNode, "xml-nodeType", propSchema.Xml.NodeType);
                        if (propSchema.Xml.Attribute != null) propNode = Docstrings.Emit.WithTag(propNode, "xml-attribute", propSchema.Xml.Attribute.Value.ToString().ToLower());
                        if (propSchema.Xml.Wrapped != null) propNode = Docstrings.Emit.WithTag(propNode, "xml-wrapped", propSchema.Xml.Wrapped.Value.ToString().ToLower());
                    }
                    if (propSchema.ExternalDocs?.Url != null)
                    {
                        propNode = Docstrings.Emit.WithTag(propNode, "externalDocs", propSchema.ExternalDocs.Url);
                    }

                    classNode = classNode.AddMembers(propNode);
                }
            }

            return classNode.NormalizeWhitespace();
        }

        private static string MapTypeToCSharp(string? openApiType)
        {
            return openApiType switch
            {
                "integer" => "int",
                "number" => "double",
                "boolean" => "bool",
                "string" => "string",
                _ => "object"
            };
        }
    }
}