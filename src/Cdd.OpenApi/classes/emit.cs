// <copyright file="emit.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Cdd.OpenApi.Common;
using Cdd.OpenApi.Ir;
using Cdd.OpenApi.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cdd.OpenApi.Classes
{
    /// <summary>Emits C# model class syntax declarations from <see cref="IrModel"/> and <see cref="OpenApiSchema"/> definitions.</summary>
    public static class Emit
    {
        /// <summary>
        /// Converts an <see cref="OpenApiSchema"/> into a Roslyn <see cref="ClassDeclarationSyntax"/>.
        /// </summary>
        /// <param name="className">The name of the C# class to generate.</param>
        /// <param name="schema">The OpenAPI schema defining the class properties and metadata.</param>
        /// <returns>The generated <see cref="ClassDeclarationSyntax"/>.</returns>
        public static ClassDeclarationSyntax ToClass(string className, OpenApiSchema schema)
        {
            return ToClass(IrModel.FromOpenApiSchema(className, schema));
        }

        /// <summary>
        /// Converts an <see cref="IrModel"/> into a Roslyn <see cref="ClassDeclarationSyntax"/>.
        /// </summary>
        /// <param name="model">The IR model defining the class fields and metadata.</param>
        /// <returns>The generated <see cref="ClassDeclarationSyntax"/>.</returns>
        public static ClassDeclarationSyntax ToClass(IrModel model)
        {
            var classNode = SyntaxFactory.ClassDeclaration(model.Name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            if (!string.IsNullOrWhiteSpace(model.Description))
            {
                classNode = Docstrings.Emit.WithSummary(classNode, model.Description);
            }

            if (model.Example != null)
            {
                classNode = Docstrings.Emit.WithTag(classNode, "example", model.Example.ToString()!);
            }

            if (model.Discriminator?.PropertyName != null)
            {
                classNode = Docstrings.Emit.WithTag(classNode, "discriminator", model.Discriminator.PropertyName);
                if (model.Discriminator.DefaultMapping != null)
                {
                    classNode = Docstrings.Emit.WithTag(classNode, "discriminator-defaultMapping", model.Discriminator.DefaultMapping);
                }

                if (model.Discriminator.Mapping != null && model.Discriminator.Mapping.Any())
                {
                    var mappingStr = string.Join(", ", model.Discriminator.Mapping.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
                    classNode = Docstrings.Emit.WithTag(classNode, "discriminator-mapping", mappingStr);
                }
            }

            if (model.Xml != null)
            {
                if (model.Xml.Name != null) classNode = Docstrings.Emit.WithTag(classNode, "xml-name", model.Xml.Name);
                if (model.Xml.Namespace != null) classNode = Docstrings.Emit.WithTag(classNode, "xml-namespace", model.Xml.Namespace);
                if (model.Xml.Prefix != null) classNode = Docstrings.Emit.WithTag(classNode, "xml-prefix", model.Xml.Prefix);
                if (model.Xml.NodeType != null) classNode = Docstrings.Emit.WithTag(classNode, "xml-nodeType", model.Xml.NodeType);
                if (model.Xml.Attribute != null) classNode = Docstrings.Emit.WithTag(classNode, "xml-attribute", model.Xml.Attribute.Value.ToString().ToLower());
                if (model.Xml.Wrapped != null) classNode = Docstrings.Emit.WithTag(classNode, "xml-wrapped", model.Xml.Wrapped.Value.ToString().ToLower());
            }

            if (model.ExternalDocs?.Url != null)
            {
                classNode = Docstrings.Emit.WithTag(classNode, "externalDocs", model.ExternalDocs.Url);
            }

            var members = new List<MemberDeclarationSyntax>();
            foreach (var field in model.Fields)
            {
                var propName = field.Name;
                var isRequired = field.IsRequired;
                var isKey = propName.Equals("id", System.StringComparison.OrdinalIgnoreCase);

                var csharpType = TypeMapper.MapOpenApiToCSharp(field.Type.TypeName, "object");
                if (!isRequired && csharpType != "object")
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

                if (csharpType == "object")
                {
                    attributes.Add(SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("System.ComponentModel.DataAnnotations.Schema.NotMapped")));
                }

                if (attributes.Any())
                {
                    var attributeList = SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(attributes));
                    propNode = propNode.AddAttributeLists(attributeList);
                }

                if (!string.IsNullOrWhiteSpace(field.Description))
                {
                    propNode = Docstrings.Emit.WithSummary(propNode, field.Description);
                }

                if (field.Example != null)
                {
                    propNode = Docstrings.Emit.WithTag(propNode, "example", field.Example.ToString()!);
                }

                if (field.Discriminator?.PropertyName != null)
                {
                    propNode = Docstrings.Emit.WithTag(propNode, "discriminator", field.Discriminator.PropertyName);
                    if (field.Discriminator.DefaultMapping != null)
                    {
                        propNode = Docstrings.Emit.WithTag(propNode, "discriminator-defaultMapping", field.Discriminator.DefaultMapping);
                    }

                    if (field.Discriminator.Mapping != null && field.Discriminator.Mapping.Any())
                    {
                        var mappingStr = string.Join(", ", field.Discriminator.Mapping.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
                        propNode = Docstrings.Emit.WithTag(propNode, "discriminator-mapping", mappingStr);
                    }
                }

                if (field.Xml != null)
                {
                    if (field.Xml.Name != null) propNode = Docstrings.Emit.WithTag(propNode, "xml-name", field.Xml.Name);
                    if (field.Xml.Namespace != null) propNode = Docstrings.Emit.WithTag(propNode, "xml-namespace", field.Xml.Namespace);
                    if (field.Xml.Prefix != null) propNode = Docstrings.Emit.WithTag(propNode, "xml-prefix", field.Xml.Prefix);
                    if (field.Xml.NodeType != null) propNode = Docstrings.Emit.WithTag(propNode, "xml-nodeType", field.Xml.NodeType);
                    if (field.Xml.Attribute != null) propNode = Docstrings.Emit.WithTag(propNode, "xml-attribute", field.Xml.Attribute.Value.ToString().ToLower());
                    if (field.Xml.Wrapped != null) propNode = Docstrings.Emit.WithTag(propNode, "xml-wrapped", field.Xml.Wrapped.Value.ToString().ToLower());
                }

                if (field.ExternalDocs?.Url != null)
                {
                    propNode = Docstrings.Emit.WithTag(propNode, "externalDocs", field.ExternalDocs.Url);
                }

                members.Add(propNode);
            }

            return classNode.AddMembers(members.ToArray());
        }
    }
}
