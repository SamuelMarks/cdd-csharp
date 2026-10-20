// <copyright file="IrPathMetadata.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Ir
{
    /// <summary>
    /// Represents path-item level metadata in the Intermediate Representation.
    /// </summary>
    public class IrPathMetadata
    {
        /// <summary>Gets or sets the path-level <c>$ref</c>.</summary>
        public string? Ref { get; set; }

        /// <summary>Gets or sets the path-level summary.</summary>
        public string? Summary { get; set; }

        /// <summary>Gets or sets the path-level description.</summary>
        public string? Description { get; set; }

        /// <summary>Gets or sets the path-level servers.</summary>
        public IList<OpenApiServer>? Servers { get; set; }

        /// <summary>Gets or sets the path-level shared parameters.</summary>
        public IList<OpenApiParameter>? Parameters { get; set; }
    }
}
