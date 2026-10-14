// <copyright file="OpenApiServerVariable.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    /// <summary>An object representing a Server Variable for server URL template substitution.</summary>
    public class OpenApiServerVariable
    {
        /// <summary>Gets or sets an enumeration of string values to be used if the substitution options are from a limited set.</summary>
        [JsonPropertyName("enum")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<string>? Enum { get; set; }

        /// <summary>Gets or sets the default value to use for substitution, which SHALL be sent if an alternate value is not supplied.</summary>
        [JsonPropertyName("default")]
        public string Default { get; set; } = string.Empty;

        /// <summary>Gets or sets an optional description for the server variable.</summary>
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }
    }
}
