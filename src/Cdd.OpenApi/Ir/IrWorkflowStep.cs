// <copyright file="IrWorkflowStep.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json;
using Cdd.OpenApi.Models.Arazzo;

namespace Cdd.OpenApi.Ir
{
    /// <summary>
    /// Represents a single workflow step in the Intermediate Representation.
    /// </summary>
    public class IrWorkflowStep
    {
        /// <summary>
        /// Gets or sets the unique step identifier within its workflow.
        /// </summary>
        public string StepId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the human-readable description of the step.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the target operationId if invoking an OpenAPI operation by ID.
        /// </summary>
        public string? OperationId { get; set; }

        /// <summary>
        /// Gets or sets the target operationPath if invoking an OpenAPI operation by JSON Pointer path.
        /// </summary>
        public string? OperationPath { get; set; }

        /// <summary>
        /// Gets or sets the target channelPath if invoking an AsyncAPI channel.
        /// </summary>
        public string? ChannelPath { get; set; }

        /// <summary>
        /// Gets or sets the target workflowId if invoking a sub-workflow.
        /// </summary>
        public string? WorkflowId { get; set; }

        /// <summary>
        /// Gets or sets the list of step parameters.
        /// </summary>
        public IList<ArazzoParameter>? Parameters { get; set; }

        /// <summary>
        /// Gets or sets the step request body definition.
        /// </summary>
        public ArazzoRequestBody? RequestBody { get; set; }

        /// <summary>
        /// Gets or sets the step success criteria.
        /// </summary>
        public IList<ArazzoCriterion>? SuccessCriteria { get; set; }

        /// <summary>
        /// Gets or sets the step onSuccess action list.
        /// </summary>
        public IList<ArazzoSuccessAction>? OnSuccess { get; set; }

        /// <summary>
        /// Gets or sets the step onFailure action list.
        /// </summary>
        public IList<ArazzoFailureAction>? OnFailure { get; set; }

        /// <summary>
        /// Gets or sets the step output expressions map.
        /// </summary>
        public IDictionary<string, ArazzoStepOutput>? Outputs { get; set; }

        /// <summary>
        /// Gets or sets the per-step execution timeout in milliseconds (Arazzo 1.1.0).
        /// </summary>
        public int? Timeout { get; set; }

        /// <summary>
        /// Gets or sets the correlation ID runtime expression or header reference (Arazzo 1.1.0).
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Gets or sets the asynchronous message flow intent (<c>"send"</c> or <c>"receive"</c>).
        /// </summary>
        public string? Action { get; set; }

        /// <summary>
        /// Gets or sets the prerequisite step identifiers within the enclosing workflow.
        /// </summary>
        public IList<string>? DependsOn { get; set; }

        /// <summary>
        /// Gets or sets specification extensions (<c>x-*</c>).
        /// </summary>
        public IDictionary<string, JsonElement>? Extensions { get; set; }
    }
}
