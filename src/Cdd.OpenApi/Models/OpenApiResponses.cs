using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    public class OpenApiResponses : Dictionary<string, OpenApiResponse>
    {
        [JsonPropertyName("default")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiResponse? Default { get; set; }
    }
}
