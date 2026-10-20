// <copyright file="TestExtensions.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using Microsoft.CodeAnalysis;
namespace Cdd.OpenApi.Tests
{
    /// <summary>Verifies or exercises test extensions.</summary>
    public static class TestExtensions
    {
        /// <summary>Verifies or exercises to formatted string.</summary>
        /// <param name="node">The node parameter.</param>
        /// <returns>The resulting value or task.</returns>
        public static string ToFormattedString(this Microsoft.CodeAnalysis.SyntaxNode node)
        {
            return WasmSafeFormatter.Format(node);
        }
    }
}
