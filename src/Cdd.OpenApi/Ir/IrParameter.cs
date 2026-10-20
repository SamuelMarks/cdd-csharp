// <copyright file="IrParameter.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Cdd.OpenApi.Common;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Ir
{
    /// <summary>
    /// Represents an operation parameter in the Intermediate Representation.
    /// </summary>
    public class IrParameter
    {
        /// <summary>
        /// Gets or sets the parameter name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the parameter location ("path", "query", "header", "cookie").
        /// </summary>
        public string Location { get; set; } = "query";

        /// <summary>
        /// Gets or sets a value indicating whether the parameter is required.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets the parameter's type reference.
        /// </summary>
        public IrTypeRef Type { get; set; } = new IrTypeRef();

        /// <summary>
        /// Gets or sets the parameter description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the parameter is deprecated.
        /// </summary>
        public bool? Deprecated { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether empty values are allowed.
        /// </summary>
        public bool? AllowEmptyValue { get; set; }

        /// <summary>
        /// Gets or sets the serialization style of the parameter.
        /// </summary>
        public string? Style { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether array/object values generate separate parameters.
        /// </summary>
        public bool? Explode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether reserved characters are allowed without percent-encoding.
        /// </summary>
        public bool? AllowReserved { get; set; }

        /// <summary>
        /// Gets or sets the parameter example value.
        /// </summary>
        public OpenApiAny? Example { get; set; }

        /// <summary>
        /// Gets or sets the map of named parameter examples.
        /// </summary>
        public IDictionary<string, OpenApiExample>? Examples { get; set; }

        /// <summary>
        /// Gets or sets the map of media types for complex parameter serialization.
        /// </summary>
        public IDictionary<string, OpenApiMediaType>? Content { get; set; }

        /// <summary>
        /// Gets or sets the full parameter schema.
        /// </summary>
        public OpenApiSchema? Schema { get; set; }

        /// <summary>
        /// Creates an <see cref="IrParameter"/> from an <see cref="OpenApiParameter"/>.
        /// </summary>
        /// <param name="param">The source OpenAPI parameter.</param>
        /// <returns>The constructed <see cref="IrParameter"/>.</returns>
        public static IrParameter FromOpenApiParameter(OpenApiParameter param)
        {
            var pType = param.Schema?.Type ?? param.Type ?? OpenApiDataType.String;
            return new IrParameter
            {
                Name = param.Name,
                Location = param.In,
                IsRequired = param.Required == true,
                Description = param.Description,
                Deprecated = param.Deprecated,
                AllowEmptyValue = param.AllowEmptyValue,
                Style = param.Style,
                Explode = param.Explode,
                AllowReserved = param.AllowReserved,
                Example = param.Example,
                Examples = param.Examples,
                Content = param.Content,
                Schema = param.Schema,
                Type = new IrTypeRef
                {
                    TypeName = pType,
                    Format = param.Schema?.Format ?? param.Format,
                    Ref = param.Schema?.Ref,
                    CSharpType = param.Schema != null
                        ? TypeMapper.MapSchemaToCSharp(param.Schema, "string")
                        : TypeMapper.MapOpenApiToCSharp(pType, "string"),
                    IsNullable = param.Required != true
                }
            };
        }

        /// <summary>
        /// Converts this <see cref="IrParameter"/> back into an <see cref="OpenApiParameter"/>.
        /// </summary>
        /// <returns>The reconstructed <see cref="OpenApiParameter"/>.</returns>
        public OpenApiParameter ToOpenApiParameter()
        {
            return new OpenApiParameter
            {
                Name = Name,
                In = Location,
                Required = IsRequired,
                Description = Description,
                Deprecated = Deprecated,
                AllowEmptyValue = AllowEmptyValue,
                Style = Style,
                Explode = Explode,
                AllowReserved = AllowReserved,
                Example = Example,
                Examples = Examples,
                Content = Content,
                Schema = Schema
            };
        }
    }
}
