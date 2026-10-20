// <copyright file="IrModel.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Cdd.OpenApi.Common;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Ir
{
    /// <summary>
    /// Represents a data model (schema/class) in the Intermediate Representation.
    /// </summary>
    public class IrModel
    {
        /// <summary>
        /// Gets or sets the name of the model.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the model.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the example value of the model.
        /// </summary>
        public OpenApiAny? Example { get; set; }

        /// <summary>
        /// Gets or sets the discriminator metadata of the model.
        /// </summary>
        public OpenApiDiscriminator? Discriminator { get; set; }

        /// <summary>
        /// Gets or sets the XML serialization metadata of the model.
        /// </summary>
        public OpenApiXml? Xml { get; set; }

        /// <summary>
        /// Gets or sets the external documentation of the model.
        /// </summary>
        public OpenApiExternalDocs? ExternalDocs { get; set; }

        /// <summary>
        /// Gets or sets the list of fields belonging to the model.
        /// </summary>
        public List<IrField> Fields { get; set; } = new List<IrField>();

        /// <summary>
        /// Gets or sets the full underlying <see cref="OpenApiSchema"/> for lossless schema metadata (XML, Discriminator, JSON Schema 2020-12 keywords).
        /// </summary>
        public OpenApiSchema? Schema { get; set; }

        /// <summary>
        /// Creates an <see cref="IrModel"/> from a model name and an <see cref="OpenApiSchema"/>.
        /// </summary>
        /// <param name="name">The schema/model name.</param>
        /// <param name="schema">The source OpenAPI schema.</param>
        /// <returns>The constructed <see cref="IrModel"/>.</returns>
        public static IrModel FromOpenApiSchema(string name, OpenApiSchema schema)
        {
            var irModel = new IrModel
            {
                Name = name,
                Description = schema.Description,
                Example = schema.Example,
                Discriminator = schema.Discriminator,
                Xml = schema.Xml,
                ExternalDocs = schema.ExternalDocs,
                Schema = schema
            };

            if (schema.Properties != null)
            {
                foreach (var propKvp in schema.Properties)
                {
                    var isReq = schema.Required != null && schema.Required.Contains(propKvp.Key);
                    irModel.Fields.Add(IrField.FromOpenApiSchema(propKvp.Key, propKvp.Value, isReq));
                }
            }

            return irModel;
        }

        /// <summary>
        /// Converts this <see cref="IrModel"/> back into an <see cref="OpenApiSchema"/>.
        /// </summary>
        /// <returns>The reconstructed <see cref="OpenApiSchema"/>.</returns>
        public OpenApiSchema ToOpenApiSchema()
        {
            var props = new Dictionary<string, OpenApiSchema>();
            var reqs = new List<string>();
            foreach (var field in Fields)
            {
                props[field.Name] = field.ToOpenApiSchema();
                if (field.IsRequired)
                {
                    reqs.Add(field.Name);
                }
            }

            var modelSchema = Schema ?? new OpenApiSchema { Type = OpenApiDataType.Object };
            modelSchema.Description = Description ?? modelSchema.Description;
            modelSchema.Example = Example ?? modelSchema.Example;
            modelSchema.Discriminator = Discriminator ?? modelSchema.Discriminator;
            modelSchema.Xml = Xml ?? modelSchema.Xml;
            modelSchema.ExternalDocs = ExternalDocs ?? modelSchema.ExternalDocs;
            if (props.Count > 0) modelSchema.Properties = props;
            if (reqs.Count > 0) modelSchema.Required = reqs;
            return modelSchema;
        }
    }
}
