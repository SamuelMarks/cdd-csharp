// <copyright file="OperationHelpers.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Common
{
    /// <summary>
    /// Provides shared helper methods for iterating and normalizing OpenAPI path operations.
    /// </summary>
    public static class OperationHelpers
    {
        /// <summary>
        /// Extracts all HTTP operations defined on an <see cref="OpenApiPathItem"/> keyed by PascalCase HTTP verb name.
        /// </summary>
        /// <param name="pathItem">The OpenAPI path item to inspect.</param>
        /// <returns>A dictionary of HTTP verb names (e.g., "Get", "Post") to their <see cref="OpenApiOperation"/> instances.</returns>
        public static Dictionary<string, OpenApiOperation?> GetOperationsByPascalVerb(OpenApiPathItem pathItem)
        {
            var operations = new Dictionary<string, OpenApiOperation?>
            {
                { "Get", pathItem.Get },
                { "Put", pathItem.Put },
                { "Post", pathItem.Post },
                { "Delete", pathItem.Delete },
                { "Options", pathItem.Options },
                { "Head", pathItem.Head },
                { "Patch", pathItem.Patch },
                { "Trace", pathItem.Trace },
                { "Query", pathItem.Query }
            };

            if (pathItem.AdditionalOperations != null)
            {
                foreach (var addOp in pathItem.AdditionalOperations)
                {
                    if (!string.IsNullOrEmpty(addOp.Key))
                    {
                        var normVerb = addOp.Key.Substring(0, 1).ToUpperInvariant() + addOp.Key.Substring(1).ToLowerInvariant();
                        operations[normVerb] = addOp.Value;
                    }
                }
            }

            return operations;
        }

        /// <summary>
        /// Converts a PascalCase or camelCase identifier to snake_case.
        /// </summary>
        /// <param name="text">The input identifier string.</param>
        /// <returns>The snake_case formatted string.</returns>
        public static string ToSnakeCase(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var sb = new System.Text.StringBuilder();
            sb.Append(char.ToLowerInvariant(text[0]));
            for (int i = 1; i < text.Length; ++i)
            {
                char c = text[i];
                if (char.IsUpper(c))
                {
                    sb.Append('_');
                    sb.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}
