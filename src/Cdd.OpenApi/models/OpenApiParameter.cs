using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
/// <summary>Auto-generated documentation for OpenApiParameter.</summary>
    public class OpenApiParameter
    {
/// <summary>Auto-generated documentation for Name.</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

/// <summary>Auto-generated documentation for In.</summary>
        [JsonPropertyName("in")]
        public string In { get; set; } = string.Empty;

/// <summary>Auto-generated documentation for Description.</summary>
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

/// <summary>Auto-generated documentation for Required.</summary>
        [JsonPropertyName("required")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Required { get; set; }

/// <summary>Auto-generated documentation for Deprecated.</summary>
        [JsonPropertyName("deprecated")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Deprecated { get; set; }

/// <summary>Auto-generated documentation for AllowEmptyValue.</summary>
        [JsonPropertyName("allowEmptyValue")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? AllowEmptyValue { get; set; }

/// <summary>Auto-generated documentation for Style.</summary>
        [JsonPropertyName("style")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Style { get; set; }

/// <summary>Auto-generated documentation for Explode.</summary>
        [JsonPropertyName("explode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Explode { get; set; }

/// <summary>Auto-generated documentation for AllowReserved.</summary>
        [JsonPropertyName("allowReserved")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? AllowReserved { get; set; }

/// <summary>Auto-generated documentation for Schema.</summary>
        [JsonPropertyName("schema")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiSchema? Schema { get; set; }

        /// <summary>Swagger 2.0 type</summary>
        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Type { get; set; }

        /// <summary>Swagger 2.0 format</summary>
        [JsonPropertyName("format")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Format { get; set; }

        /// <summary>Swagger 2.0 items</summary>
        [JsonPropertyName("items")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiSchema? Items { get; set; }

        /// <summary>Swagger 2.0 collectionFormat</summary>
        [JsonPropertyName("collectionFormat")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? CollectionFormat { get; set; }

        /// <summary>Swagger 2.0 default</summary>
        [JsonPropertyName("default")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Default { get; set; }

        /// <summary>Swagger 2.0 maximum</summary>
        [JsonPropertyName("maximum")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? Maximum { get; set; }

        /// <summary>Swagger 2.0 exclusiveMaximum</summary>
        [JsonPropertyName("exclusiveMaximum")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? ExclusiveMaximum { get; set; }

        /// <summary>Swagger 2.0 minimum</summary>
        [JsonPropertyName("minimum")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? Minimum { get; set; }

        /// <summary>Swagger 2.0 exclusiveMinimum</summary>
        [JsonPropertyName("exclusiveMinimum")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? ExclusiveMinimum { get; set; }

        /// <summary>Swagger 2.0 maxLength</summary>
        [JsonPropertyName("maxLength")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MaxLength { get; set; }

        /// <summary>Swagger 2.0 minLength</summary>
        [JsonPropertyName("minLength")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MinLength { get; set; }

        /// <summary>Swagger 2.0 pattern</summary>
        [JsonPropertyName("pattern")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Pattern { get; set; }

        /// <summary>Swagger 2.0 maxItems</summary>
        [JsonPropertyName("maxItems")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MaxItems { get; set; }

        /// <summary>Swagger 2.0 minItems</summary>
        [JsonPropertyName("minItems")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MinItems { get; set; }

        /// <summary>Swagger 2.0 uniqueItems</summary>
        [JsonPropertyName("uniqueItems")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? UniqueItems { get; set; }

        /// <summary>Swagger 2.0 enum</summary>
        [JsonPropertyName("enum")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<object>? Enum { get; set; }

        /// <summary>Swagger 2.0 multipleOf</summary>
        [JsonPropertyName("multipleOf")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? MultipleOf { get; set; }

/// <summary>Auto-generated documentation for Content.</summary>
        [JsonPropertyName("content")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiMediaType>? Content { get; set; }

/// <summary>Auto-generated documentation for Example.</summary>
        [JsonPropertyName("example")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Example { get; set; }

/// <summary>Auto-generated documentation for Examples.</summary>
        [JsonPropertyName("examples")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiExample>? Examples { get; set; }
    }
}
