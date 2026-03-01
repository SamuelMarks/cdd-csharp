using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    /// <summary>Auto-generated documentation for OpenApiSchema.</summary>
    public class OpenApiSchema
    {
        /// <summary>Auto-generated documentation for Ref.</summary>
        [JsonPropertyName("$ref")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Ref { get; set; }

        /// <summary>Auto-generated documentation for Type.</summary>
        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Type { get; set; }

        /// <summary>Auto-generated documentation for Format.</summary>
        [JsonPropertyName("format")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Format { get; set; }

        /// <summary>Auto-generated documentation for Items.</summary>
        [JsonPropertyName("items")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiSchema? Items { get; set; }

        /// <summary>Auto-generated documentation for Properties.</summary>
        [JsonPropertyName("properties")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiSchema>? Properties { get; set; }

        /// <summary>Auto-generated documentation for Required.</summary>
        [JsonPropertyName("required")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<string>? Required { get; set; }

        /// <summary>Auto-generated documentation for Description.</summary>
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

        /// <summary>Auto-generated documentation for Minimum.</summary>
        [JsonPropertyName("minimum")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? Minimum { get; set; }

        /// <summary>Auto-generated documentation for Maximum.</summary>
        [JsonPropertyName("maximum")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? Maximum { get; set; }

        /// <summary>Auto-generated documentation for ExclusiveMinimum.</summary>
        [JsonPropertyName("exclusiveMinimum")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? ExclusiveMinimum { get; set; }

        /// <summary>Auto-generated documentation for ExclusiveMaximum.</summary>
        [JsonPropertyName("exclusiveMaximum")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? ExclusiveMaximum { get; set; }

        /// <summary>Auto-generated documentation for AllOf.</summary>
        [JsonPropertyName("allOf")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<OpenApiSchema>? AllOf { get; set; }

        /// <summary>Auto-generated documentation for AnyOf.</summary>
        [JsonPropertyName("anyOf")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<OpenApiSchema>? AnyOf { get; set; }

        /// <summary>Auto-generated documentation for OneOf.</summary>
        [JsonPropertyName("oneOf")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<OpenApiSchema>? OneOf { get; set; }

        /// <summary>Auto-generated documentation for Not.</summary>
        [JsonPropertyName("not")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiSchema? Not { get; set; }

        /// <summary>Auto-generated documentation for AdditionalProperties.</summary>
        [JsonPropertyName("additionalProperties")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? AdditionalProperties { get; set; } // Can be boolean or schema

        /// <summary>Auto-generated documentation for Discriminator.</summary>
        [JsonPropertyName("discriminator")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiDiscriminator? Discriminator { get; set; }

        /// <summary>Auto-generated documentation for Title.</summary>
        [JsonPropertyName("title")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Title { get; set; }

        /// <summary>Auto-generated documentation for Default.</summary>
        [JsonPropertyName("default")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Default { get; set; }

        /// <summary>Auto-generated documentation for Nullable.</summary>
        [JsonPropertyName("nullable")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Nullable { get; set; }
        
        /// <summary>Auto-generated documentation for Example.</summary>
        [JsonPropertyName("example")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Example { get; set; }

        /// <summary>Auto-generated documentation for Examples.</summary>
        [JsonPropertyName("examples")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<object>? Examples { get; set; }

        /// <summary>Auto-generated documentation for ReadOnly.</summary>
        [JsonPropertyName("readOnly")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? ReadOnly { get; set; }

        /// <summary>Auto-generated documentation for WriteOnly.</summary>
        [JsonPropertyName("writeOnly")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? WriteOnly { get; set; }

        /// <summary>Auto-generated documentation for Enum.</summary>
        [JsonPropertyName("enum")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<object>? Enum { get; set; }

        /// <summary>Auto-generated documentation for MaxLength.</summary>
        [JsonPropertyName("maxLength")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MaxLength { get; set; }

        /// <summary>Auto-generated documentation for MinLength.</summary>
        [JsonPropertyName("minLength")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MinLength { get; set; }

        /// <summary>Auto-generated documentation for Pattern.</summary>
        [JsonPropertyName("pattern")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Pattern { get; set; }

        /// <summary>Auto-generated documentation for MaxItems.</summary>
        [JsonPropertyName("maxItems")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MaxItems { get; set; }

        /// <summary>Auto-generated documentation for MinItems.</summary>
        [JsonPropertyName("minItems")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MinItems { get; set; }

        /// <summary>Auto-generated documentation for UniqueItems.</summary>
        [JsonPropertyName("uniqueItems")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? UniqueItems { get; set; }

        /// <summary>Auto-generated documentation for MaxProperties.</summary>
        [JsonPropertyName("maxProperties")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MaxProperties { get; set; }

        /// <summary>Auto-generated documentation for MinProperties.</summary>
        [JsonPropertyName("minProperties")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MinProperties { get; set; }

        /// <summary>Auto-generated documentation for If.</summary>
        [JsonPropertyName("if")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiSchema? If { get; set; }

        /// <summary>Auto-generated documentation for Then.</summary>
        [JsonPropertyName("then")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiSchema? Then { get; set; }

        /// <summary>Auto-generated documentation for Else.</summary>
        [JsonPropertyName("else")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiSchema? Else { get; set; }

        /// <summary>Auto-generated documentation for DependentRequired.</summary>
        [JsonPropertyName("dependentRequired")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, IList<string>>? DependentRequired { get; set; }

        /// <summary>Auto-generated documentation for DependentSchemas.</summary>
        [JsonPropertyName("dependentSchemas")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiSchema>? DependentSchemas { get; set; }

        /// <summary>Auto-generated documentation for PatternProperties.</summary>
        [JsonPropertyName("patternProperties")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiSchema>? PatternProperties { get; set; }

        /// <summary>Auto-generated documentation for PropertyNames.</summary>
        [JsonPropertyName("propertyNames")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiSchema? PropertyNames { get; set; }

        /// <summary>Auto-generated documentation for UnevaluatedItems.</summary>
        [JsonPropertyName("unevaluatedItems")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? UnevaluatedItems { get; set; } // Can be boolean or schema

        /// <summary>Auto-generated documentation for UnevaluatedProperties.</summary>
        [JsonPropertyName("unevaluatedProperties")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? UnevaluatedProperties { get; set; } // Can be boolean or schema

        /// <summary>Auto-generated documentation for Contains.</summary>
        [JsonPropertyName("contains")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiSchema? Contains { get; set; }

        /// <summary>Auto-generated documentation for MinContains.</summary>
        [JsonPropertyName("minContains")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MinContains { get; set; }

        /// <summary>Auto-generated documentation for MaxContains.</summary>
        [JsonPropertyName("maxContains")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MaxContains { get; set; }

        /// <summary>Auto-generated documentation for Const.</summary>
        [JsonPropertyName("const")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Const { get; set; }

        /// <summary>Auto-generated documentation for ContentEncoding.</summary>
        [JsonPropertyName("contentEncoding")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ContentEncoding { get; set; }

        /// <summary>Auto-generated documentation for ContentMediaType.</summary>
        [JsonPropertyName("contentMediaType")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ContentMediaType { get; set; }

        /// <summary>Auto-generated documentation for ContentSchema.</summary>
        [JsonPropertyName("contentSchema")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiSchema? ContentSchema { get; set; }

        /// <summary>Auto-generated documentation for Deprecated.</summary>
        [JsonPropertyName("deprecated")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Deprecated { get; set; }
    }
}