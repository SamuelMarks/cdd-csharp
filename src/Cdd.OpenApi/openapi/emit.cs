// <copyright file="emit.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.Text.Json;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Emit
{
    /// <summary>Serializes <see cref="OpenApiDocument"/> instances into formatted OpenAPI JSON strings.</summary>
    public class OpenApiEmitter
    {
        /// <summary>Initializes a new instance of the <see cref="OpenApiEmitter"/> class.</summary>
        public OpenApiEmitter()
        {
        }

        /// <summary>
        /// Serializes the specified <see cref="OpenApiDocument"/> to an indented JSON string.
        /// </summary>
        /// <param name="document">The OpenAPI document to serialize.</param>
        /// <returns>The serialized OpenAPI JSON string.</returns>
        public string EmitJson(OpenApiDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return JsonSerializer.Serialize(document, Cdd.OpenApi.Parse.OpenApiJsonContext.Default.OpenApiDocument);
        }
    }
}
