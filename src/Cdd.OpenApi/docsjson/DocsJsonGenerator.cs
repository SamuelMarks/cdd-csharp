// <copyright file="DocsJsonGenerator.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Cdd.OpenApi.Arazzo.Expressions;
using Cdd.OpenApi.Common;
using Cdd.OpenApi.Models;
using Cdd.OpenApi.Models.Arazzo;

namespace Cdd.OpenApi.DocsJson
{
    /// <summary>Generates JSON documentation.</summary>
    public static class DocsJsonGenerator
    {
        /// <summary>
        /// Generates the documentation JSON string containing C# code snippets for all operations, webhooks, and workflows in the OpenAPI document.
        /// </summary>
        /// <param name="doc">The OpenAPI document to generate documentation snippets for.</param>
        /// <param name="noImports">Whether to omit the <c>imports</c> field from generated code objects.</param>
        /// <param name="noWrapping">Whether to omit the <c>wrapper_start</c> and <c>wrapper_end</c> fields from generated code objects.</param>
        /// <returns>The serialized documentation JSON string.</returns>
        public static string Generate(OpenApiDocument doc, bool noImports, bool noWrapping)
        {
            var output = new DocsJsonOutput
            {
                Language = "csharp",
                Operations = new List<DocsJsonOperation>()
            };

            if (doc.Paths != null)
            {
                foreach (var pathKvp in doc.Paths)
                {
                    AppendPathItemOperations(output.Operations, pathKvp.Key, pathKvp.Value, noImports, noWrapping);
                }
            }

            if (doc.Webhooks != null)
            {
                foreach (var webhookKvp in doc.Webhooks)
                {
                    AppendPathItemOperations(output.Operations, webhookKvp.Key, webhookKvp.Value, noImports, noWrapping);
                }
            }

            if (doc.Workflows != null)
            {
                AppendWorkflowOperations(output.Operations, doc.Workflows, noImports, noWrapping);
            }

            var resultList = new List<DocsJsonOutput> { output };
            return JsonSerializer.Serialize(resultList, DocsJsonContext.Default.ListDocsJsonOutput);
        }

        /// <summary>
        /// Generates the documentation JSON string containing C# code snippets for all workflows in the Arazzo document.
        /// </summary>
        /// <param name="doc">The Arazzo document to generate documentation snippets for.</param>
        /// <param name="noImports">Whether to omit the <c>imports</c> field from generated code objects.</param>
        /// <param name="noWrapping">Whether to omit the <c>wrapper_start</c> and <c>wrapper_end</c> fields from generated code objects.</param>
        /// <returns>The serialized documentation JSON string.</returns>
        public static string Generate(ArazzoDocument doc, bool noImports, bool noWrapping)
        {
            var output = new DocsJsonOutput
            {
                Language = "csharp",
                Operations = new List<DocsJsonOperation>()
            };

            AppendWorkflowOperations(output.Operations, doc.Workflows, noImports, noWrapping);

            var resultList = new List<DocsJsonOutput> { output };
            return JsonSerializer.Serialize(resultList, DocsJsonContext.Default.ListDocsJsonOutput);
        }

        /// <summary>
        /// Appends documentation operation entries for all HTTP methods defined on an <see cref="OpenApiPathItem"/>.
        /// </summary>
        /// <param name="target">The target list of documentation operations.</param>
        /// <param name="routePath">The route path or webhook name.</param>
        /// <param name="pathItem">The OpenAPI path item containing the operations.</param>
        /// <param name="noImports">Whether to omit the <c>imports</c> field.</param>
        /// <param name="noWrapping">Whether to omit the <c>wrapper_start</c> and <c>wrapper_end</c> fields.</param>
        private static void AppendPathItemOperations(
            IList<DocsJsonOperation> target,
            string routePath,
            OpenApiPathItem pathItem,
            bool noImports,
            bool noWrapping)
        {
            var operations = OperationHelpers.GetOperationsByPascalVerb(pathItem);

            foreach (var opKvp in operations)
            {
                var method = opKvp.Key.ToUpperInvariant();
                var operation = opKvp.Value;
                if (operation == null) continue;

                var operationId = operation.OperationId;
                var methodName = operationId ?? $"{method}{routePath.Replace("/", "").Replace("{", "").Replace("}", "")}";

                var opOut = new DocsJsonOperation
                {
                    Method = method,
                    Path = routePath,
                    OperationId = operation.OperationId,
                    Code = new DocsJsonCode()
                };

                if (!noImports)
                {
                    var importsLines = new List<string>
                    {
                        "using System;",
                        "using System.Threading.Tasks;",
                        "using Generated.Api;",
                        "using Generated.Models;"
                    };
                    opOut.Code.Imports = string.Join("\n", importsLines);
                }

                var tag = "Default";
                if (operation.Tags != null && operation.Tags.Count > 0 && !string.IsNullOrEmpty(operation.Tags[0]))
                {
                    tag = char.ToUpperInvariant(operation.Tags[0][0]) + operation.Tags[0].Substring(1);
                }
                var clientName = $"{tag}ApiClient";

                if (!noWrapping)
                {
                    var wrapStart = new List<string>
                    {
                        "public class Example",
                        "{",
                        "    public static async Task Main()",
                        "    {",
                        $"        var client = new {clientName}();"
                    };
                    opOut.Code.WrapperStart = string.Join("\n", wrapStart);

                    var wrapEnd = new List<string>
                    {
                        "    }",
                        "}"
                    };
                    opOut.Code.WrapperEnd = string.Join("\n", wrapEnd);
                }

                var snippetLines = new List<string>();

                var parameters = new List<string>();
                if (operation.Parameters != null)
                {
                    foreach (var param in operation.Parameters)
                    {
                        if (param == null) continue;
                        if (string.IsNullOrEmpty(param.Name)) continue;
                        var pName = param.Name;
                        var pType = TypeMapper.MapOpenApiToCSharp(param.Schema?.Type, "object");
                        var pValue = GetDefaultValueForType(pType);
                        snippetLines.Add($"{pType} {pName} = {pValue};");
                        parameters.Add(pName);
                    }
                }

                if (operation.RequestBody != null)
                {
                    snippetLines.Add(BuildRequestBodySnippetLine(operation.RequestBody));
                    parameters.Add("requestBody");
                }

                var paramsString = string.Join(", ", parameters);
                snippetLines.Add($"var response = await client.{methodName}Async({paramsString});");

                opOut.Code.Snippet = string.Join("\n", snippetLines);

                target.Add(opOut);
            }
        }

        /// <summary>
        /// Builds the C# declaration and initialization line for an operation's request body.
        /// </summary>
        /// <param name="requestBody">The OpenAPI request body definition.</param>
        /// <returns>A C# statement initializing <c>requestBody</c>.</returns>
        private static string BuildRequestBodySnippetLine(OpenApiRequestBody requestBody)
        {
            OpenApiSchema? bodySchema = null;
            if (requestBody.Content != null)
            {
                if (requestBody.Content.TryGetValue("application/json", out var jsonMedia) && jsonMedia?.Schema != null)
                {
                    bodySchema = jsonMedia.Schema;
                }
                else
                {
                    bodySchema = requestBody.Content.Values.FirstOrDefault(m => m?.Schema != null)?.Schema;
                }
            }

            if (bodySchema != null)
            {
                if (!string.IsNullOrEmpty(bodySchema.Ref))
                {
                    var modelName = bodySchema.Ref.Split('/').Last();
                    return $"var requestBody = new {modelName}();";
                }

                if (bodySchema.Type == OpenApiDataType.Array)
                {
                    var itemType = TypeMapper.MapSchemaToCSharp(bodySchema.Items, "object");
                    return $"var requestBody = new List<{itemType}>();";
                }

                if (bodySchema.Type == OpenApiDataType.Object)
                {
                    return "var requestBody = new object();";
                }

                if (!string.IsNullOrEmpty(bodySchema.Type))
                {
                    var csharpType = TypeMapper.MapOpenApiToCSharp(bodySchema.Type, "object");
                    if (csharpType != "object")
                    {
                        var defaultValue = GetDefaultValueForType(csharpType);
                        return $"{csharpType} requestBody = {defaultValue};";
                    }

                    return "var requestBody = new object();";
                }
            }

            return "var requestBody = new object();";
        }

        /// <summary>
        /// Appends documentation operation entries for all Arazzo workflows.
        /// </summary>
        /// <param name="target">The target list of documentation operations.</param>
        /// <param name="workflows">The list of Arazzo workflows.</param>
        /// <param name="noImports">Whether to omit the <c>imports</c> field.</param>
        /// <param name="noWrapping">Whether to omit the <c>wrapper_start</c> and <c>wrapper_end</c> fields.</param>
        private static void AppendWorkflowOperations(
            IList<DocsJsonOperation> target,
            IList<ArazzoWorkflow> workflows,
            bool noImports,
            bool noWrapping)
        {
            foreach (var wf in workflows)
            {
                var wfPascal = RuntimeExpression.ToPascalIdentifier(wf.WorkflowId);
                var opOut = new DocsJsonOperation
                {
                    Method = "WORKFLOW",
                    Path = $"/workflows/{wf.WorkflowId}",
                    OperationId = wf.WorkflowId,
                    Code = new DocsJsonCode()
                };

                if (!noImports)
                {
                    var importsLines = new List<string>
                    {
                        "using System;",
                        "using System.Threading.Tasks;",
                        "using Generated.Workflows;",
                        "using Generated.Workflows.Models;"
                    };
                    opOut.Code.Imports = string.Join("\n", importsLines);
                }

                if (!noWrapping)
                {
                    var wrapStart = new List<string>
                    {
                        "public class Example",
                        "{",
                        "    public static async Task Main()",
                        "    {",
                        "        var workflows = new WorkflowsOrchestrator();"
                    };
                    opOut.Code.WrapperStart = string.Join("\n", wrapStart);

                    var wrapEnd = new List<string>
                    {
                        "    }",
                        "}"
                    };
                    opOut.Code.WrapperEnd = string.Join("\n", wrapEnd);
                }

                var snippetLines = new List<string>();

                snippetLines.Add($"var input = new {wfPascal}Input();");
                if (wf.Inputs?.Properties != null)
                {
                    foreach (var prop in wf.Inputs.Properties)
                    {
                        var propPascal = RuntimeExpression.ToPascalIdentifier(prop.Key);
                        var pType = TypeMapper.MapOpenApiToCSharp(prop.Value.Type, "object");
                        var pValue = GetDefaultValueForType(pType);
                        snippetLines.Add($"input.{propPascal} = {pValue};");
                    }
                }

                snippetLines.Add($"var response = await workflows.{wfPascal}Async(input);");

                opOut.Code.Snippet = string.Join("\n", snippetLines);
                target.Add(opOut);
            }
        }

        /// <summary>
        /// Returns a sample C# literal expression string for a given C# type name.
        /// </summary>
        /// <param name="csharpType">The C# type name.</param>
        /// <returns>A C# literal expression string representing a default sample value.</returns>
        private static string GetDefaultValueForType(string csharpType)
        {
            return csharpType switch
            {
                "int" => "0",
                "double" => "0.0",
                "bool" => "false",
                "string" => "\"example\"",
                _ => "null"
            };
        }
    }
}
