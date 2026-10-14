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
        /// Generates C# source files from an OpenAPI JSON specification string for WebAssembly browser interop.
        /// </summary>
        /// <param name="specContent">The raw OpenAPI specification JSON string.</param>
        /// <param name="command">The CLI command string containing optional flags such as <c>--tests</c>.</param>
        /// <param name="target">The target generation mode (<c>to_sdk</c>, <c>to_sdk_cli</c>, <c>to_server</c>, or <c>all</c>).</param>
        /// <param name="generateTests">Whether to generate unit and integration test suites.</param>
        /// <returns>A JSON-serialized dictionary mapping generated file paths to their C# source code contents.</returns>
        [JSExport]
        internal static string FromOpenApi(string specContent, string command, string target, bool generateTests = false)
        {
            try
            {
                var doc = new OpenApiParser().ParseJson(specContent);
                var type = Cdd.OpenApi.GenerateType.All;
                if (target == "to_sdk") type = Cdd.OpenApi.GenerateType.Sdk;
                else if (target == "to_sdk_cli") type = Cdd.OpenApi.GenerateType.SdkCli;
                else if (target == "to_server") type = Cdd.OpenApi.GenerateType.Server;

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
    }
}
