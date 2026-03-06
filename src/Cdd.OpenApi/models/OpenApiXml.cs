using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    /// <summary>
    /// A metadata object that allows for more fine-tuned XML model definitions.
    /// </summary>
    public class OpenApiXml
    {
        /// <summary>
        /// Replaces the name of the element/attribute used for the described schema property.
        /// </summary>
        [JsonPropertyName("name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Name { get; set; }

        /// <summary>
        /// The URI of the namespace definition.
        /// </summary>
        [JsonPropertyName("namespace")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Namespace { get; set; }

        /// <summary>
        /// The prefix to be used for the name.
        /// </summary>
        [JsonPropertyName("prefix")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Prefix { get; set; }

        /// <summary>
        /// Declares whether the property definition translates to an attribute instead of an element.
        /// </summary>
        [JsonPropertyName("attribute")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Attribute { get; set; }

        /// <summary>
        /// May be used only for an array definition. Signifies whether the array is wrapped.
        /// </summary>
        [JsonPropertyName("wrapped")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Wrapped { get; set; }

        /// <summary>
        /// OpenAPI 3.2.0: The type of XML node.
        /// </summary>
        [JsonPropertyName("nodeType")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? NodeType { get; set; }
    }
}
