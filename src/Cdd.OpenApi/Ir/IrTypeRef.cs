// <copyright file="IrTypeRef.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

namespace Cdd.OpenApi.Ir
{
    /// <summary>
    /// Represents a strongly-typed Intermediate Representation (IR) of a type reference.
    /// </summary>
    public class IrTypeRef
    {
        /// <summary>
        /// Gets or sets the OpenAPI type name (e.g., "integer", "string", "array", "object").
        /// </summary>
        public string TypeName { get; set; } = "string";

        /// <summary>
        /// Gets or sets the OpenAPI format modifier (e.g., "int64", "uuid", "date-time").
        /// </summary>
        public string? Format { get; set; }

        /// <summary>
        /// Gets or sets the C# type name (e.g., "int", "string", "List&lt;string&gt;").
        /// </summary>
        public string CSharpType { get; set; } = "string";

        /// <summary>
        /// Gets or sets a value indicating whether the type reference is nullable.
        /// </summary>
        public bool IsNullable { get; set; }

        /// <summary>
        /// Gets or sets the referenced schema name when this type is a <c>$ref</c>.
        /// </summary>
        public string? Ref { get; set; }
    }
}
