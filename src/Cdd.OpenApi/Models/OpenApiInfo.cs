using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
/// <summary>Auto-generated documentation for OpenApiInfo.</summary>
    public class OpenApiInfo
    {
/// <summary>Auto-generated documentation for Title.</summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

/// <summary>Auto-generated documentation for Summary.</summary>
        [JsonPropertyName("summary")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Summary { get; set; }

/// <summary>Auto-generated documentation for Description.</summary>
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

/// <summary>Auto-generated documentation for TermsOfService.</summary>
        [JsonPropertyName("termsOfService")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TermsOfService { get; set; }

/// <summary>Auto-generated documentation for Contact.</summary>
        [JsonPropertyName("contact")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiContact? Contact { get; set; }

/// <summary>Auto-generated documentation for License.</summary>
        [JsonPropertyName("license")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiLicense? License { get; set; }

/// <summary>Auto-generated documentation for Version.</summary>
        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;
    }
}
