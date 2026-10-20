// <copyright file="WasmSafeFormatterTests.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Cdd.OpenApi.Tests
{
    /// <summary>
    /// Verifies that <see cref="WasmSafeFormatter"/> produces valid, diagnostics-free C# syntax.
    /// </summary>
    public class WasmSafeFormatterTests
    {
        /// <summary>
        /// Verifies that formatting a C# class with attributes and array properties produces valid, re-parseable C# syntax with zero diagnostics.
        /// </summary>
        [Fact]
        public void TestFormatter()
        {
            var code = "[System.Serializable] class A { int[] B { get; set; } }";
            var tree = CSharpSyntaxTree.ParseText(code);
            var result = WasmSafeFormatter.Format(tree.GetRoot());
            Assert.Contains("class A", result);
            Assert.Contains("int[]", result);
            Assert.Contains("[System.Serializable]", result);

            var reparsed = CSharpSyntaxTree.ParseText(result);
            Assert.Empty(reparsed.GetDiagnostics());
        }
    }
}
