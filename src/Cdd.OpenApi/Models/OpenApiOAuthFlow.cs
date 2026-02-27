using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    public class OpenApiOAuthFlow
    {
        [JsonPropertyName("authorizationUrl")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? AuthorizationUrl { get; set; }

        [JsonPropertyName("tokenUrl")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TokenUrl { get; set; }

        [JsonPropertyName("refreshUrl")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? RefreshUrl { get; set; }

        [JsonPropertyName("scopes")]
        public IDictionary<string, string> Scopes { get; set; } = new Dictionary<string, string>();
    }
}
