// <copyright file="parse.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using Cdd.OpenApi.Models.Arazzo;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cdd.OpenApi.Arazzo.Servers
{
    /// <summary>
    /// Parses C# Server/Backend E2E workflow test classes, stateful workflow mock classes, and ASP.NET Core workflow controller/endpoint classes back into an <see cref="ArazzoDocument"/>.
    /// </summary>
    public static class Parse
    {
        /// <summary>
        /// Parses one or more C# server workflow test, stateful mock, or server controller class declarations into an <see cref="ArazzoDocument"/>.
        /// </summary>
        /// <param name="classes">The collection of Roslyn <see cref="ClassDeclarationSyntax"/> nodes to inspect.</param>
        /// <returns>The reconstructed <see cref="ArazzoDocument"/>.</returns>
        public static ArazzoDocument ToArazzoDocument(IEnumerable<ClassDeclarationSyntax> classes)
        {
            if (classes == null)
            {
                throw new ArgumentNullException(nameof(classes));
            }

            var classList = classes.ToList();
            bool hasWorkflowSpec = classList.Any(cls =>
                cls.GetDescendantNodesSafe().OfType<VariableDeclaratorSyntax>().Any(v => v.Identifier.Text == "workflowSpec"));

            if (hasWorkflowSpec)
            {
                return Clients.Parse.ToArazzoDocument(classList);
            }

            var stepsByWorkflow = new Dictionary<string, List<ArazzoStep>>(StringComparer.Ordinal);
            foreach (var cls in classList)
            {
                foreach (var inv in cls.GetDescendantNodesSafe().OfType<InvocationExpressionSyntax>())
                {
                    if (inv.Expression is MemberAccessExpressionSyntax ma &&
                        ma.Name.Identifier.Text == "Add" &&
                        inv.ArgumentList.Arguments.Count == 1 &&
                        inv.ArgumentList.Arguments[0].Expression is LiteralExpressionSyntax lit)
                    {
                        var val = lit.Token.ValueText;
                        int colonIdx = val.IndexOf(':');
                        if (colonIdx > 0)
                        {
                            var wfId = val.Substring(0, colonIdx);
                            var stepId = val.Substring(colonIdx + 1);
                            if (!stepsByWorkflow.TryGetValue(wfId, out var steps))
                            {
                                steps = new List<ArazzoStep>();
                                stepsByWorkflow[wfId] = steps;
                            }

                            steps.Add(new ArazzoStep
                            {
                                StepId = stepId,
                                OperationId = stepId
                            });
                        }
                    }
                }
            }

            if (stepsByWorkflow.Count > 0)
            {
                var doc = new ArazzoDocument
                {
                    Arazzo = "1.1.0",
                    Info = new ArazzoInfo { Title = classList[0].Identifier.Text, Version = "1.0.0" },
                    SourceDescriptions = new List<ArazzoSourceDescription>
                    {
                        new ArazzoSourceDescription { Name = "default", Url = "./openapi.json", Type = "openapi" }
                    },
                    Workflows = new List<ArazzoWorkflow>()
                };

                foreach (var kvp in stepsByWorkflow)
                {
                    doc.Workflows.Add(new ArazzoWorkflow
                    {
                        WorkflowId = kvp.Key,
                        Steps = kvp.Value
                    });
                }

                return doc;
            }

            return Clients.Parse.ToArazzoDocument(classList);
        }
    }
}
