// <copyright file="BrowserInterop.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using Cdd.OpenApi.Parse;

namespace Cdd.OpenApi.Cli
{
    /// <summary>
    /// Provides interop methods for running within a WebAssembly browser environment.
    /// </summary>
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    public partial class BrowserInterop
    {
        /// <summary>
        /// Generates C# source files or documentation JSON from an OpenAPI JSON specification string for WebAssembly browser interop.
        /// </summary>
        /// <param name="specContent">The raw OpenAPI specification JSON string.</param>
        /// <param name="command">The CLI command string containing optional flags such as <c>--tests</c>.</param>
        /// <param name="target">The target generation mode (<c>to_sdk</c>, <c>to_sdk_cli</c>, <c>to_server</c>, <c>to_docs_json</c>, or <c>all</c>).</param>
        /// <param name="generateTests">Whether to generate unit and integration test suites.</param>
        /// <returns>A JSON-serialized dictionary mapping generated file paths to their C# source code contents, or documentation JSON when <paramref name="target"/> is <c>to_docs_json</c>.</returns>
        [JSExport]
        internal static string GenerateFromOpenApi(string specContent, string command, string target, bool generateTests = false)
        {
            return FromOpenApi(specContent, command, target, generateTests);
        }

        /// <summary>
        /// Generates C# source files from an OpenAPI JSON specification string for WebAssembly browser interop.
        /// </summary>
        /// <param name="specContent">The raw OpenAPI specification JSON string.</param>
        /// <param name="command">The CLI command string containing optional flags such as <c>--tests</c>.</param>
        /// <param name="target">The target generation mode (<c>to_sdk</c>, <c>to_sdk_cli</c>, <c>to_server</c>, <c>to_docs_json</c>, or <c>all</c>).</param>
        /// <param name="generateTests">Whether to generate unit and integration test suites.</param>
        /// <returns>A JSON-serialized dictionary mapping generated file paths to their C# source code contents.</returns>
        [JSExport]
        internal static string FromOpenApi(string specContent, string command, string target, bool generateTests = false)
        {
            try
            {
                var doc = new OpenApiParser().ParseJson(specContent);
                if (string.Equals(target, "to_docs_json", StringComparison.OrdinalIgnoreCase) ||
                    (command != null && command.Contains("to_docs_json", StringComparison.OrdinalIgnoreCase)))
                {
                    bool noImports = command != null && command.Contains("--no-imports", StringComparison.OrdinalIgnoreCase);
                    bool noWrapping = command != null && command.Contains("--no-wrapping", StringComparison.OrdinalIgnoreCase);
                    return Cdd.OpenApi.DocsJson.DocsJsonGenerator.Generate(doc, noImports, noWrapping);
                }

                var type = CliArgumentParser.ParseGenerateType(target);

                bool isTests = generateTests || (command != null && command.Contains("--tests"));

                var codes = Cdd.OpenApi.CodeGenerator.Generate(doc, "Generated", type, isTests);
                var result = new System.Collections.Generic.Dictionary<string, string>();
                foreach (var code in codes)
                {
                    result[code.FileName] = code.Code;
                }

                if (isTests && (type == Cdd.OpenApi.GenerateType.Sdk || type == Cdd.OpenApi.GenerateType.All))
                {
                    result["tests/IntegrationTests.cs"] = Cdd.OpenApi.IntegrationTestGenerator.Generate(doc);
                }

                return JsonSerializer.Serialize(result, Cdd.OpenApi.Parse.OpenApiJsonContext.Default.DictionaryStringString);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new System.Collections.Generic.Dictionary<string, string> { { "error", ex.Message } }, Cdd.OpenApi.Parse.OpenApiJsonContext.Default.DictionaryStringString);
            }
        }

        /// <summary>
        /// Generates C# workflow source files from an Arazzo 1.1.0 JSON or YAML specification string for WebAssembly browser interop.
        /// </summary>
        /// <param name="arazzoContent">The raw Arazzo 1.1.0 specification JSON or YAML string.</param>
        /// <param name="target">The target generation mode (<c>to_sdk</c>, <c>to_sdk_cli</c>, <c>to_server</c>, or <c>all</c>).</param>
        /// <param name="generateTests">Whether to generate xUnit workflow test files.</param>
        /// <returns>A JSON-serialized dictionary mapping generated file paths to their C# source code contents.</returns>
        [JSExport]
        internal static string FromArazzo(string arazzoContent, string target = "all", bool generateTests = false)
        {
            try
            {
                var doc = Cdd.OpenApi.Arazzo.ArazzoParser.ParseYamlOrJson(arazzoContent);
                var type = CliArgumentParser.ParseGenerateType(target);

                var codes = Cdd.OpenApi.ArazzoCodeGenerator.Generate(doc, "Generated", type, generateTests);
                var result = new System.Collections.Generic.Dictionary<string, string>();
                foreach (var code in codes)
                {
                    result[code.FileName] = code.Code;
                }

                return JsonSerializer.Serialize(result, Cdd.OpenApi.Parse.OpenApiJsonContext.Default.DictionaryStringString);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new System.Collections.Generic.Dictionary<string, string> { { "error", ex.Message } }, Cdd.OpenApi.Parse.OpenApiJsonContext.Default.DictionaryStringString);
            }
        }

        /// <summary>
        /// Extracts an Arazzo 1.1.0 JSON specification string from C# workflow source code for WebAssembly browser interop.
        /// </summary>
        /// <param name="csharpCode">The C# workflow source code string.</param>
        /// <returns>The serialized Arazzo 1.1.0 JSON specification string, or a JSON error object on failure.</returns>
        [JSExport]
        internal static string ToArazzo(string csharpCode)
        {
            try
            {
                var doc = Cdd.OpenApi.ArazzoSpecGenerator.Generate(csharpCode);
                return Cdd.OpenApi.Arazzo.ArazzoEmitter.EmitJson(doc);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new System.Collections.Generic.Dictionary<string, string> { { "error", ex.Message } }, Cdd.OpenApi.Parse.OpenApiJsonContext.Default.DictionaryStringString);
            }
        }
    }
}
