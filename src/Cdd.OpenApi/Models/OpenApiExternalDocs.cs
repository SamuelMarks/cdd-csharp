using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    public class OpenApiExternalDocs
    {
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }
}
