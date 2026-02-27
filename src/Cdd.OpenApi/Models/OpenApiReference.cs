using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    public class OpenApiReference
    {
        [JsonPropertyName("$ref")]
        public string Ref { get; set; } = string.Empty;

        [JsonPropertyName("summary")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Summary { get; set; }

        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }
    }
}
