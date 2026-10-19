// <copyright file="emit.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using Cdd.OpenApi.Common;
using Cdd.OpenApi.Models.Arazzo;

namespace Cdd.OpenApi.Arazzo
{
    /// <summary>
    /// Serializes <see cref="ArazzoDocument"/> instances into formatted JSON or YAML strings.
    /// </summary>
    public static class ArazzoEmitter
    {
        /// <summary>
        /// Serializes an <see cref="ArazzoDocument"/> into an indented JSON string.
        /// </summary>
        /// <param name="document">The <see cref="ArazzoDocument"/> to serialize.</param>
        /// <returns>The serialized Arazzo 1.1.0 JSON string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="document"/> is null.</exception>
        public static string EmitJson(ArazzoDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return JsonSerializer.Serialize(document, ArazzoJsonContext.Default.ArazzoDocument);
        }

        /// <summary>
        /// Serializes an <see cref="ArazzoDocument"/> into a canonical YAML string.
        /// </summary>
        /// <param name="document">The <see cref="ArazzoDocument"/> to serialize.</param>
        /// <returns>The serialized Arazzo 1.1.0 YAML string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="document"/> is null.</exception>
        public static string EmitYaml(ArazzoDocument document)
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
