// <copyright file="OpenApiContact.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    /// <summary>Contact information for the exposed API.</summary>
    public class OpenApiContact
    {
        /// <summary>Gets or sets the identifying name of the contact person or organization.</summary>
        [JsonPropertyName("name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Name { get; set; }

        /// <summary>Gets or sets the URL pointing to the contact information.</summary>
        [JsonPropertyName("url")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Url { get; set; }

        /// <summary>Gets or sets the email address of the contact person or organization.</summary>
        [JsonPropertyName("email")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Email { get; set; }
    }
}
