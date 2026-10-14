// <copyright file="OpenApiSecurityScheme.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    /// <summary>Defines a security scheme that can be used by the operations.</summary>
    public class OpenApiSecurityScheme
    {
        /// <summary>Gets or sets the type of the security scheme (<c>apiKey</c>, <c>http</c>, <c>mutualTLS</c>, <c>oauth2</c>, or <c>openIdConnect</c>).</summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>Gets or sets a short description for the security scheme.</summary>
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

        /// <summary>Gets or sets the name of the header, query, or cookie parameter to be used when <see cref="Type"/> is <c>apiKey</c>.</summary>
        [JsonPropertyName("name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Name { get; set; }

        /// <summary>Gets or sets the location of the API key (<c>query</c>, <c>header</c>, or <c>cookie</c>).</summary>
        [JsonPropertyName("in")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? In { get; set; }

        /// <summary>Gets or sets the name of the HTTP Authorization scheme to be used in the Authorization header.</summary>
        [JsonPropertyName("scheme")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Scheme { get; set; }

        /// <summary>Gets or sets a hint to the client to identify how the bearer token is formatted.</summary>
        [JsonPropertyName("bearerFormat")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? BearerFormat { get; set; }

        /// <summary>Gets or sets an object containing configuration information for the flow types supported.</summary>
        [JsonPropertyName("flows")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiOAuthFlows? Flows { get; set; }

        /// <summary>Gets or sets the OpenId Connect URL to discover OAuth2 configuration values.</summary>
        [JsonPropertyName("openIdConnectUrl")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? OpenIdConnectUrl { get; set; }

        /// <summary>Gets or sets the OAuth2 Authorization Server Metadata URL (OpenAPI 3.2.0).</summary>
        [JsonPropertyName("oauth2MetadataUrl")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Oauth2MetadataUrl { get; set; }

        /// <summary>Gets or sets a value indicating whether the security scheme is deprecated.</summary>
        [JsonPropertyName("deprecated")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Deprecated { get; set; }

        /// <summary>Swagger 2.0 flow</summary>
        [JsonPropertyName("flow")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Flow { get; set; }

        /// <summary>Swagger 2.0 authorizationUrl</summary>
        [JsonPropertyName("authorizationUrl")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? AuthorizationUrl { get; set; }

        /// <summary>Swagger 2.0 tokenUrl</summary>
        [JsonPropertyName("tokenUrl")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TokenUrl { get; set; }

        /// <summary>Swagger 2.0 scopes</summary>
        [JsonPropertyName("scopes")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public System.Collections.Generic.IDictionary<string, string>? Scopes { get; set; }
    }
}
