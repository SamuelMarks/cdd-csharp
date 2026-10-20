// <copyright file="ArazzoSpecGenerator.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using Cdd.OpenApi.Ir;
using Cdd.OpenApi.Models.Arazzo;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cdd.OpenApi
{
    /// <summary>
    /// Synthesizes an <see cref="ArazzoDocument"/> or <see cref="IrService"/> by reverse-parsing C# source files (SDK workflow orchestrators, CLI workflow runners, or Server E2E workflow tests/mocks).
    /// </summary>
    public static class ArazzoSpecGenerator
    {
        /// <summary>
        /// Parses a single C# source code string into an <see cref="ArazzoDocument"/>.
        /// </summary>
        /// <param name="csharpSourceCode">The C# source code string.</param>
        /// <returns>The reconstructed <see cref="ArazzoDocument"/>.</returns>
        public static ArazzoDocument Generate(string csharpSourceCode)
        {
            if (csharpSourceCode == null)
            {
                throw new ArgumentNullException(nameof(csharpSourceCode));
            }

            return Generate(new[] { csharpSourceCode });
        }

        /// <summary>
        /// Parses multiple C# source code strings into a unified <see cref="ArazzoDocument"/>.
        /// </summary>
        /// <param name="csharpSourceCodes">The collection of C# source code strings.</param>
        /// <returns>The reconstructed <see cref="ArazzoDocument"/>.</returns>
        public static ArazzoDocument Generate(IEnumerable<string> csharpSourceCodes)
        {
            return GenerateIr(csharpSourceCodes).ToArazzo();
        }

        /// <summary>
        /// Parses multiple C# source code strings into a unified <see cref="IrService"/>.
        /// </summary>
        /// <param name="csharpSourceCodes">The collection of C# source code strings.</param>
        /// <returns>The reconstructed <see cref="IrService"/>.</returns>
        public static IrService GenerateIr(IEnumerable<string> csharpSourceCodes)
        {
            if (csharpSourceCodes == null)
            {
                throw new ArgumentNullException(nameof(csharpSourceCodes));
            }

            var allClasses = new List<ClassDeclarationSyntax>();
            foreach (var code in csharpSourceCodes)
            {
                if (string.IsNullOrWhiteSpace(code)) continue;
                var tree = CSharpSyntaxTree.ParseText(code);
                var root = tree.GetRoot();
                foreach (var node in root.ChildNodes())
                {
                    if (node is ClassDeclarationSyntax topCls)
                    {
                        allClasses.Add(topCls);
                    }
                    else if (node is BaseNamespaceDeclarationSyntax ns)
                    {
                        allClasses.AddRange(ns.Members.OfType<ClassDeclarationSyntax>());
                    }
                }
            }

            var cliClasses = allClasses.Where(c => c.Identifier.Text.EndsWith("Cli", StringComparison.Ordinal)).ToList();
            var nonCliOrchestratorClasses = allClasses.Where(c =>
                !c.Identifier.Text.EndsWith("Cli", StringComparison.Ordinal) &&
                !c.Identifier.Text.EndsWith("Input", StringComparison.Ordinal) &&
                !c.Identifier.Text.EndsWith("Output", StringComparison.Ordinal)).ToList();

            if (nonCliOrchestratorClasses.Count > 0)
            {
                return Arazzo.Clients.Parse.ToIrService(allClasses.Where(c => !c.Identifier.Text.EndsWith("Cli", StringComparison.Ordinal)));
            }

            if (cliClasses.Count > 0)
            {
                return Arazzo.Cli.Parse.ToIrService(cliClasses);
            }

            var fallbackIr = Arazzo.Clients.Parse.ToIrService(allClasses);
            fallbackIr.Workflows.Add(new IrWorkflow
            {
                WorkflowId = "defaultWorkflow",
                Summary = "Synthesized default workflow",
                Steps = new List<IrWorkflowStep>
                {
                    new IrWorkflowStep
                    {
                        StepId = "step1",
                        OperationId = "defaultOperation"
                    }
                }
            });

            return fallbackIr;
        }
    }
}
