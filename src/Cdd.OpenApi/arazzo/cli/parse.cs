// <copyright file="parse.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Cdd.OpenApi.Arazzo.Clients;
using Cdd.OpenApi.Common;
using Cdd.OpenApi.Models.Arazzo;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cdd.OpenApi.Arazzo.Cli
{
    /// <summary>
    /// Parses C# Client SDK CLI workflow runner classes back into an <see cref="ArazzoDocument"/>.
    /// </summary>
    public static class Parse
    {
        /// <summary>
        /// Parses one or more C# CLI workflow runner class declarations into an <see cref="ArazzoDocument"/>.
        /// </summary>
        /// <param name="classes">The collection of Roslyn <see cref="ClassDeclarationSyntax"/> nodes to inspect.</param>
        /// <returns>The reconstructed <see cref="ArazzoDocument"/>.</returns>
        public static ArazzoDocument ToArazzoDocument(IEnumerable<ClassDeclarationSyntax> classes)
        {
            if (classes == null)
            {
                throw new ArgumentNullException(nameof(classes));
            }

            var doc = new ArazzoDocument
            {
                Arazzo = "1.1.0",
                Info = new ArazzoInfo { Title = "Workflows", Version = "1.0.0" },
                SourceDescriptions = new List<ArazzoSourceDescription>(),
                Workflows = new List<ArazzoWorkflow>()
            };

            foreach (var cls in classes)
            {
                Clients.Parse.ParseClassMetadata(cls, doc);
                foreach (var method in cls.Members.OfType<MethodDeclarationSyntax>())
                {
                    if (method.Body == null) continue;
                    foreach (var stmt in method.Body.Statements)
                    {
                        if (stmt is SwitchStatementSyntax switchStmt)
                        {
                            ParseSwitchSections(switchStmt, doc);
                        }
                    }
                }
            }

            if (doc.SourceDescriptions.Count == 0)
            {
                doc.SourceDescriptions.Add(new ArazzoSourceDescription
                {
                    Name = "default",
                    Url = "./openapi.json",
                    Type = "openapi"
                });
            }

            return doc;
        }

        /// <summary>
        /// Extracts <see cref="ArazzoWorkflow"/> entries from CLI switch sections.
        /// </summary>
        /// <param name="switchStmt">The switch statement syntax node.</param>
        /// <param name="doc">The target <see cref="ArazzoDocument"/>.</param>
        private static void ParseSwitchSections(SwitchStatementSyntax switchStmt, ArazzoDocument doc)
        {
            var dq = StringEscaper.DoubleQuote;

            foreach (var section in switchStmt.Sections)
            {
                var caseLabel = section.Labels.OfType<CaseSwitchLabelSyntax>().FirstOrDefault();
                if (caseLabel == null) continue;

                var b64 = Clients.Parse.ExtractWorkflowSpecFromText(section.ToFullString());
                if (!string.IsNullOrWhiteSpace(b64))
                {
                    var bytes = Convert.FromBase64String(b64!);
                    var wfFromSpec = JsonSerializer.Deserialize(Encoding.UTF8.GetString(bytes), ArazzoJsonContext.Default.ArazzoWorkflow)!;
                    doc.Workflows.Add(wfFromSpec);
                    continue;
                }

                var wfId = caseLabel.Value.ToString().Trim().Replace(dq, string.Empty);
                var wf = new ArazzoWorkflow
                {
                    WorkflowId = wfId,
                    Steps = new List<ArazzoStep>
                    {
                        new ArazzoStep
                        {
                            StepId = "step1",
                            OperationId = wfId
                        }
                    }
                };

                doc.Workflows.Add(wf);
            }
        }
    }
}
