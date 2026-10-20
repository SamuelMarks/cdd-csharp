// <copyright file="IrSecurityScheme.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Ir
{
    /// <summary>
    /// Represents a security scheme definition in the Intermediate Representation.
    /// </summary>
    public class IrSecurityScheme
    {
        /// <summary>
        /// Gets or sets the unique name of the security scheme in components.
        /// </summary>
        public string SchemeName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of the security scheme (e.g., "apiKey", "http", "oauth2", "openIdConnect", "mutualTLS").
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a description for security scheme.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the name of the header, query or cookie parameter to be used.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the location of the API key ("query", "header", or "cookie").
        /// </summary>
        public string? In { get; set; }

        /// <summary>
        /// Gets or sets the name of the HTTP Authorization scheme.
        /// </summary>
        public string? Scheme { get; set; }

        /// <summary>
        /// Gets or sets a hint to the client to identify how the bearer token is formatted.
        /// </summary>
        public string? BearerFormat { get; set; }

        /// <summary>
        /// Gets or sets the OAuth2 flow configurations.
        /// </summary>
        public OpenApiOAuthFlows? Flows { get; set; }

        /// <summary>
        /// Gets or sets the OpenID Connect discovery URL.
        /// </summary>
        public string? OpenIdConnectUrl { get; set; }

        /// <summary>
        /// Gets or sets the OAuth2 metadata URL.
        /// </summary>
        public string? Oauth2MetadataUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the security scheme is deprecated.
        /// </summary>
        public bool? Deprecated { get; set; }

        /// <summary>
        /// Gets or sets the legacy Swagger 2.0 flow type, if any.
        /// </summary>
        public string? Flow { get; set; }

        /// <summary>
        /// Gets or sets the legacy Swagger 2.0 authorization URL, if any.
        /// </summary>
        public string? AuthorizationUrl { get; set; }

        /// <summary>
        /// Gets or sets the legacy Swagger 2.0 token URL, if any.
        /// </summary>
        public string? TokenUrl { get; set; }

        /// <summary>
        /// Gets or sets the legacy Swagger 2.0 scopes dictionary, if any.
        /// </summary>
        public IDictionary<string, string>? Scopes { get; set; }

        /// <summary>
        /// Creates an <see cref="IrSecurityScheme"/> from a scheme name and an <see cref="OpenApiSecurityScheme"/>.
        /// </summary>
        /// <param name="schemeName">The key name of the security scheme.</param>
        /// <param name="scheme">The source OpenAPI security scheme.</param>
        /// <returns>The constructed <see cref="IrSecurityScheme"/>.</returns>
        public static IrSecurityScheme FromOpenApiSecurityScheme(string schemeName, OpenApiSecurityScheme scheme)
        {
            return new IrSecurityScheme
            {
                SchemeName = schemeName,
                Type = scheme.Type,
                Description = scheme.Description,
                Name = scheme.Name,
                In = scheme.In,
                Scheme = scheme.Scheme,
                BearerFormat = scheme.BearerFormat,
                Flows = scheme.Flows,
                OpenIdConnectUrl = scheme.OpenIdConnectUrl,
                Oauth2MetadataUrl = scheme.Oauth2MetadataUrl,
                Deprecated = scheme.Deprecated,
                Flow = scheme.Flow,
                AuthorizationUrl = scheme.AuthorizationUrl,
                TokenUrl = scheme.TokenUrl,
                Scopes = scheme.Scopes
            };
        }

        /// <summary>
        /// Converts this <see cref="IrSecurityScheme"/> back into an <see cref="OpenApiSecurityScheme"/>.
        /// </summary>
        /// <returns>The reconstructed <see cref="OpenApiSecurityScheme"/>.</returns>
        public OpenApiSecurityScheme ToOpenApiSecurityScheme()
        {
            return new OpenApiSecurityScheme
            {
                Type = Type,
                Description = Description,
                Name = Name,
                In = In,
                Scheme = Scheme,
                BearerFormat = BearerFormat,
                Flows = Flows,
                OpenIdConnectUrl = OpenIdConnectUrl,
                Oauth2MetadataUrl = Oauth2MetadataUrl,
                Deprecated = Deprecated,
                Flow = Flow,
                AuthorizationUrl = AuthorizationUrl,
                TokenUrl = TokenUrl,
                Scopes = Scopes
            };
        }
    }
}
