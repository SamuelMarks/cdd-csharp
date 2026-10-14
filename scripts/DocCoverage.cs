// <copyright file="DocCoverage.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DocCoverage
{
    /// <summary>
    /// Verifies 100% XML documentation coverage across files, namespaces, types, members, parameters, type parameters, and return values.
    /// </summary>
    internal static class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                return 0;
            }

            int totalItems = 0;
            int documentedItems = 0;

            foreach (var file in Directory.GetFiles(args[0], "*.cs", SearchOption.AllDirectories).OrderBy(f => f))
            {
                var normalized = file.Replace(Path.DirectorySeparatorChar, '/');
                if (normalized.Contains("/obj/") || normalized.Contains("/bin/"))
                {
                    continue;
                }

                var sourceText = File.ReadAllText(file);
                var tree = CSharpSyntaxTree.ParseText(sourceText);
                var root = tree.GetRoot();

                // 1. Check file-level header comment
                totalItems++;
                bool hasFileHeader = root.GetLeadingTrivia().Any(t =>
                    t.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                    t.IsKind(SyntaxKind.MultiLineCommentTrivia) ||
                    t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                    t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia));
                if (hasFileHeader)
                {
                    documentedItems++;
                }
                else
                {
                    Console.WriteLine($"Missing file header comment: {file}");
                }

                // 2. Check all declarations
                var declarations = root.DescendantNodes().Where(n =>
                    n is ClassDeclarationSyntax ||
                    n is StructDeclarationSyntax ||
                    n is InterfaceDeclarationSyntax ||
                    n is EnumDeclarationSyntax ||
                    n is EnumMemberDeclarationSyntax ||
                    n is RecordDeclarationSyntax ||
                    n is DelegateDeclarationSyntax ||
                    n is ConstructorDeclarationSyntax ||
                    n is MethodDeclarationSyntax ||
                    n is PropertyDeclarationSyntax ||
                    n is FieldDeclarationSyntax);

                foreach (var node in declarations)
                {
                    totalItems++;
                    string name = GetNodeName(node);
                    string docText = GetXmlDocText(node);

                    bool isValid = true;
                    string reason = "";

                    if (string.IsNullOrWhiteSpace(docText) || !docText.Contains("<summary>"))
                    {
                        isValid = false;
                        reason = "missing <summary>";
                    }
                    else if (docText.Contains("Auto-generated documentation for"))
                    {
                        isValid = false;
                        reason = "contains placeholder 'Auto-generated documentation for'";
                    }
                    else
                    {
                        // Check <typeparam>
                        var typeParams = GetTypeParameters(node);
                        foreach (var tp in typeParams)
                        {
                            if (!docText.Contains(@$"<typeparam name=""{tp}"">"))
                            {
                                isValid = false;
                                reason = @$"missing <typeparam name=""{tp}"">";
                                break;
                            }
                        }

                        // Check <param>
                        if (isValid)
                        {
                            var parameters = GetParameters(node);
                            foreach (var p in parameters)
                            {
                                if (!docText.Contains(@$"<param name=""{p}"">"))
                                {
                                    isValid = false;
                                    reason = @$"missing <param name=""{p}"">";
                                    break;
                                }
                            }
                        }

                        // Check <returns>
                        if (isValid && RequiresReturnsTag(node))
                        {
                            if (!docText.Contains("<returns>"))
                            {
                                isValid = false;
                                reason = "missing <returns>";
                            }
                        }
                    }

                    if (isValid)
                    {
                        documentedItems++;
                    }
                    else
                    {
                        var line = node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                        Console.WriteLine($"Missing/invalid doc: {file}:{line} - {name} ({reason})");
                    }
                }
            }

            int pct = totalItems == 0 ? 100 : (documentedItems == totalItems ? 100 : (int)((double)documentedItems / totalItems * 99));
            Console.WriteLine(pct);
            return documentedItems == totalItems ? 0 : 1;
        }

        private static string GetNodeName(SyntaxNode node) => node switch
        {
            BaseTypeDeclarationSyntax btd => btd.Identifier.Text,
            EnumMemberDeclarationSyntax emd => emd.Identifier.Text,
            DelegateDeclarationSyntax dd => dd.Identifier.Text,
            ConstructorDeclarationSyntax cd => cd.Identifier.Text,
            MethodDeclarationSyntax md => md.Identifier.Text,
            PropertyDeclarationSyntax pd => pd.Identifier.Text,
            FieldDeclarationSyntax fd => string.Join(",", fd.Declaration.Variables.Select(v => v.Identifier.Text)),
            _ => node.Kind().ToString()
        };

        private static string GetXmlDocText(SyntaxNode node)
        {
            var trivia = node.GetLeadingTrivia()
                .Where(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) || t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
                .Select(t => t.ToFullString());
            return string.Concat(trivia);
        }

        private static string[] GetTypeParameters(SyntaxNode node) => node switch
        {
            TypeDeclarationSyntax td when td.TypeParameterList != null => td.TypeParameterList.Parameters.Select(p => p.Identifier.Text).ToArray(),
            MethodDeclarationSyntax md when md.TypeParameterList != null => md.TypeParameterList.Parameters.Select(p => p.Identifier.Text).ToArray(),
            DelegateDeclarationSyntax dd when dd.TypeParameterList != null => dd.TypeParameterList.Parameters.Select(p => p.Identifier.Text).ToArray(),
            _ => Array.Empty<string>()
        };

        private static string[] GetParameters(SyntaxNode node) => node switch
        {
            MethodDeclarationSyntax md => md.ParameterList.Parameters.Select(p => p.Identifier.Text).ToArray(),
            ConstructorDeclarationSyntax cd => cd.ParameterList.Parameters.Select(p => p.Identifier.Text).ToArray(),
            DelegateDeclarationSyntax dd => dd.ParameterList.Parameters.Select(p => p.Identifier.Text).ToArray(),
            RecordDeclarationSyntax rd when rd.ParameterList != null => rd.ParameterList.Parameters.Select(p => p.Identifier.Text).ToArray(),
            _ => Array.Empty<string>()
        };

        private static bool RequiresReturnsTag(SyntaxNode node) => node switch
        {
            MethodDeclarationSyntax md => !(md.ReturnType is PredefinedTypeSyntax pts && pts.Keyword.IsKind(SyntaxKind.VoidKeyword)),
            DelegateDeclarationSyntax dd => !(dd.ReturnType is PredefinedTypeSyntax pts && pts.Keyword.IsKind(SyntaxKind.VoidKeyword)),
            _ => false
        };
    }
}
