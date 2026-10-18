// <copyright file="emit.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Cdd.OpenApi.Arazzo.Clients;
using Cdd.OpenApi.Arazzo.Expressions;
using Cdd.OpenApi.Common;
using Cdd.OpenApi.Models.Arazzo;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cdd.OpenApi.Arazzo.Cli
{
    /// <summary>
    /// Emits C# Client SDK CLI workflow runner classes that directly reuse the generated SDK workflow orchestrator and DTOs.
    /// </summary>
    public static class Emit
    {
        /// <summary>
        /// Generates a C# CLI runner class (<c>{className}WorkflowCli</c>) that parses command-line flags and invokes the SDK <c>{orchestratorClassName}</c> workflow methods.
        /// </summary>
        /// <param name="className">The name of the generated CLI class (e.g., <c>"PetstoreWorkflowCli"</c>).</param>
        /// <param name="orchestratorClassName">The name of the SDK workflow orchestrator class to reuse (e.g., <c>"PetstoreWorkflows"</c>).</param>
        /// <param name="doc">The source <see cref="ArazzoDocument"/>.</param>
        /// <returns>A Roslyn <see cref="ClassDeclarationSyntax"/> representing the CLI workflow runner.</returns>
        public static ClassDeclarationSyntax ToWorkflowCli(string className, string orchestratorClassName, ArazzoDocument doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            var dq = ((char)34).ToString();
            var sb = new StringBuilder();

            Clients.Emit.AppendDocMetadataHeader(sb, doc, "Arazzo CLI workflow runner.");
            sb.AppendLine($"public class {className}");
            sb.AppendLine("{");
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
            sb.AppendLine("    /// <summary>Executes the CLI workflow runner entry point reusing the generated SDK workflow orchestrator.</summary>");
            sb.AppendLine("    public static async System.Threading.Tasks.Task<int> Main(string[] args)");
            sb.AppendLine("    {");
            sb.AppendLine($"        if (args.Length == 0 || args[0] == {dq}--help{dq} || args[0] == {dq}-h{dq})");
            sb.AppendLine("        {");
            sb.AppendLine($"            System.Console.WriteLine({dq}{Clients.Emit.EscapeCSharp(doc.Info.Title)} ({Clients.Emit.EscapeCSharp(doc.Info.Version)}) - {Clients.Emit.EscapeCSharp(doc.Info.Summary ?? "CLI Workflow Runner")}{dq});");
            sb.AppendLine("            return 0;");
            sb.AppendLine("        }");
            sb.AppendLine($"        if (args[0] == {dq}--version{dq})");
            sb.AppendLine("        {");
            sb.AppendLine($"            System.Console.WriteLine({dq}{Clients.Emit.EscapeCSharp(doc.Info.Version)}{dq});");
            sb.AppendLine("            return 0;");
            sb.AppendLine("        }");
            sb.AppendLine($"        var workflows = new {orchestratorClassName}();");
            sb.AppendLine($"        var command = args[0] == {dq}workflow{dq} && args.Length > 1 ? args[1] : args[0];");
            sb.AppendLine("        switch (command)");
            sb.AppendLine("        {");

            foreach (var wf in doc.Workflows)
            {
                var wfPascal = RuntimeExpression.ToPascalIdentifier(wf.WorkflowId);
                var wfJson = JsonSerializer.Serialize(wf, ArazzoJsonContext.Default.ArazzoWorkflow);
                var wfB64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(wfJson));

                sb.AppendLine($"            case {dq}{Clients.Emit.EscapeCSharp(wf.WorkflowId)}{dq}:");
                sb.AppendLine("            {");
                sb.AppendLine($"                string workflowSpec = {dq}{wfB64}{dq};");
                sb.AppendLine($"                var wfInput = new {wfPascal}Input();");
                if (wf.Inputs?.Properties != null)
                {
                    foreach (var prop in wf.Inputs.Properties)
                    {
                        var propPascal = RuntimeExpression.ToPascalIdentifier(prop.Key);
                        var csType = TypeMapper.MapOpenApiToCSharp(prop.Value.Type, "string");
                        sb.AppendLine($"                for (int i = 1; i < args.Length - 1; i++)");
                        sb.AppendLine("                {");
                        sb.AppendLine($"                    if (args[i] == {dq}--{Clients.Emit.EscapeCSharp(prop.Key)}{dq})");
                        sb.AppendLine("                    {");
                        if (csType == "int")
                        {
                            sb.AppendLine($"                        wfInput.{propPascal} = int.Parse(args[i + 1]);");
                        }
                        else if (csType == "bool")
                        {
                            sb.AppendLine($"                        wfInput.{propPascal} = bool.Parse(args[i + 1]);");
                        }
                        else if (csType == "double")
                        {
                            sb.AppendLine($"                        wfInput.{propPascal} = double.Parse(args[i + 1]);");
                        }
                        else
                        {
                            sb.AppendLine($"                        wfInput.{propPascal} = args[i + 1];");
                        }

                        sb.AppendLine("                    }");
                        sb.AppendLine("                }");
                    }
                }

                sb.AppendLine($"                var wfOutput = await workflows.{wfPascal}Async(wfInput);");
                sb.AppendLine("                foreach (var kvp in wfOutput.Values)");
                sb.AppendLine("                {");
                sb.AppendLine($"                    System.Console.WriteLine(kvp.Key + {dq}={dq} + kvp.Value);");
                sb.AppendLine("                }");
                sb.AppendLine("                return 0;");
                sb.AppendLine("            }");
            }

            sb.AppendLine("            default:");
            sb.AppendLine($"                System.Console.Error.WriteLine({dq}Unknown workflow: {dq} + command);");
            sb.AppendLine("                return 1;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            var parsedTree = CSharpSyntaxTree.ParseText(sb.ToString());
            return parsedTree.GetRoot().ChildNodes().OfType<ClassDeclarationSyntax>().First();
        }
    }
}
