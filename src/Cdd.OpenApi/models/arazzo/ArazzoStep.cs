// <copyright file="ArazzoStep.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models.Arazzo
{
    /// <summary>
    /// Describes a single workflow step that invokes an OpenAPI operation, an AsyncAPI channel, or a nested Arazzo workflow.
    /// </summary>
    public class ArazzoStep
    {
        /// <summary>
        /// Gets or sets the unique identifier for the step within its enclosing workflow.
        /// </summary>
        [JsonPropertyName("stepId")]
        public string StepId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a description of the step's purpose and behavior.
        /// </summary>
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the target operationId (or runtime expression resolving to an operationId) to invoke. Mutually exclusive with <see cref="OperationPath"/>, <see cref="ChannelPath"/>, and <see cref="WorkflowId"/>.
        /// </summary>
        [JsonPropertyName("operationId")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? OperationId { get; set; }

        /// <summary>
        /// Gets or sets the JSON Pointer path to a target OpenAPI operation (e.g., <c>{$sourceDescriptions.petstore.url}#/paths/~1pet~1{petId}/get</c>). Mutually exclusive with <see cref="OperationId"/>, <see cref="ChannelPath"/>, and <see cref="WorkflowId"/>.
        /// </summary>
        [JsonPropertyName("operationPath")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? OperationPath { get; set; }

        /// <summary>
        /// Gets or sets the target AsyncAPI channel path for event streaming steps in Arazzo 1.1.0. Mutually exclusive with <see cref="OperationId"/>, <see cref="OperationPath"/>, and <see cref="WorkflowId"/>.
        /// </summary>
        [JsonPropertyName("channelPath")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ChannelPath { get; set; }

        /// <summary>
        /// Gets or sets the target nested or sibling workflowId to execute. Mutually exclusive with <see cref="OperationId"/>, <see cref="OperationPath"/>, and <see cref="ChannelPath"/>.
        /// </summary>
        [JsonPropertyName("workflowId")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? WorkflowId { get; set; }

        /// <summary>
        /// Gets or sets the list of parameters (or reusable parameter references) bound to this step.
        /// </summary>
        [JsonPropertyName("parameters")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<ArazzoParameter>? Parameters { get; set; }

        /// <summary>
        /// Gets or sets the request body payload and dynamic replacements for this step.
        /// </summary>
        [JsonPropertyName("requestBody")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ArazzoRequestBody? RequestBody { get; set; }

        /// <summary>
        /// Gets or sets the list of assertions (criteria) that must all evaluate to true for the step to be considered successful.
        /// </summary>
        [JsonPropertyName("successCriteria")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<ArazzoCriterion>? SuccessCriteria { get; set; }

        /// <summary>
        /// Gets or sets the list of success actions to evaluate when <see cref="SuccessCriteria"/> pass.
        /// </summary>
        [JsonPropertyName("onSuccess")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<ArazzoSuccessAction>? OnSuccess { get; set; }

        /// <summary>
        /// Gets or sets the list of failure actions (such as retry, goto, or end) to evaluate when the step fails.
        /// </summary>
        [JsonPropertyName("onFailure")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<ArazzoFailureAction>? OnFailure { get; set; }

        /// <summary>
        /// Gets or sets a map of output variable names to runtime expressions or structured <see cref="ArazzoSelector"/> definitions extracted from the step's execution.
        /// </summary>
        [JsonPropertyName("outputs")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, ArazzoStepOutput>? Outputs { get; set; }

        /// <summary>
        /// Gets or sets the per-step execution timeout in milliseconds (Arazzo 1.1.0).
        /// </summary>
        [JsonPropertyName("timeout")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Timeout { get; set; }

        /// <summary>
        /// Gets or sets the correlation ID runtime expression or header reference used to correlate asynchronous messages (Arazzo 1.1.0).
        /// </summary>
        [JsonPropertyName("correlationId")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Gets or sets the asynchronous message flow intent (<c>"send"</c> or <c>"receive"</c>) when invoking an AsyncAPI channel (Arazzo 1.1.0).
        /// </summary>
        [JsonPropertyName("action")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Action { get; set; }

        /// <summary>
        /// Gets or sets the prerequisite step identifiers within the enclosing workflow that must complete before this step executes (Arazzo 1.1.0).
        /// </summary>
        [JsonPropertyName("dependsOn")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<string>? DependsOn { get; set; }

        /// <summary>
        /// Gets or sets specification extension fields (keys prefixed with <c>x-</c>).
        /// </summary>
        [JsonExtensionData]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, JsonElement>? Extensions { get; set; }
    }
}
