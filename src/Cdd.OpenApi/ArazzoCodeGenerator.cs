// <copyright file="ArazzoCodeGenerator.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using Cdd.OpenApi.Arazzo.Expressions;
using Cdd.OpenApi.Ir;
using Cdd.OpenApi.Models;
using Cdd.OpenApi.Models.Arazzo;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cdd.OpenApi
{
    /// <summary>
    /// Orchestrates C# code generation (Client SDK workflow orchestrators, DTOs, Client SDK CLI workflow runners, Server E2E workflow tests, and stateful mocks) from an <see cref="ArazzoDocument"/> or <see cref="IrService"/>.
    /// </summary>
    public static class ArazzoCodeGenerator
    {
        /// <summary>
        /// Generates C# source files from an <see cref="ArazzoDocument"/> and optional linked <see cref="OpenApiDocument"/> specifications.
        /// </summary>
        /// <param name="doc">The source <see cref="ArazzoDocument"/>.</param>
        /// <param name="baseNamespace">The root C# namespace for generated files.</param>
        /// <param name="type">The target generation mode (<c>Sdk</c>, <c>SdkCli</c>, <c>Server</c>, or <c>All</c>).</param>
        /// <param name="tests">Whether to also emit xUnit workflow test files.</param>
        /// <param name="sourceOpenApiDocs">Optional map of source description names to linked <see cref="OpenApiDocument"/> instances.</param>
        /// <returns>A list of <see cref="GeneratedCode"/> entries ready to be written to disk.</returns>
        public static List<GeneratedCode> Generate(
            ArazzoDocument doc,
            string baseNamespace = "Generated",
            GenerateType type = GenerateType.All,
            bool tests = false,
            IDictionary<string, OpenApiDocument>? sourceOpenApiDocs = null)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            return GenerateFromIr(IrService.FromArazzo(doc), baseNamespace, type, tests, sourceOpenApiDocs);
        }

        /// <summary>
        /// Generates C# source files from an <see cref="IrService"/> and optional linked <see cref="OpenApiDocument"/> specifications.
        /// </summary>
        /// <param name="ir">The source <see cref="IrService"/>.</param>
        /// <param name="baseNamespace">The root C# namespace for generated files.</param>
        /// <param name="type">The target generation mode (<c>Sdk</c>, <c>SdkCli</c>, <c>Server</c>, or <c>All</c>).</param>
        /// <param name="tests">Whether to also emit xUnit workflow test files.</param>
        /// <param name="sourceOpenApiDocs">Optional map of source description names to linked <see cref="OpenApiDocument"/> instances.</param>
        /// <returns>A list of <see cref="GeneratedCode"/> entries ready to be written to disk.</returns>
        public static List<GeneratedCode> GenerateFromIr(
            IrService ir,
            string baseNamespace = "Generated",
            GenerateType type = GenerateType.All,
            bool tests = false,
            IDictionary<string, OpenApiDocument>? sourceOpenApiDocs = null)
        {
            if (ir == null)
            {
                throw new ArgumentNullException(nameof(ir));
            }

            var results = new List<GeneratedCode>();
            if (sourceOpenApiDocs != null)
            {
                foreach (var kvp in sourceOpenApiDocs)
                {
                    var oasFiles = CodeGenerator.Generate(kvp.Value, baseNamespace, type, tests);
                    results.AddRange(oasFiles);
                }
            }

            var prefix = RuntimeExpression.ToPascalIdentifier(ir.Title);
            var orchestratorClassName = $"{prefix}Workflows";

            var modelNodes = Arazzo.Clients.Emit.ToWorkflowModelsFromIr(ir);
            if (modelNodes.Count > 0)
            {
                var wfModelsNs = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Workflows"))
                    .AddMembers(modelNodes.ToArray());
                results.Add(new GeneratedCode
                {
                    FileName = $"src/Workflows/{prefix}WorkflowModels.cs",
                    Code = WasmSafeRoslyn.FormatSafe(wfModelsNs)
                });
            }

            if (type == GenerateType.All || type == GenerateType.Sdk || type == GenerateType.SdkCli)
            {
                var orchestratorNode = Arazzo.Clients.Emit.ToWorkflowOrchestratorFromIr(orchestratorClassName, ir);
                var wfNs = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Workflows"))
                    .AddMembers(orchestratorNode);
                results.Add(new GeneratedCode
                {
                    FileName = $"src/Workflows/{orchestratorClassName}.cs",
                    Code = WasmSafeRoslyn.FormatSafe(wfNs)
                });
            }

            if (type == GenerateType.All || type == GenerateType.SdkCli)
            {
                var cliClassName = $"{prefix}WorkflowCli";
                var cliNode = Arazzo.Cli.Emit.ToWorkflowCliFromIr(cliClassName, orchestratorClassName, ir);
                var cliNs = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Cli"))
                    .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{baseNamespace}.Workflows")))
                    .AddMembers(cliNode);
                results.Add(new GeneratedCode
                {
                    FileName = $"src/Cli/{cliClassName}.cs",
                    Code = WasmSafeRoslyn.FormatSafe(cliNs)
                });
            }

            if (type == GenerateType.All || type == GenerateType.Server)
            {
                var mockClassName = $"{prefix}WorkflowMock";
                var mockNode = Arazzo.Servers.Emit.ToWorkflowMockFromIr(mockClassName, ir);
                var mockNs = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Mocks"))
                    .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{baseNamespace}.Workflows")))
                    .AddMembers(mockNode);
                results.Add(new GeneratedCode
                {
                    FileName = $"src/Mocks/{mockClassName}.cs",
                    Code = WasmSafeRoslyn.FormatSafe(mockNs)
                });
            }

            if (type == GenerateType.All || type == GenerateType.Server || tests)
            {
                var testsClassName = $"{prefix}WorkflowTests";
                var targetRunner = (type == GenerateType.Server) ? $"{prefix}WorkflowMock" : orchestratorClassName;
                var testNode = Arazzo.Servers.Emit.ToWorkflowTestsFromIr(testsClassName, targetRunner, ir);
                var testsNs = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Tests"))
                    .AddUsings(
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{baseNamespace}.Workflows")),
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{baseNamespace}.Mocks")))
                    .AddMembers(testNode);
                results.Add(new GeneratedCode
                {
                    FileName = $"src/Tests/{testsClassName}.cs",
                    Code = WasmSafeRoslyn.FormatSafe(testsNs)
                });
            }

            return results;
        }
    }
}
