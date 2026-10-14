// <copyright file="ConfigTestsGenerator.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cdd.OpenApi.Orm
{
    /// <summary>Generates unit tests for ServerConfiguration.</summary>
    public static class ConfigTestsGenerator
    {
        /// <summary>Generates configuration tests.</summary>
        /// <param name="baseNamespace">The base C# namespace for the generated test classes.</param>
        /// <returns>A list of generated source code files.</returns>
        public static List<GeneratedCode> GenerateConfigTests(string baseNamespace)
        {
            var results = new List<GeneratedCode>();

            var testClass = SyntaxFactory.ClassDeclaration("ServerConfigurationTests")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithBaseList(SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                    SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("IDisposable")))))
                .AddMembers(
                    SyntaxFactory.ConstructorDeclaration("ServerConfigurationTests")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .WithBody(SyntaxFactory.Block(
                            SyntaxFactory.ParseStatement(@"Environment.SetEnvironmentVariable(""DATABASE_URL"", null);"),
                            SyntaxFactory.ParseStatement(@"Environment.SetEnvironmentVariable(""EPHEMERAL_DB"", null);")
                        )),
                    SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), "Dispose")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .WithBody(SyntaxFactory.Block(
                            SyntaxFactory.ParseStatement(@"Environment.SetEnvironmentVariable(""DATABASE_URL"", null);"),
                            SyntaxFactory.ParseStatement(@"Environment.SetEnvironmentVariable(""EPHEMERAL_DB"", null);")
                        )),
                    SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), "FromEnvironment_ParsesCorrectly")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Fact")))))
                        .WithBody(SyntaxFactory.Block(
                            SyntaxFactory.ParseStatement(@"Environment.SetEnvironmentVariable(""DATABASE_URL"", ""Host=localhost"");"),
                            SyntaxFactory.ParseStatement(@"Environment.SetEnvironmentVariable(""EPHEMERAL_DB"", ""true"");"),
                            SyntaxFactory.ParseStatement(@"var config = ServerConfiguration.FromEnvironment();"),
                            SyntaxFactory.ParseStatement(@"Assert.Equal(""Host=localhost"", config.DatabaseUrl);"),
                            SyntaxFactory.ParseStatement(@"Assert.True(config.UseEphemeral);")
                        )),
                    SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), "FromEnvironment_Defaults")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Fact")))))
                        .WithBody(SyntaxFactory.Block(
                            SyntaxFactory.ParseStatement(@"var config = ServerConfiguration.FromEnvironment();"),
                            SyntaxFactory.ParseStatement(@"Assert.Null(config.DatabaseUrl);"),
                            SyntaxFactory.ParseStatement(@"Assert.False(config.UseEphemeral);")
                        ))
                );

            var testNs = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Tests"))
                .AddUsings(
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Xunit")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{baseNamespace}.Configuration")))
                .AddMembers(testClass);

            results.Add(new GeneratedCode
            {
                FileName = "src/Tests/ServerConfigurationTests.cs",
                Code = WasmSafeRoslyn.FormatSafe(testNs)
            });

            return results;
        }
    }
}
