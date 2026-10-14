// <copyright file="OpenApiInfo.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    /// <summary>Provides metadata about the API.</summary>
    public class OpenApiInfo
    {
        /// <summary>Gets or sets the title of the API.</summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>Gets or sets a short summary of the API.</summary>
        [JsonPropertyName("summary")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Summary { get; set; }

        /// <summary>Gets or sets a description of the API.</summary>
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

        /// <summary>Gets or sets a URL to the Terms of Service for the API.</summary>
        [JsonPropertyName("termsOfService")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TermsOfService { get; set; }

        /// <summary>Gets or sets the contact information for the exposed API.</summary>
        [JsonPropertyName("contact")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiContact? Contact { get; set; }

        /// <summary>Gets or sets the license information for the exposed API.</summary>
        [JsonPropertyName("license")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiLicense? License { get; set; }

        /// <summary>Gets or sets the version of the OpenAPI document.</summary>
        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;
    }
}
