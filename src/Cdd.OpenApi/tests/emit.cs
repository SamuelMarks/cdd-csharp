// <copyright file="emit.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.TestsModule
{
    /// <summary>Emits xUnit API and client test classes from <see cref="Cdd.OpenApi.Ir.IrEndpoint"/> and <see cref="OpenApiPaths"/> definitions.</summary>
    public static class Emit
    {
        /// <summary>
        /// Generates a Roslyn <see cref="ClassDeclarationSyntax"/> containing server API test methods for the specified IR endpoints.
        /// </summary>
        /// <param name="name">The name of the generated test class.</param>
        /// <param name="endpoints">The IR endpoints to generate test methods for.</param>
        /// <param name="tests">Whether to emit composable xUnit <c>[Fact]</c> test harnesses.</param>
        /// <returns>The generated <see cref="ClassDeclarationSyntax"/>.</returns>
        public static ClassDeclarationSyntax ToTests(string name, IReadOnlyList<Cdd.OpenApi.Ir.IrEndpoint> endpoints, bool tests = false)
        {
            return ToTests(name, Cdd.OpenApi.Common.OperationHelpers.ConvertEndpointsToPaths(endpoints), tests);
        }

        /// <summary>
        /// Generates a Roslyn <see cref="ClassDeclarationSyntax"/> containing HTTP client test methods for the specified IR endpoints.
        /// </summary>
        /// <param name="name">The name of the generated client test class.</param>
        /// <param name="endpoints">The IR endpoints to generate client test methods for.</param>
        /// <param name="tests">Whether to emit composable xUnit <c>[Fact]</c> test harnesses.</param>
        /// <returns>The generated <see cref="ClassDeclarationSyntax"/>.</returns>
        public static ClassDeclarationSyntax ToClientTests(string name, IReadOnlyList<Cdd.OpenApi.Ir.IrEndpoint> endpoints, bool tests = false)
        {
            return ToClientTests(name, Cdd.OpenApi.Common.OperationHelpers.ConvertEndpointsToPaths(endpoints), tests);
        }

        /// <summary>
        /// Generates a Roslyn <see cref="ClassDeclarationSyntax"/> containing server API test methods for the specified paths.
        /// </summary>
        /// <param name="name">The name of the generated test class.</param>
        /// <param name="paths">The OpenAPI paths to generate test methods for.</param>
        /// <param name="tests">Whether to emit composable xUnit <c>[Fact]</c> test harnesses.</param>
        /// <returns>The generated <see cref="ClassDeclarationSyntax"/>.</returns>
        public static ClassDeclarationSyntax ToTests(string name, OpenApiPaths paths, bool tests = false)
        {
            var classDecl = SyntaxFactory.ClassDeclaration(name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
            classDecl = Cdd.OpenApi.Docstrings.Emit.WithSummary(classDecl, $"Unit tests for {name}.");

            var members = new List<MemberDeclarationSyntax>();
            if (tests)
            {
                var interfaceName = $"I{name.Replace("Tests", "")}";
                var field = SyntaxFactory.FieldDeclaration(
                    SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(interfaceName))
                    .AddVariables(SyntaxFactory.VariableDeclarator("_api"))
                ).AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword));

                var ctor = SyntaxFactory.ConstructorDeclaration(name)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("api")).WithType(SyntaxFactory.ParseTypeName(interfaceName)))
                    .WithBody(SyntaxFactory.Block(
                        SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName("_api"),
                            SyntaxFactory.IdentifierName("api")
                        ))
                    ));
                ctor = Cdd.OpenApi.Docstrings.Emit.WithSummary(ctor, $"Initializes a new instance of the {name} class.");

                members.Add(field);
                members.Add(ctor);
            }

            foreach (var pathKvp in paths)
            {
                var route = pathKvp.Key;
                var pathItem = pathKvp.Value;
                foreach (var opKvp in Cdd.OpenApi.Common.OperationHelpers.GetOperationsByPascalVerb(pathItem))
                {
                    if (opKvp.Value != null)
                    {
                        members.Add(CreateTestMethod(opKvp.Key, route, opKvp.Value, tests));
                    }
                }
            }

            return classDecl.AddMembers(members.ToArray());
        }

        /// <summary>
        /// Generates a Roslyn <see cref="ClassDeclarationSyntax"/> containing HTTP client test methods for the specified paths.
        /// </summary>
        /// <param name="name">The name of the generated client test class.</param>
        /// <param name="paths">The OpenAPI paths to generate client test methods for.</param>
        /// <param name="tests">Whether to emit composable xUnit <c>[Fact]</c> test harnesses.</param>
        /// <returns>The generated <see cref="ClassDeclarationSyntax"/>.</returns>
        public static ClassDeclarationSyntax ToClientTests(string name, OpenApiPaths paths, bool tests = false)
        {
            var classDecl = SyntaxFactory.ClassDeclaration(name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
            classDecl = Cdd.OpenApi.Docstrings.Emit.WithSummary(classDecl, $"Client unit tests for {name}.");

            var members = new List<MemberDeclarationSyntax>();
            if (tests)
            {
                var clientName = name.Replace("Tests", "");
                var field = SyntaxFactory.FieldDeclaration(
                    SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(clientName))
                    .AddVariables(SyntaxFactory.VariableDeclarator("_client"))
                ).AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword));

                var ctor = SyntaxFactory.ConstructorDeclaration(name)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("client")).WithType(SyntaxFactory.ParseTypeName(clientName)))
                    .WithBody(SyntaxFactory.Block(
                        SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName("_client"),
                            SyntaxFactory.IdentifierName("client")
                        ))
                    ));
                ctor = Cdd.OpenApi.Docstrings.Emit.WithSummary(ctor, $"Initializes a new instance of the {name} class.");

                members.Add(field);
                members.Add(ctor);
            }

            foreach (var pathKvp in paths)
            {
                var route = pathKvp.Key;
                var pathItem = pathKvp.Value;
                foreach (var opKvp in Cdd.OpenApi.Common.OperationHelpers.GetOperationsByPascalVerb(pathItem))
                {
                    if (opKvp.Value != null)
                    {
                        members.Add(CreateClientTestMethod(opKvp.Key, route, opKvp.Value, tests));
                    }
                }
            }

            return classDecl.AddMembers(members.ToArray());
        }

        /// <summary>
        /// Creates a server API test method declaration for a single OpenAPI operation.
        /// </summary>
        /// <param name="method">The PascalCase HTTP verb.</param>
        /// <param name="route">The route path template.</param>
        /// <param name="op">The OpenAPI operation metadata.</param>
        /// <param name="tests">Whether to attach <c>[Xunit.Fact]</c> and async task body.</param>
        /// <returns>The generated <see cref="MethodDeclarationSyntax"/>.</returns>
        private static MethodDeclarationSyntax CreateTestMethod(string method, string route, OpenApiOperation op, bool tests)
        {
            var methodName = op.OperationId ?? $"{method}{route.Replace("/", "").Replace("{", "").Replace("}", "")}";
            var returnType = SyntaxFactory.ParseTypeName(tests ? "System.Threading.Tasks.Task" : "void");
            var methodDecl = SyntaxFactory.MethodDeclaration(returnType, methodName + "Test")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
            if (tests)
            {
                methodDecl = methodDecl.AddAttributeLists(
                    SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Xunit.Fact"))))
                );
            }
            methodDecl = Cdd.OpenApi.Docstrings.Emit.WithSummary(methodDecl, $"Tests the {methodName} operation.");

            var body = SyntaxFactory.Block();

            if (tests)
            {
                body = SyntaxFactory.Block(
                    SyntaxFactory.ReturnStatement(SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName("System.Threading.Tasks.Task"),
                        SyntaxFactory.IdentifierName("CompletedTask")
                    ))
                );
            }

            return methodDecl.WithBody(body);
        }

        /// <summary>
        /// Creates a client test method declaration for a single OpenAPI operation.
        /// </summary>
        /// <param name="method">The PascalCase HTTP verb.</param>
        /// <param name="route">The route path template.</param>
        /// <param name="op">The OpenAPI operation metadata.</param>
        /// <param name="tests">Whether to attach <c>[Xunit.Fact]</c> and async task body.</param>
        /// <returns>The generated <see cref="MethodDeclarationSyntax"/>.</returns>
        private static MethodDeclarationSyntax CreateClientTestMethod(string method, string route, OpenApiOperation op, bool tests)
        {
            var methodName = op.OperationId ?? $"{method}{route.Replace("/", "").Replace("{", "").Replace("}", "")}";
            if (!methodName.EndsWith("Async")) methodName += "Async";
            var returnType = SyntaxFactory.ParseTypeName(tests ? "System.Threading.Tasks.Task" : "void");
            var methodDecl = SyntaxFactory.MethodDeclaration(returnType, methodName + "Test")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
            if (tests)
            {
                methodDecl = methodDecl.AddAttributeLists(
                    SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Xunit.Fact"))))
                );
            }
            methodDecl = Cdd.OpenApi.Docstrings.Emit.WithSummary(methodDecl, $"Tests the {methodName} client operation.");

            var body = SyntaxFactory.Block();

            if (tests)
            {
                body = SyntaxFactory.Block(
                    SyntaxFactory.ReturnStatement(SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName("System.Threading.Tasks.Task"),
                        SyntaxFactory.IdentifierName("CompletedTask")
                    ))
                );
            }

            return methodDecl.WithBody(body);
        }
    }
}
