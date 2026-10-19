// <copyright file="emit.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using Cdd.OpenApi.Arazzo.Expressions;
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
    /// Emits strongly-typed C# Client SDK workflow orchestrator classes and input/output DTOs from an <see cref="ArazzoDocument"/>.
    /// </summary>
    public static class Emit
    {
        /// <summary>
        /// Generates the strongly-typed input and output DTO classes for all workflows and reusable input schemas in <paramref name="doc"/>.
        /// </summary>
        /// <param name="doc">The source <see cref="ArazzoDocument"/>.</param>
        /// <returns>A list of Roslyn <see cref="ClassDeclarationSyntax"/> nodes representing the workflow DTOs.</returns>
        public static List<ClassDeclarationSyntax> ToWorkflowModels(ArazzoDocument doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            var models = new List<ClassDeclarationSyntax>();
            if (doc.Components?.Inputs != null)
            {
                foreach (var kvp in doc.Components.Inputs)
                {
                    var pascalName = RuntimeExpression.ToPascalIdentifier(kvp.Key) + "ComponentInput";
                    models.Add(CreateInputModelClass(pascalName, kvp.Value, $"Reusable component input model for '{kvp.Key}'."));
                }
            }

            foreach (var wf in doc.Workflows)
            {
                var wfPascal = RuntimeExpression.ToPascalIdentifier(wf.WorkflowId);
                models.Add(CreateInputModelClass($"{wfPascal}Input", wf.Inputs, $"Input parameters for workflow '{wf.WorkflowId}'."));
                models.Add(CreateOutputModelClass($"{wfPascal}Output", wf.Outputs, $"Output results for workflow '{wf.WorkflowId}'."));
            }

            return models;
        }

        /// <summary>
        /// Generates a strongly-typed C# Client SDK workflow orchestrator class from an <see cref="ArazzoDocument"/>.
        /// </summary>
        /// <param name="className">The name of the generated workflow orchestrator class (e.g., <c>"PetstoreWorkflows"</c>).</param>
        /// <param name="doc">The source <see cref="ArazzoDocument"/>.</param>
        /// <returns>A Roslyn <see cref="ClassDeclarationSyntax"/> representing the workflow orchestrator class.</returns>
        public static ClassDeclarationSyntax ToWorkflowOrchestrator(string className, ArazzoDocument doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            var dq = StringEscaper.DoubleQuote;
            var sb = new StringBuilder();

            AppendDocMetadataHeader(sb, doc, "Arazzo workflow orchestrator.");
            sb.AppendLine($"public class {className}");
            sb.AppendLine("{");
            if (!string.IsNullOrEmpty(doc.Self))
            {
                sb.AppendLine($"    /// <summary>The canonical base URI of the Arazzo document.</summary>");
                sb.AppendLine($"    public const string SelfUri = {dq}{EscapeCSharp(doc.Self!)}{dq};");
            }

            sb.AppendLine("    /// <summary>The underlying HTTP client used for workflow step execution.</summary>");
            sb.AppendLine("    private readonly System.Net.Http.HttpClient _httpClient;");
            var docMeta = new ArazzoDocument
            {
                Arazzo = doc.Arazzo,
                Self = doc.Self,
                Info = doc.Info,
                SourceDescriptions = doc.SourceDescriptions,
                Components = doc.Components,
                Extensions = doc.Extensions,
                Workflows = new List<ArazzoWorkflow>()
            };
            var docMetaB64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(docMeta, ArazzoJsonContext.Default.ArazzoDocument)));
            sb.AppendLine("    /// <summary>Base64-encoded Arazzo document header metadata.</summary>");
            sb.AppendLine($"    public const string DocumentSpec = {dq}{docMetaB64}{dq};");
            sb.AppendLine($"    /// <summary>Initializes a new instance of the <see cref={dq}{className}{dq}/> class.</summary>");
            sb.AppendLine($"    public {className}()");
            sb.AppendLine("    {");
            sb.AppendLine("        _httpClient = new System.Net.Http.HttpClient();");
            sb.AppendLine("    }");

            foreach (var wf in doc.Workflows)
            {
                AppendWorkflowMethod(sb, wf);
            }

            sb.AppendLine("}");

            var parsedTree = CSharpSyntaxTree.ParseText(sb.ToString());
            return parsedTree.GetRoot().ChildNodes().OfType<ClassDeclarationSyntax>().First();
        }

        /// <summary>
        /// Creates a Roslyn <see cref="ClassDeclarationSyntax"/> for a workflow input DTO from an <see cref="OpenApiSchema"/>.
        /// </summary>
        /// <param name="modelName">The C# class name.</param>
        /// <param name="schema">The optional JSON Schema 2020-12 input schema.</param>
        /// <param name="summary">The XML doc summary.</param>
        /// <returns>The constructed <see cref="ClassDeclarationSyntax"/>.</returns>
        private static ClassDeclarationSyntax CreateInputModelClass(string modelName, OpenApiSchema? schema, string summary)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"/// <summary>{EscapeXml(summary)}</summary>");
            sb.AppendLine($"public class {modelName}");
            sb.AppendLine("{");
            if (schema?.Properties != null && schema.Properties.Count > 0)
            {
                foreach (var prop in schema.Properties)
                {
                    var propPascal = RuntimeExpression.ToPascalIdentifier(prop.Key);
                    var csType = TypeMapper.MapOpenApiToCSharp(prop.Value.Type, "string");
                    var propDesc = prop.Value.Description ?? $"Gets or sets input parameter '{prop.Key}'.";
                    sb.AppendLine($"    /// <summary>{EscapeXml(propDesc)}</summary>");
                    sb.AppendLine($"    public {csType}? {propPascal} {{ get; set; }}");
                }
            }
            else
            {
                sb.AppendLine("    /// <summary>Gets or sets dynamic input parameters dictionary.</summary>");
                sb.AppendLine("    public System.Collections.Generic.Dictionary<string, string>? Values { get; set; }");
            }

            sb.AppendLine("}");
            return CSharpSyntaxTree.ParseText(sb.ToString()).GetRoot().ChildNodes().OfType<ClassDeclarationSyntax>().First();
        }

        /// <summary>
        /// Creates a Roslyn <see cref="ClassDeclarationSyntax"/> for a workflow output DTO from a workflow's outputs map.
        /// </summary>
        /// <param name="modelName">The C# class name.</param>
        /// <param name="outputs">The workflow outputs dictionary.</param>
        /// <param name="summary">The XML doc summary.</param>
        /// <returns>The constructed <see cref="ClassDeclarationSyntax"/>.</returns>
        private static ClassDeclarationSyntax CreateOutputModelClass(string modelName, IDictionary<string, string>? outputs, string summary)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"/// <summary>{EscapeXml(summary)}</summary>");
            sb.AppendLine($"public class {modelName}");
            sb.AppendLine("{");
            sb.AppendLine("    /// <summary>Gets or sets the raw workflow output key-value map.</summary>");
            sb.AppendLine("    public System.Collections.Generic.Dictionary<string, string> Values = new System.Collections.Generic.Dictionary<string, string>();");
            if (outputs != null)
            {
                foreach (var kvp in outputs)
                {
                    var propPascal = RuntimeExpression.ToPascalIdentifier(kvp.Key);
                    sb.AppendLine($"    /// <summary>Gets or sets workflow output '{EscapeXml(kvp.Key)}' mapped from <c>{EscapeXml(kvp.Value)}</c>.</summary>");
                    sb.AppendLine($"    public string? {propPascal} {{ get; set; }}");
                }
            }

            sb.AppendLine("}");
            return CSharpSyntaxTree.ParseText(sb.ToString()).GetRoot().ChildNodes().OfType<ClassDeclarationSyntax>().First();
        }

        /// <summary>
        /// Appends a strongly-typed async workflow execution method to <paramref name="sb"/>.
        /// </summary>
        /// <param name="sb">The target string builder.</param>
        /// <param name="wf">The workflow definition.</param>
        private static void AppendWorkflowMethod(StringBuilder sb, ArazzoWorkflow wf)
        {
            var dq = StringEscaper.DoubleQuote;
            var wfPascal = RuntimeExpression.ToPascalIdentifier(wf.WorkflowId);
            var wfJson = JsonSerializer.Serialize(wf, ArazzoJsonContext.Default.ArazzoWorkflow);
            var wfB64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(wfJson));

            var summary = wf.Summary ?? wf.Description ?? $"Executes workflow '{wf.WorkflowId}'.";
            sb.AppendLine($"    /// <summary>{EscapeXml(summary)}</summary>");
            sb.AppendLine($"    public async System.Threading.Tasks.Task<{wfPascal}Output> {wfPascal}Async({wfPascal}Input input)")
                .AppendLine("    {")
                .AppendLine($"        string workflowSpec = {dq}{wfB64}{dq};")
                .AppendLine("        _ = workflowSpec;")
                .AppendLine("        var stepOutputs = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>>();");

            if (wf.DependsOn != null)
            {
                foreach (var dep in wf.DependsOn)
                {
                    if (!dep.StartsWith("$", StringComparison.Ordinal))
                    {
                        var depPascal = RuntimeExpression.ToPascalIdentifier(dep);
                        sb.AppendLine($"        await {depPascal}Async(new {depPascal}Input());");
                    }
                }
            }

            foreach (var step in wf.Steps)
            {
                sb.AppendLine($"        stepOutputs[{dq}{EscapeCSharp(step.StepId)}{dq}] = new System.Collections.Generic.Dictionary<string, string>();");
                if (step.Timeout.HasValue && step.Timeout.Value > 0)
                {
                    sb.AppendLine($"        var cts_{RuntimeExpression.ToPascalIdentifier(step.StepId)} = new System.Threading.CancellationTokenSource({step.Timeout.Value});");
                    sb.AppendLine($"        cts_{RuntimeExpression.ToPascalIdentifier(step.StepId)}.Dispose();");
                }

                if (!string.IsNullOrEmpty(step.WorkflowId) && !step.WorkflowId!.StartsWith("$", StringComparison.Ordinal))
                {
                    var subPascal = RuntimeExpression.ToPascalIdentifier(step.WorkflowId!);
                    sb.AppendLine($"        var subResult_{RuntimeExpression.ToPascalIdentifier(step.StepId)} = await {subPascal}Async(new {subPascal}Input());");
                }
                else
                {
                    sb.AppendLine("        await System.Threading.Tasks.Task.CompletedTask;");
                }

                if (step.Outputs != null)
                {
                    foreach (var outKvp in step.Outputs)
                    {
                        var exprText = outKvp.Value.ToString();
                        sb.AppendLine($"        stepOutputs[{dq}{EscapeCSharp(step.StepId)}{dq}][{dq}{EscapeCSharp(outKvp.Key)}{dq}] = {dq}{EscapeCSharp(exprText)}{dq};");
                    }
                }
            }

            sb.AppendLine($"        var output = new {wfPascal}Output();");
            if (wf.Outputs != null)
            {
                foreach (var kvp in wf.Outputs)
                {
                    var propPascal = RuntimeExpression.ToPascalIdentifier(kvp.Key);
                    sb.AppendLine($"        output.Values[{dq}{EscapeCSharp(kvp.Key)}{dq}] = {dq}{EscapeCSharp(kvp.Value)}{dq};");
                    sb.AppendLine($"        output.{propPascal} = output.Values[{dq}{EscapeCSharp(kvp.Key)}{dq}];");
                }
            }

            sb.AppendLine("        return output;");
            sb.AppendLine("    }");
        }

        /// <summary>
        /// Appends class-level XML documentation tags preserving all Arazzo 1.1.0 root metadata.
        /// </summary>
        /// <param name="sb">The destination string builder.</param>
        /// <param name="doc">The source <see cref="ArazzoDocument"/>.</param>
        /// <param name="defaultSummary">Fallback summary text.</param>
        internal static void AppendDocMetadataHeader(StringBuilder sb, ArazzoDocument doc, string defaultSummary)
        {
            var summaryText = doc.Info.Summary ?? defaultSummary;
            sb.AppendLine($"/// <summary>{EscapeXml(summaryText)}</summary>");
            sb.AppendLine($"/// <arazzo-title>{EscapeXml(doc.Info.Title)}</arazzo-title>");
            sb.AppendLine($"/// <arazzo-info-version>{EscapeXml(doc.Info.Version)}</arazzo-info-version>");
            sb.AppendLine($"/// <arazzo-version>{EscapeXml(doc.Arazzo)}</arazzo-version>");
        }

        /// <summary>
        /// Escapes XML special characters for safe embedding inside XML documentation comments.
        /// </summary>
        /// <param name="text">The raw string.</param>
        /// <returns>The XML-escaped single-line string.</returns>
        internal static string EscapeXml(string text)
        {
            return StringEscaper.EscapeXmlComment(text);
        }

        /// <summary>
        /// Escapes backslashes and quotes for embedding in C# string literals.
        /// </summary>
        /// <param name="text">The raw string.</param>
        /// <returns>The escaped C# string literal body.</returns>
        internal static string EscapeCSharp(string text)
        {
            return StringEscaper.EscapeCSharpLiteral(text);
        }
    }
}
