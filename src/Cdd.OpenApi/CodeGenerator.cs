using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi
{
    /// <summary>Auto-generated documentation for GenerateType.</summary>
    public enum GenerateType
    {
        /// <summary>Auto-generated documentation for Sdk.</summary>
        Sdk,
        /// <summary>Auto-generated documentation for SdkCli.</summary>
        SdkCli,
        /// <summary>Auto-generated documentation for Server.</summary>
        Server,
        /// <summary>Auto-generated documentation for All.</summary>
        All
    }

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
        private static TNode AddDocTags<TNode>(TNode node, OpenApiDocument doc) where TNode : SyntaxNode
        {
            if (doc == null) return node;
            
            if (!string.IsNullOrEmpty(doc.Self)) node = Cdd.OpenApi.Docstrings.Emit.WithTag(node, "self", doc.Self);
            if (!string.IsNullOrEmpty(doc.JsonSchemaDialect)) node = Cdd.OpenApi.Docstrings.Emit.WithTag(node, "jsonSchemaDialect", doc.JsonSchemaDialect);
            if (!string.IsNullOrEmpty(doc.Info?.TermsOfService)) node = Cdd.OpenApi.Docstrings.Emit.WithTag(node, "termsOfService", doc.Info.TermsOfService);
            if (!string.IsNullOrEmpty(doc.Info?.License?.Identifier)) node = Cdd.OpenApi.Docstrings.Emit.WithTag(node, "license-identifier", doc.Info.License.Identifier);
            
            return node;
        }

/// <summary>Auto-generated documentation for Generate.</summary>
        public static List<GeneratedCode> Generate(OpenApiDocument doc, string baseNamespace = "Generated", GenerateType type = GenerateType.All)
        {
            var results = new List<GeneratedCode>();

            if (doc.Components?.Schemas != null)
            {
                foreach (var schemaKvp in doc.Components.Schemas)
                {
                    var classNode = Cdd.OpenApi.Classes.Emit.ToClass(schemaKvp.Key, schemaKvp.Value);
                    
                    var nsNode = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Models"))
                        .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.ComponentModel.DataAnnotations")))
                        .AddMembers(classNode).NormalizeWhitespace();
                    
                    results.Add(new GeneratedCode 
                    { 
                        FileName = $"Models/{schemaKvp.Key}.cs", 
                        Code = nsNode.ToFullString() 
                    });
                }

                if (type == GenerateType.All || type == GenerateType.Server)
                {
                    var dbContextNode = Cdd.OpenApi.Orm.Emit.ToDbContext("AppDbContext", doc.Components.Schemas);
                    var dbContextNsNode = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Models"))
                        .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.EntityFrameworkCore")))
                        .AddMembers(dbContextNode).NormalizeWhitespace();
                    results.Add(new GeneratedCode { FileName = "Models/AppDbContext.cs", Code = dbContextNsNode.ToFullString() });
                }
            }

            if (doc.Paths != null && doc.Paths.Count > 0)
            {
                if (type == GenerateType.All || type == GenerateType.Server)
                {
                    var interfaceNode = Cdd.OpenApi.Routes.Emit.ToInterface("IApi", doc.Paths);
                    interfaceNode = AddDocTags(interfaceNode, doc);
                    var nsNode = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Api"))
                        .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.AspNetCore.Mvc")))
                        .AddMembers(interfaceNode).NormalizeWhitespace();
                    results.Add(new GeneratedCode { FileName = "IApi.cs", Code = nsNode.ToFullString() });
                }

                if (type == GenerateType.All || type == GenerateType.Sdk)
                {
                    var clientNode = Cdd.OpenApi.Clients.Emit.ToClient("ApiClient", doc.Paths);
                    clientNode = AddDocTags(clientNode, doc);
                    var clientNsNode = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Client"))
                        .AddMembers(clientNode).NormalizeWhitespace();
                    results.Add(new GeneratedCode { FileName = "ApiClient.cs", Code = clientNsNode.ToFullString() });
                }

                if (type == GenerateType.All || type == GenerateType.SdkCli)
                {
                    var cliNode = Cdd.OpenApi.CliModule.Emit.ToCli("ApiClientCli", doc.Paths);
                    cliNode = AddDocTags(cliNode, doc);
                    var cliNsNode = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Cli"))
                        .AddMembers(cliNode).NormalizeWhitespace();
                    results.Add(new GeneratedCode { FileName = "ApiClientCli.cs", Code = cliNsNode.ToFullString() });
                }

                if (type == GenerateType.All)
                {
                    var mockNode = Cdd.OpenApi.Mocks.Emit.ToMock("ApiMock", doc.Paths);
                    mockNode = AddDocTags(mockNode, doc);
                    var mockNsNode = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Mocks"))
                        .AddMembers(mockNode).NormalizeWhitespace();
                    results.Add(new GeneratedCode { FileName = "ApiMock.cs", Code = mockNsNode.ToFullString() });

                    var testsNode = Cdd.OpenApi.TestsModule.Emit.ToTests("ApiTests", doc.Paths);
                    testsNode = AddDocTags(testsNode, doc);
                    var testsNsNode = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Tests"))
                        .AddMembers(testsNode).NormalizeWhitespace();
                    results.Add(new GeneratedCode { FileName = "ApiTests.cs", Code = testsNsNode.ToFullString() });
                }
            }

            return results;
        }
    }
}
