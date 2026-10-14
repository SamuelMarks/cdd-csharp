// <copyright file="OpenApiOAuthFlow.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    /// <summary>Configuration details for a supported OAuth Flow.</summary>
    public class OpenApiOAuthFlow
    {
        /// <summary>Gets or sets the authorization URL to be used for this flow.</summary>
        [JsonPropertyName("authorizationUrl")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? AuthorizationUrl { get; set; }

        /// <summary>Gets or sets the token URL to be used for this flow.</summary>
        [JsonPropertyName("tokenUrl")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TokenUrl { get; set; }

        /// <summary>Gets or sets the device authorization URL to be used for this flow.</summary>
        [JsonPropertyName("deviceAuthorizationUrl")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? DeviceAuthorizationUrl { get; set; }

        /// <summary>Gets or sets the URL to be used for obtaining refresh tokens.</summary>
        [JsonPropertyName("refreshUrl")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? RefreshUrl { get; set; }

        /// <summary>Gets or sets the available scopes for the OAuth2 security scheme, mapping scope names to short descriptions.</summary>
        [JsonPropertyName("scopes")]
        public IDictionary<string, string> Scopes { get; set; } = new Dictionary<string, string>();
    }
}
