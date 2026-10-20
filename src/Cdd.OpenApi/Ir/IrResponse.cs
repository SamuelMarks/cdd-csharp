// <copyright file="IrResponse.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Cdd.OpenApi.Common;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Ir
{
    /// <summary>
    /// Represents an HTTP response in the Intermediate Representation.
    /// </summary>
    public class IrResponse
    {
        /// <summary>
        /// Gets or sets the HTTP status code key (e.g., "200", "default").
        /// </summary>
        public string StatusCode { get; set; } = "200";

        /// <summary>
        /// Gets or sets the response description.
        /// </summary>
        public string? Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the response payload type reference, if any.
        /// </summary>
        public IrTypeRef? ContentType { get; set; }

        /// <summary>
        /// Gets or sets the response headers map.
        /// </summary>
        public IDictionary<string, OpenApiHeader>? Headers { get; set; }

        /// <summary>
        /// Gets or sets the full media-type content map for the response.
        /// </summary>
        public IDictionary<string, OpenApiMediaType>? Content { get; set; }

        /// <summary>
        /// Gets or sets the response links map.
        /// </summary>
        public IDictionary<string, OpenApiLink>? Links { get; set; }

        /// <summary>
        /// Creates an <see cref="IrResponse"/> from a status code and an <see cref="OpenApiResponse"/>.
        /// </summary>
        /// <param name="statusCode">The HTTP status code key.</param>
        /// <param name="response">The source OpenAPI response.</param>
        /// <returns>The constructed <see cref="IrResponse"/>.</returns>
        public static IrResponse FromOpenApiResponse(string statusCode, OpenApiResponse response)
        {
            IrTypeRef? contentType = null;
            if (response.Content != null && response.Content.TryGetValue("application/json", out var mediaType) && mediaType.Schema != null)
            {
                var rType = mediaType.Schema.Type ?? OpenApiDataType.Object;
                contentType = new IrTypeRef
                {
                    TypeName = rType,
                    Format = mediaType.Schema.Format,
                    Ref = mediaType.Schema.Ref,
                    CSharpType = TypeMapper.MapSchemaToCSharp(mediaType.Schema, "object")
                };
            }

            return new IrResponse
            {
                StatusCode = statusCode,
                Description = response.Description,
                Headers = response.Headers,
                Content = response.Content,
                Links = response.Links,
                ContentType = contentType
            };
        }

        /// <summary>
        /// Converts this <see cref="IrResponse"/> back into an <see cref="OpenApiResponse"/>.
        /// </summary>
        /// <returns>The reconstructed <see cref="OpenApiResponse"/>.</returns>
        public OpenApiResponse ToOpenApiResponse()
        {
            return new OpenApiResponse
            {
                Description = Description!,
                Headers = Headers,
                Links = Links,
                Content = Content
            };
        }
    }
}
