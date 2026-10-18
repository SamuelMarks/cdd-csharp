// <copyright file="ArazzoFailureAction.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models.Arazzo
{
    /// <summary>
    /// Represents a failure recovery action (<c>end</c>, <c>retry</c>, or <c>goto</c>) or a Reusable Object referencing <c>$components.failureActions.&lt;name&gt;</c>.
    /// </summary>
    public class ArazzoFailureAction
    {
        /// <summary>
        /// Gets or sets the case-sensitive name of the failure action.
        /// </summary>
        [JsonPropertyName("name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the action flow type (<c>"end"</c>, <c>"retry"</c>, or <c>"goto"</c>).
        /// </summary>
        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the target workflowId to invoke on failure when <see cref="Type"/> is <c>"goto"</c> or <c>"retry"</c>.
        /// </summary>
        [JsonPropertyName("workflowId")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? WorkflowId { get; set; }

        /// <summary>
        /// Gets or sets the target stepId to jump to or retry when <see cref="Type"/> is <c>"goto"</c> or <c>"retry"</c>.
        /// </summary>
        [JsonPropertyName("stepId")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? StepId { get; set; }

        /// <summary>
        /// Gets or sets parameters passed to the recovery target step or workflow.
        /// </summary>
        [JsonPropertyName("parameters")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<ArazzoParameter>? Parameters { get; set; }

        /// <summary>
        /// Gets or sets the non-negative delay in seconds to wait before retrying the step when <see cref="Type"/> is <c>"retry"</c>.
        /// </summary>
        [JsonPropertyName("retryAfter")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? RetryAfter { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of retry attempts allowed when <see cref="Type"/> is <c>"retry"</c>.
        /// </summary>
        [JsonPropertyName("retryLimit")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? RetryLimit { get; set; }

        /// <summary>
        /// Gets or sets guard criteria that must evaluate to true for this failure action to trigger.
        /// </summary>
        [JsonPropertyName("criteria")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<ArazzoCriterion>? Criteria { get; set; }

        /// <summary>
        /// Gets or sets a runtime expression reference to a reusable failure action in <c>$components.failureActions</c>.
        /// </summary>
        [JsonPropertyName("reference")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Reference { get; set; }

        /// <summary>
        /// Gets or sets an optional override value when referencing a reusable component.
        /// </summary>
        [JsonPropertyName("value")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiAny? Value { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is a Reusable Object reference.
        /// </summary>
        [JsonIgnore]
        public bool IsReusable => !string.IsNullOrEmpty(Reference);

        /// <summary>
        /// Gets or sets specification extension fields (keys prefixed with <c>x-</c>).
        /// </summary>
        [JsonExtensionData]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, JsonElement>? Extensions { get; set; }
    }
}
