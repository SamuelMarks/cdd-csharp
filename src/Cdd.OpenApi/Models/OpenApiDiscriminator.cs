using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    /// <summary>
    /// When request bodies or response payloads may be one of a number of different schemas, a discriminator object can be used to aid in serialization, deserialization, and validation.
    /// </summary>
    public class OpenApiDiscriminator
    {
        /// <summary>
        /// The name of the property in the payload that will hold the discriminator value.
        /// </summary>
        [JsonPropertyName("propertyName")]
        public string PropertyName { get; set; } = string.Empty;

        /// <summary>
        /// An object to hold mappings between payload values and schema names or references.
        /// </summary>
        [JsonPropertyName("mapping")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, string>? Mapping { get; set; }
    }
}