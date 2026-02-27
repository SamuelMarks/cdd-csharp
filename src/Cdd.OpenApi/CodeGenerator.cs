using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi
{
    public class GeneratedCode
    {
        public string FileName { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public static class CodeGenerator
    {
        public static List<GeneratedCode> Generate(OpenApiDocument doc, string baseNamespace = "Generated")
        {
            var results = new List<GeneratedCode>();

            if (doc.Components?.Schemas != null)
            {
                foreach (var schemaKvp in doc.Components.Schemas)
                {
                    var classNode = Cdd.OpenApi.Classes.Emit.ToClass(schemaKvp.Key, schemaKvp.Value);
                    
                    var nsNode = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Models"))
                        .AddMembers(classNode).NormalizeWhitespace();
                    
                    results.Add(new GeneratedCode 
                    { 
                        FileName = $"Models/{schemaKvp.Key}.cs", 
                        Code = nsNode.ToFullString() 
                    });
                }
            }

            if (doc.Paths != null && doc.Paths.Count > 0)
            {
                var interfaceNode = Cdd.OpenApi.Routes.Emit.ToInterface("IApi", doc.Paths);
                
                var nsNode = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Api"))
                    .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.AspNetCore.Mvc")))
                    .AddMembers(interfaceNode).NormalizeWhitespace();

                results.Add(new GeneratedCode 
                { 
                    FileName = "IApi.cs", 
                    Code = nsNode.ToFullString() 
                });
            }

            return results;
        }
    }
}