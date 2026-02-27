using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
/// <summary>Auto-generated documentation for OpenApiOAuthFlows.</summary>
    public class OpenApiOAuthFlows
    {
/// <summary>Auto-generated documentation for Implicit.</summary>
        [JsonPropertyName("implicit")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiOAuthFlow? Implicit { get; set; }

/// <summary>Auto-generated documentation for Password.</summary>
        [JsonPropertyName("password")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiOAuthFlow? Password { get; set; }

/// <summary>Auto-generated documentation for ClientCredentials.</summary>
        [JsonPropertyName("clientCredentials")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiOAuthFlow? ClientCredentials { get; set; }

/// <summary>Auto-generated documentation for AuthorizationCode.</summary>
        [JsonPropertyName("authorizationCode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiOAuthFlow? AuthorizationCode { get; set; }
    }
}
