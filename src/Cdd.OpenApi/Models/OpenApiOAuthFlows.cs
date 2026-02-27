using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    public class OpenApiOAuthFlows
    {
        [JsonPropertyName("implicit")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiOAuthFlow? Implicit { get; set; }

        [JsonPropertyName("password")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiOAuthFlow? Password { get; set; }

        [JsonPropertyName("clientCredentials")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiOAuthFlow? ClientCredentials { get; set; }

        [JsonPropertyName("authorizationCode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiOAuthFlow? AuthorizationCode { get; set; }
    }
}
