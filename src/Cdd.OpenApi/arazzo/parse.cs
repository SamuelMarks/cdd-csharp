// <copyright file="parse.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Cdd.OpenApi.Common;
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

            ValidateSourceDescriptions(document.SourceDescriptions);
            ValidateWorkflows(document.Workflows);
        }

        /// <summary>
        /// Validates the <c>sourceDescriptions</c> list of an <see cref="ArazzoDocument"/>.
        /// </summary>
        /// <param name="sourceDescriptions">The source descriptions collection to validate.</param>
        private static void ValidateSourceDescriptions(IList<ArazzoSourceDescription> sourceDescriptions)
        {
            if (sourceDescriptions.Count == 0)
            {
                throw new CddException(
                    CddErrorCode.InvalidSpecification,
                    "Arazzo 'sourceDescriptions' must contain at least one source description.");
            }

            var sourceNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (var sd in sourceDescriptions)
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
        }

        /// <summary>
        /// Validates the <c>workflows</c> collection of an <see cref="ArazzoDocument"/>.
        /// </summary>
        /// <param name="workflows">The workflows list to validate.</param>
        private static void ValidateWorkflows(IList<ArazzoWorkflow> workflows)
        {
            if (workflows.Count == 0)
            {
                throw new CddException(
                    CddErrorCode.InvalidSpecification,
                    "Arazzo 'workflows' must contain at least one workflow.");
            }

            var workflowIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var wf in workflows)
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
                    ValidateStep(wf.WorkflowId, step, stepIds);
                }

                ValidateStepDependencies(wf, stepIds);
            }

            ValidateWorkflowDependencies(workflows, workflowIds);
        }

        /// <summary>
        /// Validates a single <see cref="ArazzoStep"/> within a workflow.
        /// </summary>
        /// <param name="workflowId">The enclosing workflow identifier.</param>
        /// <param name="step">The step to validate.</param>
        /// <param name="stepIds">The set of seen step identifiers in the workflow.</param>
        private static void ValidateStep(string workflowId, ArazzoStep step, HashSet<string> stepIds)
        {
            if (string.IsNullOrWhiteSpace(step.StepId))
            {
                throw new CddException(
                    CddErrorCode.InvalidSpecification,
                    $"Workflow '{workflowId}' contains a step with an empty 'stepId'.");
            }

            if (!stepIds.Add(step.StepId))
            {
                throw new CddException(
                    CddErrorCode.InvalidSpecification,
                    $"Workflow '{workflowId}' contains duplicate stepId '{step.StepId}'.");
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
                    $"Step '{step.StepId}' in workflow '{workflowId}' must specify exactly one of 'operationId', 'operationPath', 'channelPath', or 'workflowId' (found {targetCount}).");
            }

            if (step.Action != null && step.Action != "send" && step.Action != "receive")
            {
                throw new CddException(
                    CddErrorCode.InvalidSpecification,
                    $"Step '{step.StepId}' has invalid action '{step.Action}'. Expected 'send' or 'receive'.");
            }
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
            return YamlConverter.ConvertYamlToJsonNode(yaml);
        }
    }
}
