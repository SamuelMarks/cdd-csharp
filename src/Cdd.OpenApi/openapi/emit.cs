// <copyright file="emit.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using Cdd.OpenApi.Common;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Emit
{
    /// <summary>Serializes <see cref="OpenApiDocument"/> instances into formatted OpenAPI JSON or YAML strings.</summary>
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

        /// <summary>
        /// Serializes the specified <see cref="OpenApiDocument"/> to a canonical YAML string.
        /// </summary>
        /// <param name="document">The OpenAPI document to serialize.</param>
        /// <returns>The serialized OpenAPI YAML string.</returns>
        public string EmitYaml(OpenApiDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            var json = EmitJson(document);
            var rootNode = JsonNode.Parse(json);
            return YamlConverter.ConvertJsonNodeToYaml(rootNode);
        }
    }
}
