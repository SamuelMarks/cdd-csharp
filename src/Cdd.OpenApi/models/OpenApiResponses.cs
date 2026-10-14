// <copyright file="OpenApiResponses.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    /// <summary>A container for the expected responses of an operation, mapping HTTP status codes to <see cref="OpenApiResponse"/> objects.</summary>
    public class OpenApiResponses : Dictionary<string, OpenApiResponse>
    {
        /// <summary>Gets or sets the documentation of responses other than the ones declared for specific HTTP response codes.</summary>
        [JsonPropertyName("default")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiResponse? Default { get; set; }
    }
}
