// <copyright file="OpenApiParameter.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    /// <summary>Describes a single operation parameter.</summary>
    public class OpenApiParameter
    {
        /// <summary>Gets or sets the name of the parameter.</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Gets or sets the location of the parameter (<c>query</c>, <c>header</c>, <c>path</c>, or <c>cookie</c>).</summary>
        [JsonPropertyName("in")]
        public string In { get; set; } = string.Empty;

        /// <summary>Gets or sets a brief description of the parameter.</summary>
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

        /// <summary>Gets or sets a value indicating whether this parameter is mandatory.</summary>
        [JsonPropertyName("required")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Required { get; set; }

        /// <summary>Gets or sets a value indicating whether a parameter is deprecated.</summary>
        [JsonPropertyName("deprecated")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Deprecated { get; set; }

        /// <summary>Gets or sets a value indicating whether the parameter value is allowed to be empty.</summary>
        [JsonPropertyName("allowEmptyValue")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? AllowEmptyValue { get; set; }

        /// <summary>Gets or sets a description of how the parameter value will be serialized depending on the type of the parameter value.</summary>
        [JsonPropertyName("style")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Style { get; set; }

        /// <summary>Gets or sets a value indicating whether parameter values of type <c>array</c> or <c>object</c> generate separate parameters for each value.</summary>
        [JsonPropertyName("explode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Explode { get; set; }

        /// <summary>Gets or sets a value indicating whether the parameter value SHOULD allow reserved characters.</summary>
        [JsonPropertyName("allowReserved")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? AllowReserved { get; set; }

        /// <summary>Gets or sets the schema defining the type used for the parameter.</summary>
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
        public OpenApiAny? Default { get; set; }

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
        public IList<OpenApiAny>? Enum { get; set; }

        /// <summary>Swagger 2.0 multipleOf</summary>
        [JsonPropertyName("multipleOf")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? MultipleOf { get; set; }

        /// <summary>Gets or sets a map containing the media type representations for the parameter.</summary>
        [JsonPropertyName("content")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiMediaType>? Content { get; set; }

        /// <summary>Gets or sets an example of the parameter's potential value.</summary>
        [JsonPropertyName("example")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiAny? Example { get; set; }

        /// <summary>Gets or sets a map of examples of the parameter's potential value.</summary>
        [JsonPropertyName("examples")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiExample>? Examples { get; set; }
    }
}
