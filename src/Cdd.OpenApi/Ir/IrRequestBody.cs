// <copyright file="IrRequestBody.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Ir
{
    /// <summary>
    /// Represents a request body payload in the Intermediate Representation.
    /// </summary>
    public class IrRequestBody
    {
        /// <summary>
        /// Gets or sets the request body description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the request body is required.
        /// </summary>
        public bool? Required { get; set; }

        /// <summary>
        /// Gets or sets the media-type content map for the request body.
        /// </summary>
        public IDictionary<string, OpenApiMediaType> Content { get; set; } = new Dictionary<string, OpenApiMediaType>();

        /// <summary>
        /// Creates an <see cref="IrRequestBody"/> from an <see cref="OpenApiRequestBody"/>.
        /// </summary>
        /// <param name="body">The source OpenAPI request body.</param>
        /// <returns>The constructed <see cref="IrRequestBody"/>.</returns>
        public static IrRequestBody FromOpenApiRequestBody(OpenApiRequestBody body)
        {
            return new IrRequestBody
            {
                Description = body.Description,
                Required = body.Required,
                Content = body.Content
            };
        }

        /// <summary>
        /// Converts this <see cref="IrRequestBody"/> back into an <see cref="OpenApiRequestBody"/>.
        /// </summary>
        /// <returns>The reconstructed <see cref="OpenApiRequestBody"/>.</returns>
        public OpenApiRequestBody ToOpenApiRequestBody()
        {
            return new OpenApiRequestBody
            {
                Description = Description,
                Required = Required,
                Content = Content
            };
        }
    }
}
