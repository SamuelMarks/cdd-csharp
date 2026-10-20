// <copyright file="parse.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Parses C# Client SDK workflow orchestrator classes and DTOs back into an <see cref="Cdd.OpenApi.Ir.IrService"/> or <see cref="ArazzoDocument"/>.
    /// </summary>
    public static class Parse
    {
        /// <summary>
        /// Parses one or more C# workflow orchestrator and DTO class declarations into an <see cref="Cdd.OpenApi.Ir.IrService"/>.
        /// </summary>
        /// <param name="classes">The collection of Roslyn <see cref="ClassDeclarationSyntax"/> nodes to inspect.</param>
        /// <returns>The reconstructed <see cref="Cdd.OpenApi.Ir.IrService"/>.</returns>
        public static Cdd.OpenApi.Ir.IrService ToIrService(IEnumerable<ClassDeclarationSyntax> classes)
        {
            return Cdd.OpenApi.Ir.IrService.FromArazzo(ToArazzoDocument(classes));
        }

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
                    if (variable.Initializer?.Value is LiteralExpressionSyntax valLit)
                    {
                        var valText = valLit.Token.ValueText;
                        if (variable.Identifier.Text == "ArazzoVersion")
                        {
                            doc.Arazzo = valText;
                        }
                        else if (variable.Identifier.Text == "SelfUri")
                        {
                            doc.Self = valText;
                        }
                    }
                }
            }

            var arazzoVersionTag = Docstrings.Parse.GetTag(cls, "arazzo-version");
            if (!string.IsNullOrWhiteSpace(arazzoVersionTag))
            {
                doc.Arazzo = arazzoVersionTag!;
            }

            var selfTag = Docstrings.Parse.GetTag(cls, "arazzo-self");
            if (!string.IsNullOrWhiteSpace(selfTag))
            {
                doc.Self = selfTag!;
            }

            var titleTag = Docstrings.Parse.GetTag(cls, "arazzo-title");
            doc.Info.Title = !string.IsNullOrWhiteSpace(titleTag) ? titleTag! : cls.Identifier.Text;

            var infoVerTag = Docstrings.Parse.GetTag(cls, "arazzo-info-version");
            if (!string.IsNullOrWhiteSpace(infoVerTag))
            {
                doc.Info.Version = infoVerTag!;
            }

            var summaryTag = Docstrings.Parse.GetSummary(cls);
            if (!string.IsNullOrWhiteSpace(summaryTag))
            {
                doc.Info.Summary = summaryTag;
            }

            var descTag = Docstrings.Parse.GetTag(cls, "arazzo-description");
            if (!string.IsNullOrWhiteSpace(descTag))
            {
                doc.Info.Description = descTag;
            }

            var infoExts = Docstrings.Parse.GetTagsWithAttributes(cls, "arazzo-info-ext").ToList();
            if (infoExts.Count > 0)
            {
                doc.Info.Extensions = new Dictionary<string, JsonElement>();
                foreach (var ext in infoExts)
                {
                    var k = ext.Attributes["key"];
                    using var jDoc = JsonDocument.Parse(ext.Text);
                    doc.Info.Extensions[k] = jDoc.RootElement.Clone();
                }
            }

            var sources = Docstrings.Parse.GetTagsWithAttributes(cls, "arazzo-source").ToList();
            if (sources.Count > 0)
            {
                doc.SourceDescriptions = new List<ArazzoSourceDescription>();
                foreach (var src in sources)
                {
                    var sd = JsonSerializer.Deserialize(src.Text, ArazzoJsonContext.Default.ArazzoSourceDescription)!;
                    doc.SourceDescriptions.Add(sd);
                }
            }

            var compTag = Docstrings.Parse.GetTag(cls, "arazzo-components");
            if (!string.IsNullOrWhiteSpace(compTag))
            {
                doc.Components = JsonSerializer.Deserialize(compTag!, ArazzoJsonContext.Default.ArazzoComponents);
            }

            var docExts = Docstrings.Parse.GetTagsWithAttributes(cls, "arazzo-ext").ToList();
            if (docExts.Count > 0)
            {
                doc.Extensions = new Dictionary<string, JsonElement>();
                foreach (var ext in docExts)
                {
                    var k = ext.Attributes["key"];
                    using var jDoc = JsonDocument.Parse(ext.Text);
                    doc.Extensions[k] = jDoc.RootElement.Clone();
                }
            }
        }

        /// <summary>
        /// Extracts <see cref="ArazzoWorkflow"/> definitions from structured XML documentation tags attached to a syntax node.
        /// </summary>
        /// <param name="node">The syntax node carrying <c>&lt;arazzo-workflow&gt;</c> and <c>&lt;arazzo-step&gt;</c> tags.</param>
        /// <returns>A list of reconstructed <see cref="ArazzoWorkflow"/> instances, or an empty list if no structured tags are present.</returns>
        internal static List<ArazzoWorkflow> ExtractWorkflowsFromXmlTags(SyntaxNode node)
        {
            var workflows = new List<ArazzoWorkflow>();
            var trivia = node.GetLeadingTrivia()
                .Select(i => i.GetStructure())
                .OfType<DocumentationCommentTriviaSyntax>()
                .FirstOrDefault();

            if (trivia == null) return workflows;

            ArazzoWorkflow? currentWf = null;
            var methodSummary = Docstrings.Parse.GetSummary(node);

            foreach (var xmlNode in trivia.ChildNodes())
            {
                if (xmlNode is XmlEmptyElementSyntax emptyElem && emptyElem.Name.ToString() == "arazzo-workflow")
                {
                    var wfId = GetXmlAttribute(emptyElem.Attributes, "id");
                    currentWf = new ArazzoWorkflow
                    {
                        WorkflowId = wfId,
                        Summary = methodSummary,
                        Steps = new List<ArazzoStep>()
                    };
                    workflows.Add(currentWf);
                }
                else if (xmlNode is XmlElementSyntax elem && currentWf != null)
                {
                    var tagName = elem.StartTag.Name.ToString();
                    var innerText = string.Join(" ", elem.Content
                        .OfType<XmlTextSyntax>()
                        .SelectMany(t => t.TextTokens)
                        .Select(t => t.ValueText.Trim())
                        .Where(t => !string.IsNullOrEmpty(t)));

                    if (tagName == "arazzo-workflow-description")
                    {
                        currentWf.Description = innerText;
                    }
                    else if (tagName == "arazzo-workflow-inputs")
                    {
                        currentWf.Inputs = JsonSerializer.Deserialize(innerText, Cdd.OpenApi.Parse.OpenApiJsonContext.Default.OpenApiSchema);
                    }
                    else if (tagName == "arazzo-depends-on")
                    {
                        currentWf.DependsOn ??= new List<string>();
                        currentWf.DependsOn.Add(innerText);
                    }
                    else if (tagName == "arazzo-workflow-param")
                    {
                        currentWf.Parameters ??= new List<ArazzoParameter>();
                        currentWf.Parameters.Add(JsonSerializer.Deserialize(innerText, ArazzoJsonContext.Default.ArazzoParameter)!);
                    }
                    else if (tagName == "arazzo-workflow-success-action")
                    {
                        currentWf.SuccessActions ??= new List<ArazzoSuccessAction>();
                        currentWf.SuccessActions.Add(JsonSerializer.Deserialize(innerText, ArazzoJsonContext.Default.ArazzoSuccessAction)!);
                    }
                    else if (tagName == "arazzo-workflow-failure-action")
                    {
                        currentWf.FailureActions ??= new List<ArazzoFailureAction>();
                        currentWf.FailureActions.Add(JsonSerializer.Deserialize(innerText, ArazzoJsonContext.Default.ArazzoFailureAction)!);
                    }
                    else if (tagName == "arazzo-workflow-output")
                    {
                        var key = GetXmlAttribute(elem.StartTag.Attributes, "key");
                        currentWf.Outputs ??= new Dictionary<string, string>();
                        currentWf.Outputs[key] = innerText;
                    }
                    else if (tagName == "arazzo-workflow-ext")
                    {
                        var key = GetXmlAttribute(elem.StartTag.Attributes, "key");
                        using var jDoc = JsonDocument.Parse(innerText);
                        currentWf.Extensions ??= new Dictionary<string, JsonElement>();
                        currentWf.Extensions[key] = jDoc.RootElement.Clone();
                    }
                    else if (tagName == "arazzo-step")
                    {
                        var step = JsonSerializer.Deserialize(innerText, ArazzoJsonContext.Default.ArazzoStep)!;
                        currentWf.Steps.Add(step);
                    }
                }
            }

            return workflows;
        }

        /// <summary>
        /// Reads the value of a named XML attribute from a syntax list of XML attributes.
        /// </summary>
        /// <param name="attributes">The XML attribute syntax list.</param>
        /// <param name="name">The attribute name to look up.</param>
        /// <returns>The attribute value string.</returns>
        private static string GetXmlAttribute(SyntaxList<XmlAttributeSyntax> attributes, string name)
        {
            var attr = attributes.OfType<XmlTextAttributeSyntax>().First(a => a.Name.ToString() == name);
            return string.Join(string.Empty, attr.TextTokens.Select(t => t.ValueText));
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
                var taggedWorkflows = ExtractWorkflowsFromXmlTags(method);
                if (taggedWorkflows.Count > 0)
                {
                    foreach (var taggedWf in taggedWorkflows)
                    {
                        if (!doc.Workflows.Any(w => w.WorkflowId == taggedWf.WorkflowId))
                        {
                            doc.Workflows.Add(taggedWf);
                        }
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
