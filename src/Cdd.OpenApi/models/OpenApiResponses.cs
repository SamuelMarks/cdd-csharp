using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
/// <summary>Auto-generated documentation for OpenApiResponses.</summary>
    public class OpenApiResponses : Dictionary<string, OpenApiResponse>
    {
/// <summary>Auto-generated documentation for Default.</summary>
        [JsonPropertyName("default")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiResponse? Default { get; set; }
    }
}
