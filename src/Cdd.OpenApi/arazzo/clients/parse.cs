// <copyright file="parse.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Cdd.OpenApi.Common;
using Cdd.OpenApi.Docstrings;
using Cdd.OpenApi.Models;
using Cdd.OpenApi.Models.Arazzo;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cdd.OpenApi.Arazzo.Clients
{
    /// <summary>
    /// Parses C# Client SDK workflow orchestrator classes and DTOs back into an <see cref="ArazzoDocument"/>.
    /// </summary>
    public static class Parse
    {
        /// <summary>
        /// Parses one or more C# workflow orchestrator and DTO class declarations into an <see cref="ArazzoDocument"/>.
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
            var doc = new ArazzoDocument
            {
                Arazzo = "1.1.0",
                Info = new ArazzoInfo { Title = "Workflows", Version = "1.0.0" },
                SourceDescriptions = new List<ArazzoSourceDescription>(),
                Workflows = new List<ArazzoWorkflow>()
            };

            var inputModels = new Dictionary<string, ClassDeclarationSyntax>(StringComparer.OrdinalIgnoreCase);
            foreach (var cls in classList)
            {
                var name = cls.Identifier.Text;
                if (name.EndsWith("Input", StringComparison.Ordinal))
                {
                    inputModels[name] = cls;
                }
            }

            foreach (var cls in classList)
            {
                var name = cls.Identifier.Text;
                if (name.EndsWith("Input", StringComparison.Ordinal) || name.EndsWith("Output", StringComparison.Ordinal))
                {
                    continue;
                }

                ParseClassMetadata(cls, doc);
                ParseWorkflowMethods(cls, doc, inputModels);
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
        /// Extracts root Arazzo metadata, constants, source descriptions, components, and extensions from a class declaration.
        /// </summary>
        /// <param name="cls">The orchestrator class declaration.</param>
        /// <param name="doc">The target <see cref="ArazzoDocument"/>.</param>
        internal static void ParseClassMetadata(ClassDeclarationSyntax cls, ArazzoDocument doc)
        {
            foreach (var field in cls.Members.OfType<FieldDeclarationSyntax>())
            {
                foreach (var variable in field.Declaration.Variables)
                {
                    if (variable.Initializer == null) continue;
                    var valLit = variable.Initializer.Value as LiteralExpressionSyntax;
                    var valText = valLit!.Token.ValueText;
                    if (variable.Identifier.Text == "DocumentSpec")
                    {
                        var metaBytes = Convert.FromBase64String(valText);
                        var metaDoc = JsonSerializer.Deserialize(Encoding.UTF8.GetString(metaBytes), ArazzoJsonContext.Default.ArazzoDocument)!;
                        doc.Arazzo = metaDoc.Arazzo;
                        doc.Self = metaDoc.Self;
                        doc.Info = metaDoc.Info;
                        doc.SourceDescriptions = metaDoc.SourceDescriptions;
                        doc.Components = metaDoc.Components;
                        doc.Extensions = metaDoc.Extensions;
                        return;
                    }
                    else if (variable.Identifier.Text == "ArazzoVersion")
                    {
                        doc.Arazzo = valText;
                    }
                    else if (variable.Identifier.Text == "SelfUri")
                    {
                        doc.Self = valText;
                    }
                }
            }

            doc.Info.Title = cls.Identifier.Text;
            var summaryTag = Docstrings.Parse.GetSummary(cls);
            if (!string.IsNullOrWhiteSpace(summaryTag))
            {
                doc.Info.Summary = summaryTag;
            }
        }

        /// <summary>
        /// Extracts <see cref="ArazzoWorkflow"/> instances from public methods on a C# class.
        /// </summary>
        /// <param name="cls">The C# class declaration.</param>
        /// <param name="doc">The target <see cref="ArazzoDocument"/>.</param>
        /// <param name="inputModels">Map of discovered input DTO classes.</param>
        private static void ParseWorkflowMethods(ClassDeclarationSyntax cls, ArazzoDocument doc, IDictionary<string, ClassDeclarationSyntax> inputModels)
        {
            foreach (var method in cls.Members.OfType<MethodDeclarationSyntax>())
            {
                var specB64 = ExtractWorkflowSpecFromText(method.Body!.ToFullString());
                if (!string.IsNullOrWhiteSpace(specB64))
                {
                    var specBytes = Convert.FromBase64String(specB64!);
                    var wfFromSpec = JsonSerializer.Deserialize(Encoding.UTF8.GetString(specBytes), ArazzoJsonContext.Default.ArazzoWorkflow)!;
                    if (!doc.Workflows.Any(w => w.WorkflowId == wfFromSpec.WorkflowId))
                    {
                        doc.Workflows.Add(wfFromSpec);
                    }

                    continue;
                }

                var wfId = TrimAsyncSuffix(method.Identifier.Text);
                var wf = new ArazzoWorkflow
                {
                    WorkflowId = wfId,
                    Summary = Docstrings.Parse.GetSummary(method),
                    Steps = new List<ArazzoStep>()
                };

                var firstParam = method.ParameterList.Parameters.FirstOrDefault();
                if (firstParam != null && inputModels.TryGetValue(firstParam.Type!.ToString().TrimEnd('?'), out var inputCls))
                {
                    wf.Inputs = ExtractInputSchemaFromDto(inputCls);
                }

                int stepIndex = 1;
                foreach (var stmt in method.Body!.Statements)
                {
                    var text = stmt.ToString();
                    if (text.Contains("await ", StringComparison.Ordinal))
                    {
                        var opName = ExtractOperationNameFromStatement(text);
                        wf.Steps.Add(new ArazzoStep
                        {
                            StepId = $"step{stepIndex++}",
                            OperationId = opName
                        });
                    }
                }

                if (wf.Steps.Count == 0)
                {
                    wf.Steps.Add(new ArazzoStep
                    {
                        StepId = "step1",
                        OperationId = wfId
                    });
                }

                doc.Workflows.Add(wf);
            }
        }

        /// <summary>
        /// Reconstructs an <see cref="OpenApiSchema"/> from a workflow Input DTO class declaration.
        /// </summary>
        /// <param name="inputCls">The DTO class declaration.</param>
        /// <returns>The reconstructed <see cref="OpenApiSchema"/> or <c>null</c>.</returns>
        private static OpenApiSchema? ExtractInputSchemaFromDto(ClassDeclarationSyntax inputCls)
        {
            var props = new Dictionary<string, OpenApiSchema>();
            foreach (var prop in inputCls.Members.OfType<PropertyDeclarationSyntax>())
            {
                if (prop.Identifier.Text == "Values") continue;
                var propName = char.ToLowerInvariant(prop.Identifier.Text[0]) + prop.Identifier.Text.Substring(1);
                var rawType = prop.Type.ToString().TrimEnd('?');
                props[propName] = new OpenApiSchema
                {
                    Type = TypeMapper.MapCSharpToOpenApi(rawType, "string"),
                    Description = Docstrings.Parse.GetSummary(prop)
                };
            }

            return new OpenApiSchema
            {
                Type = "object",
                Properties = props
            };
        }

        /// <summary>
        /// Removes trailing <c>Async</c> or <c>WorkflowTest</c> suffixes from a C# method name and converts to camelCase.
        /// </summary>
        /// <param name="methodName">The C# method identifier.</param>
        /// <returns>The normalized workflowId.</returns>
        internal static string TrimAsyncSuffix(string methodName)
        {
            var s = methodName;
            if (s.EndsWith("WorkflowTest", StringComparison.Ordinal))
            {
                s = s.Substring(0, s.Length - "WorkflowTest".Length);
            }
            else if (s.EndsWith("Async", StringComparison.Ordinal))
            {
                s = s.Substring(0, s.Length - "Async".Length);
            }

            return char.ToLowerInvariant(s[0]) + s.Substring(1);
        }

        /// <summary>
        /// Extracts the base64-encoded <c>workflowSpec</c> string literal from a C# statement or block text.
        /// </summary>
        /// <param name="text">The C# source text containing a <c>workflowSpec</c> assignment.</param>
        /// <returns>The base64 string if found; otherwise <c>null</c>.</returns>
        internal static string? ExtractWorkflowSpecFromText(string text)
        {
            int varIdx = text.IndexOf("workflowSpec", StringComparison.Ordinal);
            if (varIdx < 0) return null;
            int eqIdx = text.IndexOf('=', varIdx + "workflowSpec".Length);
            int firstQuote = text.IndexOf((char)34, eqIdx + 1);
            int secondQuote = text.IndexOf((char)34, firstQuote + 1);
            return text.Substring(firstQuote + 1, secondQuote - firstQuote - 1).Trim();
        }

        /// <summary>
        /// Extracts an operation name from a C# <c>await</c> invocation statement string.
        /// </summary>
        /// <param name="statementText">The raw C# statement text.</param>
        /// <returns>The extracted camelCase operation identifier.</returns>
        internal static string ExtractOperationNameFromStatement(string statementText)
        {
            int awaitIdx = statementText.IndexOf("await ", StringComparison.Ordinal);
            var after = statementText.Substring(awaitIdx + 6).Trim();
            int parenIdx = after.IndexOf('(');
            var callPart = after.Substring(0, parenIdx);
            int dotIdx = callPart.LastIndexOf('.');
            var methodIdent = callPart.Substring(dotIdx + 1).Trim();
            return TrimAsyncSuffix(methodIdent);
        }
    }
}
