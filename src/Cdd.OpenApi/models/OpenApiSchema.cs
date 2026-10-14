// <copyright file="OpenApiSchema.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    /// <summary>The Schema Object allows the definition of input and output data types in an OpenAPI specification.</summary>
    public class OpenApiSchema
    {
        /// <summary>Gets or sets the JSON Reference URI pointing to another schema definition.</summary>
        [JsonPropertyName("$ref")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Ref { get; set; }

        /// <summary>Gets or sets the data type of the schema (e.g., <c>string</c>, <c>number</c>, <c>integer</c>, <c>boolean</c>, <c>array</c>, or <c>object</c>).</summary>
        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Type { get; set; }

        /// <summary>Gets or sets the modifier format of the data type (e.g., <c>int32</c>, <c>int64</c>, <c>date-time</c>, <c>uuid</c>).</summary>
        [JsonPropertyName("format")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Format { get; set; }

        /// <summary>Gets or sets the schema describing elements of an array when <see cref="Type"/> is <c>array</c>.</summary>
        [JsonPropertyName("items")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiSchema? Items { get; set; }

        /// <summary>Gets or sets the property definitions of an object schema.</summary>
        [JsonPropertyName("properties")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiSchema>? Properties { get; set; }

        /// <summary>Gets or sets the list of required property names on an object schema.</summary>
        [JsonPropertyName("required")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<string>? Required { get; set; }

        /// <summary>Gets or sets a description of the schema.</summary>
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

        /// <summary>Gets or sets the inclusive minimum numeric value.</summary>
        [JsonPropertyName("minimum")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? Minimum { get; set; }

        /// <summary>Gets or sets the inclusive maximum numeric value.</summary>
        [JsonPropertyName("maximum")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? Maximum { get; set; }

        /// <summary>Gets or sets the exclusive minimum numeric value.</summary>
        [JsonPropertyName("exclusiveMinimum")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? ExclusiveMinimum { get; set; }

        /// <summary>Gets or sets the exclusive maximum numeric value.</summary>
        [JsonPropertyName("exclusiveMaximum")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? ExclusiveMaximum { get; set; }

        /// <summary>Gets or sets the list of schemas that must all be valid (<c>allOf</c> composition).</summary>
        [JsonPropertyName("allOf")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<OpenApiSchema>? AllOf { get; set; }

        /// <summary>Gets or sets the list of schemas where at least one must be valid (<c>anyOf</c> composition).</summary>
        [JsonPropertyName("anyOf")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<OpenApiSchema>? AnyOf { get; set; }

        /// <summary>Gets or sets the list of schemas where exactly one must be valid (<c>oneOf</c> composition).</summary>
        [JsonPropertyName("oneOf")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<OpenApiSchema>? OneOf { get; set; }

        /// <summary>Gets or sets a schema that must not be valid (<c>not</c> negation).</summary>
        [JsonPropertyName("not")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiSchema? Not { get; set; }

        /// <summary>Gets or sets a boolean or schema governing additional object properties not listed in <see cref="Properties"/>.</summary>
        [JsonPropertyName("additionalProperties")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiAny? AdditionalProperties { get; set; }

        /// <summary>Gets or sets additional external documentation for this schema.</summary>
        [JsonPropertyName("externalDocs")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiExternalDocs? ExternalDocs { get; set; }

        /// <summary>Gets or sets the polymorphism discriminator object.</summary>
        [JsonPropertyName("discriminator")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiDiscriminator? Discriminator { get; set; }

        /// <summary>Gets or sets additional XML representation metadata for this schema.</summary>
        [JsonPropertyName("xml")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiXml? Xml { get; set; }

        /// <summary>Gets or sets a short title for the schema.</summary>
        [JsonPropertyName("title")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Title { get; set; }

        /// <summary>Gets or sets the default value of the schema.</summary>
        [JsonPropertyName("default")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiAny? Default { get; set; }

        /// <summary>Gets or sets a value indicating whether <c>null</c> is a valid value for the schema.</summary>
        [JsonPropertyName("nullable")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Nullable { get; set; }

        /// <summary>Gets or sets a free-form example instance of this schema.</summary>
        [JsonPropertyName("example")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiAny? Example { get; set; }

        /// <summary>Gets or sets a list of example instances of this schema.</summary>
        [JsonPropertyName("examples")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<OpenApiAny>? Examples { get; set; }

        /// <summary>Gets or sets a value indicating whether the property is sent only in responses and not in requests.</summary>
        [JsonPropertyName("readOnly")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? ReadOnly { get; set; }

        /// <summary>Gets or sets a value indicating whether the property is sent only in requests and not in responses.</summary>
        [JsonPropertyName("writeOnly")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? WriteOnly { get; set; }

        /// <summary>Gets or sets the list of allowed enumerated values for the schema.</summary>
        [JsonPropertyName("enum")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<OpenApiAny>? Enum { get; set; }

        /// <summary>Gets or sets the maximum character length of a string instance.</summary>
        [JsonPropertyName("maxLength")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MaxLength { get; set; }

        /// <summary>Gets or sets the minimum character length of a string instance.</summary>
        [JsonPropertyName("minLength")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MinLength { get; set; }

        /// <summary>Gets or sets the regular expression pattern that a string instance must match.</summary>
        [JsonPropertyName("pattern")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Pattern { get; set; }

        /// <summary>Gets or sets the maximum number of elements in an array instance.</summary>
        [JsonPropertyName("maxItems")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MaxItems { get; set; }

        /// <summary>Gets or sets the minimum number of elements in an array instance.</summary>
        [JsonPropertyName("minItems")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MinItems { get; set; }

        /// <summary>Gets or sets a value indicating whether all elements in an array instance must be unique.</summary>
        [JsonPropertyName("uniqueItems")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? UniqueItems { get; set; }

        /// <summary>Gets or sets the maximum number of properties in an object instance.</summary>
        [JsonPropertyName("maxProperties")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MaxProperties { get; set; }

        /// <summary>Gets or sets the minimum number of properties in an object instance.</summary>
        [JsonPropertyName("minProperties")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MinProperties { get; set; }

        /// <summary>Gets or sets the conditional <c>if</c> subschema.</summary>
        [JsonPropertyName("if")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiSchema? If { get; set; }

        /// <summary>Gets or sets the conditional <c>then</c> subschema applied when <see cref="If"/> succeeds.</summary>
        [JsonPropertyName("then")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiSchema? Then { get; set; }

        /// <summary>Gets or sets the conditional <c>else</c> subschema applied when <see cref="If"/> fails.</summary>
        [JsonPropertyName("else")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiSchema? Else { get; set; }

        /// <summary>Gets or sets property dependencies where the presence of a property requires other properties.</summary>
        [JsonPropertyName("dependentRequired")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, IList<string>>? DependentRequired { get; set; }

        /// <summary>Gets or sets schema dependencies where the presence of a property requires the object to validate against a subschema.</summary>
        [JsonPropertyName("dependentSchemas")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiSchema>? DependentSchemas { get; set; }

        /// <summary>Gets or sets a map of regular expressions to schemas for validating matching property names.</summary>
        [JsonPropertyName("patternProperties")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiSchema>? PatternProperties { get; set; }

        /// <summary>Gets or sets a schema that all property names of an object must validate against.</summary>
        [JsonPropertyName("propertyNames")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiSchema? PropertyNames { get; set; }

        /// <summary>Gets or sets a boolean or schema governing array items not evaluated by adjacent keywords.</summary>
        [JsonPropertyName("unevaluatedItems")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiAny? UnevaluatedItems { get; set; }

        /// <summary>Gets or sets a boolean or schema governing object properties not evaluated by adjacent keywords.</summary>
        [JsonPropertyName("unevaluatedProperties")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiAny? UnevaluatedProperties { get; set; }

        /// <summary>Gets or sets a schema that at least one element of an array must validate against.</summary>
        [JsonPropertyName("contains")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiSchema? Contains { get; set; }

        /// <summary>Gets or sets the minimum number of array elements that must match <see cref="Contains"/>.</summary>
        [JsonPropertyName("minContains")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MinContains { get; set; }

        /// <summary>Gets or sets the maximum number of array elements that may match <see cref="Contains"/>.</summary>
        [JsonPropertyName("maxContains")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MaxContains { get; set; }

        /// <summary>Gets or sets the single constant value that an instance must equal.</summary>
        [JsonPropertyName("const")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiAny? Const { get; set; }

        /// <summary>Gets or sets the encoding used to store binary content in a string (e.g., <c>base64</c>).</summary>
        [JsonPropertyName("contentEncoding")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ContentEncoding { get; set; }

        /// <summary>Gets or sets the MIME media type of the content stored in a string.</summary>
        [JsonPropertyName("contentMediaType")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ContentMediaType { get; set; }

        /// <summary>Gets or sets the schema describing the structure of decoded string content when <see cref="ContentMediaType"/> is specified.</summary>
        [JsonPropertyName("contentSchema")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiSchema? ContentSchema { get; set; }

        /// <summary>Gets or sets a value indicating whether the schema is deprecated.</summary>
        [JsonPropertyName("deprecated")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Deprecated { get; set; }
    }
}
