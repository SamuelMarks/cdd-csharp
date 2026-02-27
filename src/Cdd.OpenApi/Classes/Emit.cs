using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Classes
{
    public static class Emit
    {
        public static ClassDeclarationSyntax ToClass(string className, OpenApiSchema schema)
        {
            var classNode = SyntaxFactory.ClassDeclaration(className)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            if (!string.IsNullOrWhiteSpace(schema.Description))
            {
                classNode = Docstrings.Emit.WithSummary(classNode, schema.Description);
            }

            if (schema.Properties != null)
            {
                foreach (var prop in schema.Properties)
                {
                    var propName = prop.Key;
                    var propSchema = prop.Value;
                    
                    var isRequired = schema.Required != null && schema.Required.Contains(propName);
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

                    if (!string.IsNullOrWhiteSpace(propSchema.Description))
                    {
                        propNode = Docstrings.Emit.WithSummary(propNode, propSchema.Description);
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
