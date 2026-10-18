// <copyright file="parse.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Cdd.OpenApi.Errors;
using Cdd.OpenApi.Models.Arazzo;

namespace Cdd.OpenApi.Arazzo
{
    /// <summary>
    /// Parses and validates Arazzo 1.1.0 documents from JSON or YAML representations.
    /// </summary>
    public static class ArazzoParser
    {
        /// <summary>
        /// Regular expression pattern validating source description names.
        /// </summary>
        private static readonly Regex SourceNameRegex = new Regex(@"^[A-Za-z0-9_\-]+$", RegexOptions.Compiled);

        /// <summary>
        /// Parses a JSON or YAML string into a validated <see cref="ArazzoDocument"/> instance.
        /// </summary>
        /// <param name="content">The raw JSON or YAML string representing an Arazzo specification.</param>
        /// <returns>The parsed and validated <see cref="ArazzoDocument"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="content"/> is null or whitespace.</exception>
        /// <exception cref="CddException">Thrown when parsing or Arazzo 1.1.0 validation fails.</exception>
        public static ArazzoDocument ParseYamlOrJson(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("Arazzo content cannot be null or empty.", nameof(content));
            }

            var trimmed = content.TrimStart();
            if (trimmed.StartsWith("{", StringComparison.Ordinal))
            {
                return ParseJson(content);
            }

            var jsonNode = ConvertYamlToJsonNode(content);
            var json = jsonNode.ToJsonString();
            return ParseJson(json);
        }

        /// <summary>
        /// Parses a JSON string into a validated <see cref="ArazzoDocument"/> instance.
        /// </summary>
        /// <param name="json">The raw JSON string representing an Arazzo specification.</param>
        /// <returns>The parsed and validated <see cref="ArazzoDocument"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or whitespace.</exception>
        /// <exception cref="CddException">Thrown when JSON deserialization or Arazzo 1.1.0 validation fails.</exception>
        public static ArazzoDocument ParseJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentException("JSON string cannot be null or empty.", nameof(json));
            }

            ArazzoDocument? document;
            try
            {
                document = JsonSerializer.Deserialize(json, ArazzoJsonContext.Default.ArazzoDocument);
            }
            catch (JsonException ex)
            {
                throw new CddException(CddErrorCode.InvalidSpecification, $"Failed to parse Arazzo JSON: {ex.Message}", ex);
            }

            if (document == null)
            {
                throw new CddException(CddErrorCode.InvalidSpecification, "Deserialized Arazzo document was null.");
            }

            Validate(document);
            return document;
        }

        /// <summary>
        /// Validates an <see cref="ArazzoDocument"/> against Arazzo 1.0.x / 1.1.x semantic and structural rules.
        /// </summary>
        /// <param name="document">The <see cref="ArazzoDocument"/> to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="document"/> is null.</exception>
        /// <exception cref="CddException">Thrown when any semantic validation rule is violated.</exception>
        public static void Validate(ArazzoDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (string.IsNullOrWhiteSpace(document.Arazzo) ||
                (!document.Arazzo.StartsWith("1.0.", StringComparison.Ordinal) &&
                 !document.Arazzo.StartsWith("1.1.", StringComparison.Ordinal)))
            {
                throw new CddException(
                    CddErrorCode.InvalidSpecification,
                    $"Unsupported Arazzo version '{document.Arazzo}'. Expected '1.0.x' or '1.1.x'.");
            }

            if (string.IsNullOrWhiteSpace(document.Info.Title) || string.IsNullOrWhiteSpace(document.Info.Version))
            {
                throw new CddException(
                    CddErrorCode.InvalidSpecification,
                    "Arazzo 'info' object must specify non-empty 'title' and 'version'.");
            }

            if (document.SourceDescriptions.Count == 0)
            {
                throw new CddException(
                    CddErrorCode.InvalidSpecification,
                    "Arazzo 'sourceDescriptions' must contain at least one source description.");
            }

            var sourceNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (var sd in document.SourceDescriptions)
            {
                if (!SourceNameRegex.IsMatch(sd.Name))
                {
                    throw new CddException(
                        CddErrorCode.InvalidSpecification,
                        $"Invalid sourceDescription name '{sd.Name}'. Must match ^[A-Za-z0-9_-]+$.");
                }

                if (!sourceNames.Add(sd.Name))
                {
                    throw new CddException(
                        CddErrorCode.InvalidSpecification,
                        $"Duplicate sourceDescription name '{sd.Name}'.");
                }

                if (string.IsNullOrWhiteSpace(sd.Url))
                {
                    throw new CddException(
                        CddErrorCode.InvalidSpecification,
                        $"sourceDescription '{sd.Name}' must have a non-empty 'url'.");
                }

                if (sd.Type != null &&
                    sd.Type != "openapi" &&
                    sd.Type != "asyncapi" &&
                    sd.Type != "arazzo")
                {
                    throw new CddException(
                        CddErrorCode.InvalidSpecification,
                        $"Unsupported sourceDescription type '{sd.Type}'. Expected 'openapi', 'asyncapi', or 'arazzo'.");
                }
            }

            if (document.Workflows.Count == 0)
            {
                throw new CddException(
                    CddErrorCode.InvalidSpecification,
                    "Arazzo 'workflows' must contain at least one workflow.");
            }

            var workflowIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var wf in document.Workflows)
            {
                if (string.IsNullOrWhiteSpace(wf.WorkflowId))
                {
                    throw new CddException(
                        CddErrorCode.InvalidSpecification,
                        "Every Arazzo workflow must have a non-empty 'workflowId'.");
                }

                if (!workflowIds.Add(wf.WorkflowId))
                {
                    throw new CddException(
                        CddErrorCode.InvalidSpecification,
                        $"Duplicate workflowId '{wf.WorkflowId}'.");
                }

                if (wf.Steps.Count == 0)
                {
                    throw new CddException(
                        CddErrorCode.InvalidSpecification,
                        $"Workflow '{wf.WorkflowId}' must contain at least one step.");
                }

                var stepIds = new HashSet<string>(StringComparer.Ordinal);
                foreach (var step in wf.Steps)
                {
                    if (string.IsNullOrWhiteSpace(step.StepId))
                    {
                        throw new CddException(
                            CddErrorCode.InvalidSpecification,
                            $"Workflow '{wf.WorkflowId}' contains a step with an empty 'stepId'.");
                    }

                    if (!stepIds.Add(step.StepId))
                    {
                        throw new CddException(
                            CddErrorCode.InvalidSpecification,
                            $"Workflow '{wf.WorkflowId}' contains duplicate stepId '{step.StepId}'.");
                    }

                    int targetCount = 0;
                    if (!string.IsNullOrWhiteSpace(step.OperationId)) targetCount++;
                    if (!string.IsNullOrWhiteSpace(step.OperationPath)) targetCount++;
                    if (!string.IsNullOrWhiteSpace(step.ChannelPath)) targetCount++;
                    if (!string.IsNullOrWhiteSpace(step.WorkflowId)) targetCount++;

                    if (targetCount != 1)
                    {
                        throw new CddException(
                            CddErrorCode.InvalidSpecification,
                            $"Step '{step.StepId}' in workflow '{wf.WorkflowId}' must specify exactly one of 'operationId', 'operationPath', 'channelPath', or 'workflowId' (found {targetCount}).");
                    }

                    if (step.Action != null && step.Action != "send" && step.Action != "receive")
                    {
                        throw new CddException(
                            CddErrorCode.InvalidSpecification,
                            $"Step '{step.StepId}' has invalid action '{step.Action}'. Expected 'send' or 'receive'.");
                    }
                }

                ValidateStepDependencies(wf, stepIds);
            }

            ValidateWorkflowDependencies(document.Workflows, workflowIds);
        }

        /// <summary>
        /// Validates step-level <c>dependsOn</c> references within a workflow for unknown step IDs and circular dependency cycles.
        /// </summary>
        /// <param name="workflow">The workflow whose steps are being validated.</param>
        /// <param name="stepIds">The set of valid step IDs in the workflow.</param>
        private static void ValidateStepDependencies(ArazzoWorkflow workflow, HashSet<string> stepIds)
        {
            var graph = new Dictionary<string, IList<string>>(StringComparer.Ordinal);
            foreach (var step in workflow.Steps)
            {
                if (step.DependsOn != null && step.DependsOn.Count > 0)
                {
                    foreach (var dep in step.DependsOn)
                    {
                        if (!stepIds.Contains(dep))
                        {
                            throw new CddException(
                                CddErrorCode.InvalidSpecification,
                                $"Step '{step.StepId}' in workflow '{workflow.WorkflowId}' depends on unknown stepId '{dep}'.");
                        }
                    }

                    graph[step.StepId] = step.DependsOn;
                }
            }

            DetectCycle(graph, $"workflow '{workflow.WorkflowId}' steps");
        }

        /// <summary>
        /// Validates workflow-level <c>dependsOn</c> references for unknown workflow IDs and circular dependency cycles.
        /// </summary>
        /// <param name="workflows">The list of workflows in the document.</param>
        /// <param name="workflowIds">The set of valid workflow IDs in the document.</param>
        private static void ValidateWorkflowDependencies(IList<ArazzoWorkflow> workflows, HashSet<string> workflowIds)
        {
            var graph = new Dictionary<string, IList<string>>(StringComparer.Ordinal);
            foreach (var wf in workflows)
            {
                if (wf.DependsOn != null && wf.DependsOn.Count > 0)
                {
                    var localDeps = new List<string>();
                    foreach (var dep in wf.DependsOn)
                    {
                        if (!dep.StartsWith("$", StringComparison.Ordinal))
                        {
                            if (!workflowIds.Contains(dep))
                            {
                                throw new CddException(
                                    CddErrorCode.InvalidSpecification,
                                    $"Workflow '{wf.WorkflowId}' depends on unknown workflowId '{dep}'.");
                            }

                            localDeps.Add(dep);
                        }
                    }

                    if (localDeps.Count > 0)
                    {
                        graph[wf.WorkflowId] = localDeps;
                    }
                }
            }

            DetectCycle(graph, "workflows");
        }

        /// <summary>
        /// Detects whether a directed dependency graph contains any cycles using depth-first search.
        /// </summary>
        /// <param name="graph">Adjacency list mapping node IDs to dependency node IDs.</param>
        /// <param name="scopeLabel">Label describing the graph scope for error reporting.</param>
        private static void DetectCycle(IDictionary<string, IList<string>> graph, string scopeLabel)
        {
            var state = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var node in graph.Keys)
            {
                if (HasCycleDfs(node, graph, state))
                {
                    throw new CddException(
                        CddErrorCode.InvalidSpecification,
                        $"Circular dependency detected in {scopeLabel} involving '{node}'.");
                }
            }
        }

        /// <summary>
        /// Recursive depth-first cycle check helper.
        /// </summary>
        /// <param name="node">The current node ID.</param>
        /// <param name="graph">The dependency adjacency list.</param>
        /// <param name="state">Visit state map (0 = unvisited, 1 = visiting, 2 = visited).</param>
        /// <returns><c>true</c> if a cycle is found; otherwise <c>false</c>.</returns>
        private static bool HasCycleDfs(string node, IDictionary<string, IList<string>> graph, IDictionary<string, int> state)
        {
            if (state.TryGetValue(node, out var s))
            {
                return s == 1;
            }

            state[node] = 1;
            if (graph.TryGetValue(node, out var deps))
            {
                foreach (var dep in deps)
                {
                    if (HasCycleDfs(dep, graph, state))
                    {
                        return true;
                    }
                }
            }

            state[node] = 2;
            return false;
        }

        /// <summary>
        /// Converts a YAML string into a <see cref="JsonNode"/> hierarchy without external dependencies.
        /// </summary>
        /// <param name="yaml">The YAML document string.</param>
        /// <returns>The root <see cref="JsonNode"/>.</returns>
        internal static JsonNode ConvertYamlToJsonNode(string yaml)
        {
            var lf = ((char)10).ToString();
            var crlf = ((char)13).ToString() + lf;
            var rawLines = yaml.Replace(crlf, lf).Split((char)10);
            var lines = new List<(int Indent, string Content)>();
            foreach (var raw in rawLines)
            {
                var stripped = StripYamlComment(raw);
                if (string.IsNullOrWhiteSpace(stripped) || stripped.Trim() == "---" || stripped.Trim() == "...")
                {
                    continue;
                }

                int indent = 0;
                while (stripped[indent] == ' ')
                {
                    indent++;
                }

                lines.Add((indent, stripped.Substring(indent).TrimEnd()));
            }

            if (lines.Count == 0)
            {
                return new JsonObject();
            }

            int index = 0;
            return ParseYamlBlock(lines, ref index, lines[0].Indent);
        }

        /// <summary>
        /// Strips unquoted YAML comments (<c># ...</c>) from a single line.
        /// </summary>
        /// <param name="line">The raw YAML line.</param>
        /// <returns>The line without trailing comment.</returns>
        private static string StripYamlComment(string line)
        {
            int hashIdx = line.IndexOf(" #", StringComparison.Ordinal);
            if (hashIdx >= 0)
            {
                return line.Substring(0, hashIdx);
            }

            if (line.TrimStart().StartsWith("#", StringComparison.Ordinal))
            {
                return string.Empty;
            }

            return line;
        }

        /// <summary>
        /// Parses an indented YAML block (mapping or sequence) starting at <paramref name="index"/>.
        /// </summary>
        /// <param name="lines">The preprocessed list of indented YAML lines.</param>
        /// <param name="index">The current line index (updated as lines are consumed).</param>
        /// <param name="baseIndent">The indentation level of the current block.</param>
        /// <returns>A <see cref="JsonNode"/> representing the block.</returns>
        private static JsonNode ParseYamlBlock(List<(int Indent, string Content)> lines, ref int index, int baseIndent)
        {
            if (lines[index].Content.StartsWith("-", StringComparison.Ordinal))
            {
                var arr = new JsonArray();
                while (index < lines.Count && lines[index].Indent == baseIndent &&
                       lines[index].Content.StartsWith("-", StringComparison.Ordinal))
                {
                    var currentLine = lines[index];
                    var afterDash = currentLine.Content.Substring(1).Trim();
                    index++;

                    if (string.IsNullOrEmpty(afterDash))
                    {
                        arr.Add(ParseYamlBlock(lines, ref index, lines[index].Indent));
                    }
                    else if (TrySplitYamlKeyValue(afterDash, out var k, out var v))
                    {
                        var itemObj = new JsonObject();
                        itemObj[k] = ParseYamlScalar(v);

                        var childIndent = lines[index].Indent;
                        var restObj = ParseYamlObjectEntries(lines, ref index, childIndent);
                        foreach (var kvp in restObj)
                        {
                            itemObj[kvp.Key] = kvp.Value!.DeepClone();
                        }

                        arr.Add(itemObj);
                    }
                    else
                    {
                        arr.Add(ParseYamlScalar(afterDash));
                    }
                }

                return arr;
            }

            return ParseYamlObjectEntries(lines, ref index, baseIndent);
        }

        /// <summary>
        /// Parses YAML mapping key-value entries at <paramref name="baseIndent"/> into a <see cref="JsonObject"/>.
        /// </summary>
        /// <param name="lines">The preprocessed list of indented YAML lines.</param>
        /// <param name="index">The current line index.</param>
        /// <param name="baseIndent">The indentation level of the mapping.</param>
        /// <returns>The populated <see cref="JsonObject"/>.</returns>
        private static JsonObject ParseYamlObjectEntries(List<(int Indent, string Content)> lines, ref int index, int baseIndent)
        {
            var obj = new JsonObject();
            while (index < lines.Count && lines[index].Indent == baseIndent &&
                   !lines[index].Content.StartsWith("- ", StringComparison.Ordinal))
            {
                var line = lines[index].Content;
                index++;
                TrySplitYamlKeyValue(line, out var key, out var val);
                if (string.IsNullOrEmpty(val) || val == "|")
                {
                    if (val == "|")
                    {
                        var childIndent = lines[index].Indent;
                        var sb = new List<string>();
                        while (index < lines.Count && lines[index].Indent >= childIndent)
                        {
                            sb.Add(lines[index].Content);
                            index++;
                        }

                        obj[key] = JsonValue.Create(string.Join(((char)10).ToString(), sb));
                    }
                    else
                    {
                        obj[key] = ParseYamlBlock(lines, ref index, lines[index].Indent);
                    }
                }
                else
                {
                    obj[key] = ParseYamlScalar(val);
                }
            }

            return obj;
        }

        /// <summary>
        /// Attempts to split a YAML <c>key: value</c> line into unquoted key and raw value parts.
        /// </summary>
        /// <param name="text">The YAML line text.</param>
        /// <param name="key">When successful, receives the unquoted property key.</param>
        /// <param name="value">When successful, receives the trimmed value part.</param>
        /// <returns><c>true</c> if the line represents a YAML key-value mapping; otherwise <c>false</c>.</returns>
        private static bool TrySplitYamlKeyValue(string text, out string key, out string value)
        {
            bool inQuotes = false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '"') inQuotes = !inQuotes;
                else if (c == ':' && !inQuotes && (i + 1 == text.Length || text[i + 1] == ' '))
                {
                    key = UnquoteYaml(text.Substring(0, i).Trim());
                    value = text.Substring(i + 1).Trim();
                    return true;
                }
            }

            key = string.Empty;
            value = string.Empty;
            return false;
        }

        /// <summary>
        /// Parses an inline YAML scalar or inline JSON array/object into a <see cref="JsonNode"/>.
        /// </summary>
        /// <param name="raw">The raw scalar value text.</param>
        /// <returns>The parsed <see cref="JsonNode"/> or <c>null</c>.</returns>
        private static JsonNode? ParseYamlScalar(string raw)
        {
            if (raw == "null" || raw == "~")
            {
                return null;
            }

            if (raw.StartsWith("[", StringComparison.Ordinal) || raw.StartsWith("{", StringComparison.Ordinal))
            {
                return JsonNode.Parse(raw);
            }

            if (raw.StartsWith(@"""", StringComparison.Ordinal) || raw.StartsWith("'", StringComparison.Ordinal))
            {
                return JsonValue.Create(UnquoteYaml(raw));
            }

            if (string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase))
            {
                return JsonValue.Create(true);
            }

            if (string.Equals(raw, "false", StringComparison.OrdinalIgnoreCase))
            {
                return JsonValue.Create(false);
            }

            if (long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l))
            {
                return JsonValue.Create(l);
            }

            if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
            {
                return JsonValue.Create(d);
            }

            return JsonValue.Create(raw);
        }

        /// <summary>
        /// Removes surrounding single or double quotes from a YAML scalar string.
        /// </summary>
        /// <param name="val">The potentially quoted YAML string.</param>
        /// <returns>The unquoted string.</returns>
        private static string UnquoteYaml(string val)
        {
            if (val.Length >= 2)
            {
                var dq = ((char)34).ToString();
                var bs = ((char)92).ToString();
                if (val[0] == (char)34 && val[val.Length - 1] == (char)34)
                {
                    return val.Substring(1, val.Length - 2).Replace(bs + dq, dq).Replace(bs + bs, bs);
                }

                if (val[0] == (char)39 && val[val.Length - 1] == (char)39)
                {
                    return val.Substring(1, val.Length - 2).Replace("''", "'");
                }
            }

            return val;
        }
    }
}
