// <copyright file="emit.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Mocks
{
    /// <summary>Emits C# mock API implementation classes from <see cref="Cdd.OpenApi.Ir.IrEndpoint"/> and <see cref="OpenApiPaths"/> definitions.</summary>
    public static class Emit
    {
        /// <summary>
        /// Generates a Roslyn <see cref="ClassDeclarationSyntax"/> representing a mock implementation of the IR endpoints.
        /// </summary>
        /// <param name="name">The name of the generated mock class.</param>
        /// <param name="endpoints">The IR endpoints to mock.</param>
        /// <param name="tests">Whether to implement the route interface for composable unit tests.</param>
        /// <returns>The generated <see cref="ClassDeclarationSyntax"/>.</returns>
        public static ClassDeclarationSyntax ToMock(string name, IReadOnlyList<Cdd.OpenApi.Ir.IrEndpoint> endpoints, bool tests = false)
        {
            return ToMock(name, Cdd.OpenApi.Common.OperationHelpers.ConvertEndpointsToPaths(endpoints), tests);
        }

        /// <summary>
        /// Generates a Roslyn <see cref="ClassDeclarationSyntax"/> representing a mock implementation of the OpenAPI operations.
        /// </summary>
        /// <param name="name">The name of the generated mock class.</param>
        /// <param name="paths">The OpenAPI paths to mock.</param>
        /// <param name="tests">Whether to implement the route interface for composable unit tests.</param>
        /// <returns>The generated <see cref="ClassDeclarationSyntax"/>.</returns>
        public static ClassDeclarationSyntax ToMock(string name, OpenApiPaths paths, bool tests = false)
        {
            var classDecl = SyntaxFactory.ClassDeclaration(name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            var members = new List<MemberDeclarationSyntax>();
            if (tests)
            {
                var interfaceName = $"I{name.Replace("Mock", "")}";
                classDecl = classDecl.AddBaseListTypes(
                    SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(interfaceName))
                );

                var interfaceNode = Routes.Emit.ToInterface(interfaceName, paths);
                foreach (var member in interfaceNode.Members.OfType<MethodDeclarationSyntax>())
                {
                    var methodDecl = member.WithAttributeLists(SyntaxFactory.List<AttributeListSyntax>());

                    var newParams = methodDecl.ParameterList.Parameters.Select(p => p.WithAttributeLists(SyntaxFactory.List<AttributeListSyntax>()));
                    methodDecl = methodDecl.WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(newParams)));

                    BlockSyntax body;
                    if (methodDecl.ReturnType.ToString() == "void")
                    {
                        body = SyntaxFactory.Block();
                    }
                    else
                    {
                        body = SyntaxFactory.Block(
                            SyntaxFactory.ReturnStatement(SyntaxFactory.DefaultExpression(methodDecl.ReturnType))
                        );
                    }

                    methodDecl = methodDecl.WithBody(body)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None))
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
                    members.Add(methodDecl);
                }
            }
            else
            {
                foreach (var pathKvp in paths)
                {
                    var route = pathKvp.Key;
                    var pathItem = pathKvp.Value;
                    foreach (var opKvp in Cdd.OpenApi.Common.OperationHelpers.GetOperationsByPascalVerb(pathItem))
                    {
                        if (opKvp.Value != null)
                        {
                            members.Add(CreateMockMethod(opKvp.Key, route, opKvp.Value));
                        }
                    }
                }
            }

            return classDecl.AddMembers(members.ToArray());
        }

        /// <summary>
        /// Creates a mock method declaration for a single OpenAPI operation.
        /// </summary>
        /// <param name="method">The PascalCase HTTP verb.</param>
        /// <param name="route">The route path template.</param>
        /// <param name="op">The OpenAPI operation metadata.</param>
        /// <returns>The generated <see cref="MethodDeclarationSyntax"/>.</returns>
        private static MethodDeclarationSyntax CreateMockMethod(string method, string route, OpenApiOperation op)
        {
            var methodName = op.OperationId ?? $"{method}{route.Replace("/", "").Replace("{", "").Replace("}", "")}";
            var returnType = SyntaxFactory.ParseTypeName("object");
            var methodDecl = SyntaxFactory.MethodDeclaration(returnType, methodName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            var body = SyntaxFactory.Block(
                SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression))
            );
            return methodDecl.WithBody(body);
        }
    }
}
