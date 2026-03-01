using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi
{
/// <summary>Auto-generated documentation for GeneratedCode.</summary>
    public class GeneratedCode
    {
/// <summary>Auto-generated documentation for FileName.</summary>
        public string FileName { get; set; } = string.Empty;
/// <summary>Auto-generated documentation for Code.</summary>
        public string Code { get; set; } = string.Empty;
    }

/// <summary>Auto-generated documentation for CodeGenerator.</summary>
    public static class CodeGenerator
    {
/// <summary>Auto-generated documentation for Generate.</summary>
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

                var clientNode = Cdd.OpenApi.Clients.Emit.ToClient("ApiClient", doc.Paths);
                
                var clientNsNode = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Client"))
                    .AddMembers(clientNode).NormalizeWhitespace();

                results.Add(new GeneratedCode 
                { 
                    FileName = "ApiClient.cs", 
                    Code = clientNsNode.ToFullString() 
                });

                var mockNode = Cdd.OpenApi.Mocks.Emit.ToMock("ApiMock", doc.Paths);
                
                var mockNsNode = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Mocks"))
                    .AddMembers(mockNode).NormalizeWhitespace();

                results.Add(new GeneratedCode 
                { 
                    FileName = "ApiMock.cs", 
                    Code = mockNsNode.ToFullString() 
                });

                var testsNode = Cdd.OpenApi.TestsModule.Emit.ToTests("ApiTests", doc.Paths);
                
                var testsNsNode = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Tests"))
                    .AddMembers(testsNode).NormalizeWhitespace();

                results.Add(new GeneratedCode 
                { 
                    FileName = "ApiTests.cs", 
                    Code = testsNsNode.ToFullString() 
                });
            }

            return results;
        }
    }
}