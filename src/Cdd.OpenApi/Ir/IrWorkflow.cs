// <copyright file="IrWorkflow.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json;
using Cdd.OpenApi.Models;
using Cdd.OpenApi.Models.Arazzo;

namespace Cdd.OpenApi.Ir
{
    /// <summary>
    /// Represents an executable Arazzo workflow in the Intermediate Representation.
    /// </summary>
    public class IrWorkflow
    {
        /// <summary>
        /// Gets or sets the unique workflow identifier.
        /// </summary>
        public string WorkflowId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the workflow summary.
        /// </summary>
        public string? Summary { get; set; }

        /// <summary>
        /// Gets or sets the workflow description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the JSON Schema describing the workflow inputs.
        /// </summary>
        public OpenApiSchema? Inputs { get; set; }

        /// <summary>
        /// Gets or sets the prerequisite workflow identifiers.
        /// </summary>
        public List<string> DependsOn { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the ordered list of steps in the workflow.
        /// </summary>
        public List<IrWorkflowStep> Steps { get; set; } = new List<IrWorkflowStep>();

        /// <summary>
        /// Gets or sets workflow-level success actions.
        /// </summary>
        public IList<ArazzoSuccessAction>? SuccessActions { get; set; }

        /// <summary>
        /// Gets or sets workflow-level failure actions.
        /// </summary>
        public IList<ArazzoFailureAction>? FailureActions { get; set; }

        /// <summary>
        /// Gets or sets the workflow output expressions map.
        /// </summary>
        public IDictionary<string, string>? Outputs { get; set; }

        /// <summary>
        /// Gets or sets workflow-level default parameters.
        /// </summary>
        public IList<ArazzoParameter>? Parameters { get; set; }

        /// <summary>
        /// Gets or sets specification extensions (<c>x-*</c>).
        /// </summary>
        public IDictionary<string, JsonElement>? Extensions { get; set; }
    }
}
