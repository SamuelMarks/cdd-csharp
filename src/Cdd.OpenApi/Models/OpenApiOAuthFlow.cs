using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
/// <summary>Auto-generated documentation for OpenApiOAuthFlow.</summary>
    public class OpenApiOAuthFlow
    {
/// <summary>Auto-generated documentation for AuthorizationUrl.</summary>
        [JsonPropertyName("authorizationUrl")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? AuthorizationUrl { get; set; }

/// <summary>Auto-generated documentation for TokenUrl.</summary>
        [JsonPropertyName("tokenUrl")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TokenUrl { get; set; }

/// <summary>Auto-generated documentation for RefreshUrl.</summary>
        [JsonPropertyName("refreshUrl")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? RefreshUrl { get; set; }

/// <summary>Auto-generated documentation for Scopes.</summary>
        [JsonPropertyName("scopes")]
        public IDictionary<string, string> Scopes { get; set; } = new Dictionary<string, string>();
    }
}
