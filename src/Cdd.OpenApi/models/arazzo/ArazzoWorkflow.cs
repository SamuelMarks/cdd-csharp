// <copyright file="ArazzoWorkflow.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models.Arazzo
{
    /// <summary>
    /// Describes an individual Arazzo workflow consisting of inputs, prerequisites, sequential or DAG steps, actions, and outputs.
    /// </summary>
    public class ArazzoWorkflow
    {
        /// <summary>
        /// Gets or sets the unique identifier for the workflow within the Arazzo document.
        /// </summary>
        [JsonPropertyName("workflowId")]
        public string WorkflowId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a short summary of the purpose of the workflow.
        /// </summary>
        [JsonPropertyName("summary")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Summary { get; set; }

        /// <summary>
        /// Gets or sets an extended description of the workflow (supports CommonMark syntax).
        /// </summary>
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the JSON Schema 2020-12 object defining the input parameters expected by the workflow.
        /// </summary>
        [JsonPropertyName("inputs")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiSchema? Inputs { get; set; }

        /// <summary>
        /// Gets or sets the list of prerequisite workflow identifiers or runtime expressions that must complete before this workflow executes.
        /// </summary>
        [JsonPropertyName("dependsOn")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<string>? DependsOn { get; set; }

        /// <summary>
        /// Gets or sets the ordered list of steps to execute in this workflow.
        /// </summary>
        [JsonPropertyName("steps")]
        public IList<ArazzoStep> Steps { get; set; } = new List<ArazzoStep>();

        /// <summary>
        /// Gets or sets the workflow-level success actions applied across steps unless overridden at the step level.
        /// </summary>
        [JsonPropertyName("successActions")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<ArazzoSuccessAction>? SuccessActions { get; set; }

        /// <summary>
        /// Gets or sets the workflow-level failure actions applied across steps unless overridden at the step level.
        /// </summary>
        [JsonPropertyName("failureActions")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<ArazzoFailureAction>? FailureActions { get; set; }

        /// <summary>
        /// Gets or sets a map between output keys and runtime expressions producing the workflow's return values.
        /// </summary>
        [JsonPropertyName("outputs")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, string>? Outputs { get; set; }

        /// <summary>
        /// Gets or sets workflow-wide parameters injected into every step unless overridden by a step-level parameter.
        /// </summary>
        [JsonPropertyName("parameters")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<ArazzoParameter>? Parameters { get; set; }

        /// <summary>
        /// Gets or sets specification extension fields (keys prefixed with <c>x-</c>).
        /// </summary>
        [JsonExtensionData]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, JsonElement>? Extensions { get; set; }
    }
}
