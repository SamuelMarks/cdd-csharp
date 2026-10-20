// <copyright file="IrField.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Cdd.OpenApi.Common;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Ir
{
    /// <summary>
    /// Represents a field or property on an <see cref="IrModel"/>.
    /// </summary>
    public class IrField
    {
        /// <summary>
        /// Gets or sets the property name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the field's resolved type reference.
        /// </summary>
        public IrTypeRef Type { get; set; } = new IrTypeRef();

        /// <summary>
        /// Gets or sets a value indicating whether the field is required.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets the human-readable description of the field.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the example value for the field.
        /// </summary>
        public OpenApiAny? Example { get; set; }

        /// <summary>
        /// Gets or sets the discriminator metadata for the field.
        /// </summary>
        public OpenApiDiscriminator? Discriminator { get; set; }

        /// <summary>
        /// Gets or sets the XML serialization metadata for the field.
        /// </summary>
        public OpenApiXml? Xml { get; set; }

        /// <summary>
        /// Gets or sets the external documentation for the field.
        /// </summary>
        public OpenApiExternalDocs? ExternalDocs { get; set; }

        /// <summary>
        /// Gets or sets the allowed enumeration values for the field.
        /// </summary>
        public IList<OpenApiAny>? Enum { get; set; }

        /// <summary>
        /// Gets or sets the minimum numeric value constraint.
        /// </summary>
        public double? Minimum { get; set; }

        /// <summary>
        /// Gets or sets the maximum numeric value constraint.
        /// </summary>
        public double? Maximum { get; set; }

        /// <summary>
        /// Gets or sets the minimum string length constraint.
        /// </summary>
        public int? MinLength { get; set; }

        /// <summary>
        /// Gets or sets the maximum string length constraint.
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// Gets or sets the regular expression pattern constraint.
        /// </summary>
        public string? Pattern { get; set; }

        /// <summary>
        /// Gets or sets the full underlying <see cref="OpenApiSchema"/> for lossless schema metadata preservation.
        /// </summary>
        public OpenApiSchema? Schema { get; set; }

        /// <summary>
        /// Creates an <see cref="IrField"/> from a property name, an <see cref="OpenApiSchema"/>, and its required status.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="schema">The OpenAPI property schema.</param>
        /// <param name="isRequired">Whether the property is required on its parent model.</param>
        /// <returns>The constructed <see cref="IrField"/>.</returns>
        public static IrField FromOpenApiSchema(string name, OpenApiSchema schema, bool isRequired)
        {
            var openApiType = schema.Type ?? OpenApiDataType.Object;
            return new IrField
            {
                Name = name,
                IsRequired = isRequired,
                Description = schema.Description,
                Example = schema.Example,
                Discriminator = schema.Discriminator,
                Xml = schema.Xml,
                ExternalDocs = schema.ExternalDocs,
                Enum = schema.Enum,
                Minimum = schema.Minimum,
                Maximum = schema.Maximum,
                MinLength = schema.MinLength,
                MaxLength = schema.MaxLength,
                Pattern = schema.Pattern,
                Schema = schema,
                Type = new IrTypeRef
                {
                    TypeName = openApiType,
                    Format = schema.Format,
                    Ref = schema.Ref,
                    CSharpType = TypeMapper.MapSchemaToCSharp(schema, "object"),
                    IsNullable = !isRequired
                }
            };
        }

        /// <summary>
        /// Converts this <see cref="IrField"/> back into an <see cref="OpenApiSchema"/>.
        /// </summary>
        /// <returns>The reconstructed <see cref="OpenApiSchema"/>.</returns>
        public OpenApiSchema ToOpenApiSchema()
        {
            var fieldSchema = Schema ?? new OpenApiSchema();
            fieldSchema.Type = Type.TypeName;
            fieldSchema.Format = Type.Format;
            fieldSchema.Ref = Type.Ref ?? fieldSchema.Ref;
            fieldSchema.Description = Description ?? fieldSchema.Description;
            fieldSchema.Example = Example ?? fieldSchema.Example;
            fieldSchema.Discriminator = Discriminator ?? fieldSchema.Discriminator;
            fieldSchema.Xml = Xml ?? fieldSchema.Xml;
            fieldSchema.ExternalDocs = ExternalDocs ?? fieldSchema.ExternalDocs;
            fieldSchema.Enum = Enum ?? fieldSchema.Enum;
            fieldSchema.Minimum = Minimum ?? fieldSchema.Minimum;
            fieldSchema.Maximum = Maximum ?? fieldSchema.Maximum;
            fieldSchema.MinLength = MinLength ?? fieldSchema.MinLength;
            fieldSchema.MaxLength = MaxLength ?? fieldSchema.MaxLength;
            fieldSchema.Pattern = Pattern ?? fieldSchema.Pattern;
            return fieldSchema;
        }
    }
}
