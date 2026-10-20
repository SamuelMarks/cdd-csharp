// <copyright file="IrSourceDescription.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json;

namespace Cdd.OpenApi.Ir
{
    /// <summary>
    /// Represents a source specification binding in the Intermediate Representation.
    /// </summary>
    public class IrSourceDescription
    {
        /// <summary>
        /// Gets or sets the unique name of the source description.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the URL or relative reference to the source specification.
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the specification protocol type (<c>"openapi"</c>, <c>"asyncapi"</c>, or <c>"arazzo"</c>).
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets specification extensions (<c>x-*</c>).
        /// </summary>
        public IDictionary<string, JsonElement>? Extensions { get; set; }
    }
}
