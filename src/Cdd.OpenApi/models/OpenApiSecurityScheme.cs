using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
/// <summary>Auto-generated documentation for OpenApiSecurityScheme.</summary>
    public class OpenApiSecurityScheme
    {
/// <summary>Auto-generated documentation for Type.</summary>
        [JsonPropertyName("type")]
/// <summary>Auto-generated documentation for Type.</summary>
        public string Type { get; set; } = string.Empty;

/// <summary>Auto-generated documentation for Description.</summary>
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

/// <summary>Auto-generated documentation for Name.</summary>
        [JsonPropertyName("name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Name { get; set; }

/// <summary>Auto-generated documentation for In.</summary>
        [JsonPropertyName("in")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? In { get; set; }

/// <summary>Auto-generated documentation for Scheme.</summary>
        [JsonPropertyName("scheme")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Scheme { get; set; }

/// <summary>Auto-generated documentation for BearerFormat.</summary>
        [JsonPropertyName("bearerFormat")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? BearerFormat { get; set; }

/// <summary>Auto-generated documentation for Flows.</summary>
        [JsonPropertyName("flows")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiOAuthFlows? Flows { get; set; }

/// <summary>Auto-generated documentation for OpenIdConnectUrl.</summary>
        [JsonPropertyName("openIdConnectUrl")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? OpenIdConnectUrl { get; set; }

/// <summary>Auto-generated documentation for Oauth2MetadataUrl.</summary>
        [JsonPropertyName("oauth2MetadataUrl")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Oauth2MetadataUrl { get; set; }

/// <summary>Auto-generated documentation for Deprecated.</summary>
        [JsonPropertyName("deprecated")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Deprecated { get; set; }
    }
}
