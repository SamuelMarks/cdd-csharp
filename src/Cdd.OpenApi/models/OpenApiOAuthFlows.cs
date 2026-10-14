// <copyright file="OpenApiOAuthFlows.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    /// <summary>Allows configuration of the supported OAuth Flows.</summary>
    public class OpenApiOAuthFlows
    {
        /// <summary>Gets or sets configuration for the OAuth Implicit flow.</summary>
        [JsonPropertyName("implicit")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiOAuthFlow? Implicit { get; set; }

        /// <summary>Gets or sets configuration for the OAuth Resource Owner Password flow.</summary>
        [JsonPropertyName("password")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiOAuthFlow? Password { get; set; }

        /// <summary>Gets or sets configuration for the OAuth Client Credentials flow.</summary>
        [JsonPropertyName("clientCredentials")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiOAuthFlow? ClientCredentials { get; set; }

        /// <summary>Gets or sets configuration for the OAuth Authorization Code flow.</summary>
        [JsonPropertyName("authorizationCode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiOAuthFlow? AuthorizationCode { get; set; }

        /// <summary>Gets or sets configuration for the OAuth Device Authorization flow.</summary>
        [JsonPropertyName("deviceAuthorization")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiOAuthFlow? DeviceAuthorization { get; set; }
    }
}
