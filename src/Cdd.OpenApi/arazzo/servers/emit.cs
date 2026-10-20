// <copyright file="emit.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cdd.OpenApi.Arazzo.Clients;
using Cdd.OpenApi.Arazzo.Expressions;
using Cdd.OpenApi.Common;
using Cdd.OpenApi.Models.Arazzo;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cdd.OpenApi.Arazzo.Servers
{
    /// <summary>
    /// Emits C# Server/Backend xUnit end-to-end workflow test classes and stateful workflow mock engine classes from an <see cref="Cdd.OpenApi.Ir.IrService"/> or <see cref="ArazzoDocument"/>.
    /// </summary>
    public static class Emit
    {
        /// <summary>
        /// Generates an xUnit end-to-end workflow test class (<c>{className}</c>) from an <see cref="Cdd.OpenApi.Ir.IrService"/>.
        /// </summary>
        /// <param name="className">The name of the generated test class (e.g., <c>"PetstoreWorkflowTests"</c>).</param>
        /// <param name="orchestratorClassName">The name of the workflow orchestrator or mock class used by the tests.</param>
        /// <param name="service">The source <see cref="Cdd.OpenApi.Ir.IrService"/>.</param>
        /// <returns>A Roslyn <see cref="ClassDeclarationSyntax"/> representing the xUnit workflow test suite.</returns>
        public static ClassDeclarationSyntax ToWorkflowTestsFromIr(string className, string orchestratorClassName, Cdd.OpenApi.Ir.IrService service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            return ToWorkflowTests(className, orchestratorClassName, service.ToArazzo());
        }

        /// <summary>
        /// Generates a stateful workflow mock engine class (<c>{className}</c>) from an <see cref="Cdd.OpenApi.Ir.IrService"/>.
        /// </summary>
        /// <param name="className">The name of the generated mock class (e.g., <c>"PetstoreWorkflowMock"</c>).</param>
        /// <param name="service">The source <see cref="Cdd.OpenApi.Ir.IrService"/>.</param>
        /// <returns>A Roslyn <see cref="ClassDeclarationSyntax"/> representing the stateful workflow mock engine.</returns>
        public static ClassDeclarationSyntax ToWorkflowMockFromIr(string className, Cdd.OpenApi.Ir.IrService service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            return ToWorkflowMock(className, service.ToArazzo());
        }

        /// <summary>
        /// Generates an xUnit end-to-end workflow test class (<c>{className}</c>) that executes and asserts every workflow in <paramref name="doc"/>.
        /// </summary>
        /// <param name="className">The name of the generated test class (e.g., <c>"PetstoreWorkflowTests"</c>).</param>
        /// <param name="orchestratorClassName">The name of the workflow orchestrator or mock class used by the tests.</param>
        /// <param name="doc">The source <see cref="ArazzoDocument"/>.</param>
        /// <returns>A Roslyn <see cref="ClassDeclarationSyntax"/> representing the xUnit workflow test suite.</returns>
        public static ClassDeclarationSyntax ToWorkflowTests(string className, string orchestratorClassName, ArazzoDocument doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            var sb = new StringBuilder();

            Clients.Emit.AppendDocMetadataHeader(sb, doc, "Arazzo E2E server workflow test suite.");
            sb.AppendLine($"public class {className}");
            sb.AppendLine("{");

            foreach (var wf in doc.Workflows)
            {
                var wfPascal = RuntimeExpression.ToPascalIdentifier(wf.WorkflowId);
                var summary = wf.Summary ?? wf.Description ?? $"End-to-end server test for workflow '{wf.WorkflowId}'.";

                sb.AppendLine($"    /// <summary>{Clients.Emit.EscapeXml(summary)}</summary>");
                Clients.Emit.AppendWorkflowXmlMetadata(sb, wf, "    ");
                sb.AppendLine("    [Xunit.Fact]");
                sb.AppendLine($"    public async System.Threading.Tasks.Task {wfPascal}WorkflowTest()");
                sb.AppendLine("    {");
                sb.AppendLine($"        var workflows = new {orchestratorClassName}();");
                sb.AppendLine($"        var input = new {wfPascal}Input();");
                sb.AppendLine($"        var result = await workflows.{wfPascal}Async(input);");
                sb.AppendLine("        Xunit.Assert.NotNull(result);");
                if (wf.Outputs != null)
                {
                    foreach (var outKvp in wf.Outputs)
                    {
                        var outProp = RuntimeExpression.ToPascalIdentifier(outKvp.Key);
                        sb.AppendLine($"        Xunit.Assert.NotNull(result.{outProp});");
                    }
                }

                sb.AppendLine("    }");
            }

            sb.AppendLine("}");

            var parsedTree = CSharpSyntaxTree.ParseText(sb.ToString());
            return parsedTree.GetRoot().ChildNodes().OfType<ClassDeclarationSyntax>().First();
        }

        /// <summary>
        /// Generates a stateful server-side workflow mock &amp; engine class (<c>{className}</c>) from an <see cref="ArazzoDocument"/>.
        /// </summary>
        /// <param name="className">The name of the generated mock class (e.g., <c>"PetstoreWorkflowMock"</c>).</param>
        /// <param name="doc">The source <see cref="ArazzoDocument"/>.</param>
        /// <returns>A Roslyn <see cref="ClassDeclarationSyntax"/> representing the stateful workflow mock engine.</returns>
        public static ClassDeclarationSyntax ToWorkflowMock(string className, ArazzoDocument doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            var dq = StringEscaper.DoubleQuote;
            var sb = new StringBuilder();

            Clients.Emit.AppendDocMetadataHeader(sb, doc, "Stateful server workflow mock and step simulator.");
            sb.AppendLine($"public class {className}");
            sb.AppendLine("{");
            sb.AppendLine("    /// <summary>Gets the recorded step execution log for stateful mock verification.</summary>");
            sb.AppendLine("    public readonly System.Collections.Generic.List<string> ExecutedSteps = new System.Collections.Generic.List<string>();");

            foreach (var wf in doc.Workflows)
            {
                var wfPascal = RuntimeExpression.ToPascalIdentifier(wf.WorkflowId);
                var summary = wf.Summary ?? $"Simulates server-side execution of workflow '{wf.WorkflowId}'.";

                sb.AppendLine($"    /// <summary>{Clients.Emit.EscapeXml(summary)}</summary>");
                Clients.Emit.AppendWorkflowXmlMetadata(sb, wf, "    ");
                sb.AppendLine($"    public System.Threading.Tasks.Task<{wfPascal}Output> {wfPascal}Async({wfPascal}Input input)");
                sb.AppendLine("    {");
                foreach (var step in wf.Steps)
                {
                    sb.AppendLine($"        ExecutedSteps.Add({dq}{Clients.Emit.EscapeCSharp(wf.WorkflowId)}:{Clients.Emit.EscapeCSharp(step.StepId)}{dq});");
                }

                sb.AppendLine($"        var output = new {wfPascal}Output();");
                if (wf.Outputs != null)
                {
                    foreach (var kvp in wf.Outputs)
                    {
                        var propPascal = RuntimeExpression.ToPascalIdentifier(kvp.Key);
                        sb.AppendLine($"        output.Values[{dq}{Clients.Emit.EscapeCSharp(kvp.Key)}{dq}] = {dq}{Clients.Emit.EscapeCSharp(kvp.Value)}{dq};");
                        sb.AppendLine($"        output.{propPascal} = {dq}{Clients.Emit.EscapeCSharp(kvp.Value)}{dq};");
                    }
                }

                sb.AppendLine("        return System.Threading.Tasks.Task.FromResult(output);");
                sb.AppendLine("    }");
            }

            sb.AppendLine("}");

            var parsedTree = CSharpSyntaxTree.ParseText(sb.ToString());
            return parsedTree.GetRoot().ChildNodes().OfType<ClassDeclarationSyntax>().First();
        }
    }
}
