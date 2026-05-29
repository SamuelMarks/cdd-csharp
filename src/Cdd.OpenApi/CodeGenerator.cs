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


            if (!string.IsNullOrEmpty(doc.Self))
            {
                node = Cdd.OpenApi.Docstrings.Emit.WithTag(node, "self", doc.Self);
            }
            if (!string.IsNullOrEmpty(doc.JsonSchemaDialect))
            {
                node = Cdd.OpenApi.Docstrings.Emit.WithTag(node, "jsonSchemaDialect", doc.JsonSchemaDialect);
            }
            if (doc.Info != null)
            {
                if (!string.IsNullOrEmpty(doc.Info.TermsOfService))
                {
                    node = Cdd.OpenApi.Docstrings.Emit.WithTag(node, "termsOfService", doc.Info.TermsOfService);
                }
            }
            if (doc.Info != null)
            {
                if (doc.Info.License != null)
                {
                    if (!string.IsNullOrEmpty(doc.Info.License.Identifier))
                    {
                        node = Cdd.OpenApi.Docstrings.Emit.WithTag(node, "license-identifier", doc.Info.License.Identifier);
                    }
                }
            }

            return node;
        }

        /// <summary>Auto-generated documentation for Generate.</summary>
        public static List<GeneratedCode> Generate(OpenApiDocument doc, string baseNamespace = "Generated", GenerateType type = GenerateType.All, bool tests = false)
        {
            var results = new List<GeneratedCode>();
            if (doc == null) return results;

            if (doc.Components?.Schemas != null)
            {
                var modelsNsNode = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Models"))
                    .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.ComponentModel.DataAnnotations")));

                foreach (var schemaKvp in doc.Components.Schemas)
                {
                    var classNode = Cdd.OpenApi.Classes.Emit.ToClass(schemaKvp.Key, schemaKvp.Value);
                    var singleModelNs = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Models"))
                        .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Text.Json.Serialization")))
                        .AddMembers(classNode);
                    results.Add(new GeneratedCode
                    {
                        FileName = $"src/Models/{schemaKvp.Key}.cs",
                        Code = WasmSafeRoslyn.FormatSafe(singleModelNs)
                    });
                }

                if (type == GenerateType.All || type == GenerateType.Server)
                {
                    var dbContextNode = Cdd.OpenApi.Orm.Emit.ToDbContext("AppDbContext", doc.Components.Schemas);
                    var dbContextNsNode = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Models"))
                        .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.EntityFrameworkCore")))
                        .AddMembers(dbContextNode);
                    results.Add(new GeneratedCode { FileName = "src/Data/AppDbContext.cs", Code = WasmSafeRoslyn.FormatSafe(dbContextNsNode) });
                }
            }

            if (doc.Paths != null && doc.Paths.Count > 0)
            {
                var attributesCode = @"namespace " + baseNamespace + @"
{
    using System;
    /// <summary>Auto-generated documentation for ExplodeAttribute.</summary>
    [AttributeUsage(AttributeTargets.All)]
    public class ExplodeAttribute : Attribute {}
    /// <summary>Auto-generated documentation for AllowEmptyValueAttribute.</summary>
    [AttributeUsage(AttributeTargets.All)]
    public class AllowEmptyValueAttribute : Attribute {}
    /// <summary>Auto-generated documentation for AllowReservedAttribute.</summary>
    [AttributeUsage(AttributeTargets.All)]
    public class AllowReservedAttribute : Attribute {}
    /// <summary>Auto-generated documentation for StyleAttribute.</summary>
    [AttributeUsage(AttributeTargets.All)]
    public class StyleAttribute : Attribute {
        /// <summary>Auto-generated documentation for StyleAttribute.</summary>
        public StyleAttribute(string style) {}
    }
    /// <summary>Auto-generated documentation for ContentAttribute.</summary>
    [AttributeUsage(AttributeTargets.All)]
    public class ContentAttribute : Attribute {
        /// <summary>Auto-generated documentation for ContentAttribute.</summary>
        public ContentAttribute(string mediaType, string schemaType) {}
    }
    /// <summary>Auto-generated documentation for ExamplesAttribute.</summary>
    [AttributeUsage(AttributeTargets.All)]
    public class ExamplesAttribute : Attribute {
        /// <summary>Auto-generated documentation for ExamplesAttribute.</summary>
        public ExamplesAttribute(params string[] args) {}
    }
}
";
                results.Add(new GeneratedCode { FileName = "Attributes.cs", Code = attributesCode });

                if (type == GenerateType.All || type == GenerateType.Server)
                {
                    var interfaceNode = Cdd.OpenApi.Routes.Emit.ToInterface("IApi", doc.Paths);
                    interfaceNode = AddDocTags(interfaceNode, doc);
                    var nsNode = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Api"))
                        .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.AspNetCore.Mvc")))
                        .AddMembers(interfaceNode);
                    results.Add(new GeneratedCode { FileName = "src/Api/IApi.cs", Code = WasmSafeRoslyn.FormatSafe(nsNode) });
                }

                if (type == GenerateType.All || type == GenerateType.Sdk)
                {
                    var clientNode = Cdd.OpenApi.Clients.Emit.ToClient("ApiClient", doc.Paths);
                    clientNode = AddDocTags(clientNode, doc);
                    var clientNsNode = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Client"))
                        .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{baseNamespace}.Models")),
                                   SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Net.Http.Json")))
                        .AddMembers(clientNode);
                    results.Add(new GeneratedCode { FileName = "src/Client/Client.cs", Code = WasmSafeRoslyn.FormatSafe(clientNsNode) });
                }

                if (type == GenerateType.All || type == GenerateType.SdkCli)
                {
                    var cliNode = Cdd.OpenApi.CliModule.Emit.ToCli("ApiClientCli", doc.Paths);
                    cliNode = AddDocTags(cliNode, doc);
                    var cliNsNode = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Cli"))
                        .AddMembers(cliNode);
                    results.Add(new GeneratedCode { FileName = "src/Cli/ApiClientCli.cs", Code = WasmSafeRoslyn.FormatSafe(cliNsNode) });
                }

                if (type == GenerateType.All)
                {
                    var mockNode = Cdd.OpenApi.Mocks.Emit.ToMock("ApiMock", doc.Paths, tests);
                    mockNode = AddDocTags(mockNode, doc);
                    var mockNsNode = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Mocks"))
                        .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{baseNamespace}.Api")))
                        .AddMembers(mockNode);
                    results.Add(new GeneratedCode { FileName = "src/Mocks/ApiMock.cs", Code = WasmSafeRoslyn.FormatSafe(mockNsNode) });

                    var testsNode = Cdd.OpenApi.TestsModule.Emit.ToTests("ApiTests", doc.Paths, tests);
                    testsNode = AddDocTags(testsNode, doc);
                    var testsNsNode = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Tests"))
                        .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{baseNamespace}.Api")))
                        .AddMembers(testsNode);
                    results.Add(new GeneratedCode { FileName = "src/Tests/ApiTests.cs", Code = WasmSafeRoslyn.FormatSafe(testsNsNode) });
                }
            }

            return results;
        }
    }
}
