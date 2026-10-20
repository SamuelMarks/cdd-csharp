// <copyright file="IrEndpoint.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Ir
{
    /// <summary>
    /// Represents a symmetric HTTP endpoint in the Intermediate Representation.
    /// </summary>
    public class IrEndpoint
    {
        /// <summary>
        /// Gets or sets the route path template (e.g., "/pets/{id}").
        /// </summary>
        public string Path { get; set; } = "/";

        /// <summary>
        /// Gets or sets the HTTP verb (e.g., "Get", "Post").
        /// </summary>
        public string HttpMethod { get; set; } = "Get";

        /// <summary>
        /// Gets or sets the unique operation identifier.
        /// </summary>
        public string OperationId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the source operation explicitly specified an <c>operationId</c>.
        /// </summary>
        public bool HasExplicitOperationId { get; set; } = true;

        /// <summary>
        /// Gets or sets the operation summary.
        /// </summary>
        public string? Summary { get; set; }

        /// <summary>
        /// Gets or sets the operation description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the operation tags.
        /// </summary>
        public IList<string>? Tags { get; set; }

        /// <summary>
        /// Gets or sets the operation external documentation.
        /// </summary>
        public OpenApiExternalDocs? ExternalDocs { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the operation is deprecated.
        /// </summary>
        public bool? Deprecated { get; set; }

        /// <summary>
        /// Gets or sets the operation request body.
        /// </summary>
        public OpenApiRequestBody? RequestBody { get; set; }

        /// <summary>
        /// Gets or sets the strongly-typed IR request body representation.
        /// </summary>
        public IrRequestBody? IrBody { get; set; }

        /// <summary>
        /// Gets or sets the operation callbacks.
        /// </summary>
        public IDictionary<string, OpenApiCallback>? Callbacks { get; set; }

        /// <summary>
        /// Gets or sets the operation-specific security requirements.
        /// </summary>
        public IList<IDictionary<string, IList<string>>>? Security { get; set; }

        /// <summary>
        /// Gets or sets the operation-specific server overrides.
        /// </summary>
        public IList<OpenApiServer>? Servers { get; set; }

        /// <summary>
        /// Gets or sets the list of endpoint parameters.
        /// </summary>
        public List<IrParameter> Parameters { get; set; } = new List<IrParameter>();

        /// <summary>
        /// Gets or sets the list of endpoint responses.
        /// </summary>
        public List<IrResponse> Responses { get; set; } = new List<IrResponse>();

        /// <summary>
        /// Creates an <see cref="IrEndpoint"/> from a path, HTTP method, and <see cref="OpenApiOperation"/>.
        /// </summary>
        /// <param name="path">The route path template.</param>
        /// <param name="httpMethod">The HTTP verb in PascalCase or uppercase.</param>
        /// <param name="op">The source OpenAPI operation.</param>
        /// <returns>The constructed <see cref="IrEndpoint"/>.</returns>
        public static IrEndpoint FromOpenApiOperation(string path, string httpMethod, OpenApiOperation op)
        {
            var endpoint = new IrEndpoint
            {
                Path = path,
                HttpMethod = httpMethod,
                HasExplicitOperationId = op.OperationId != null,
                OperationId = op.OperationId ?? $"{httpMethod}{path.Replace("/", "").Replace("{", "").Replace("}", "")}",
                Summary = op.Summary,
                Description = op.Description,
                Tags = op.Tags,
                ExternalDocs = op.ExternalDocs,
                Deprecated = op.Deprecated,
                RequestBody = op.RequestBody,
                IrBody = op.RequestBody != null ? IrRequestBody.FromOpenApiRequestBody(op.RequestBody) : null,
                Callbacks = op.Callbacks,
                Security = op.Security,
                Servers = op.Servers
            };

            if (op.Parameters != null)
            {
                foreach (var p in op.Parameters)
                {
                    endpoint.Parameters.Add(IrParameter.FromOpenApiParameter(p));
                }
            }

            if (op.Responses != null)
            {
                foreach (var rKvp in op.Responses)
                {
                    endpoint.Responses.Add(IrResponse.FromOpenApiResponse(rKvp.Key, rKvp.Value));
                }
            }

            return endpoint;
        }

        /// <summary>
        /// Converts this <see cref="IrEndpoint"/> back into an <see cref="OpenApiOperation"/>.
        /// </summary>
        /// <returns>The reconstructed <see cref="OpenApiOperation"/>.</returns>
        public OpenApiOperation ToOpenApiOperation()
        {
            var op = new OpenApiOperation
            {
                OperationId = HasExplicitOperationId ? OperationId : null,
                Summary = Summary,
                Description = Description,
                Tags = Tags,
                ExternalDocs = ExternalDocs,
                Deprecated = Deprecated,
                RequestBody = RequestBody,
                Callbacks = Callbacks,
                Security = Security,
                Servers = Servers
            };

            if (Parameters.Count > 0)
            {
                op.Parameters = Parameters.Select(p => p.ToOpenApiParameter()).ToList();
            }

            if (Responses.Count > 0)
            {
                var responses = new OpenApiResponses();
                foreach (var r in Responses)
                {
                    responses[r.StatusCode] = r.ToOpenApiResponse();
                }

                op.Responses = responses;
            }

            return op;
        }
    }
}
