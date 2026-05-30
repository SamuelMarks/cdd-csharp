using System;
using System.IO;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using Cdd.OpenApi.Parse;

/// <summary>
/// Provides interop methods for running within a WebAssembly browser environment.
/// </summary>
public partial class BrowserInterop
{
    [JSExport]
    internal static string GenerateFromOpenApi(string specContent, string command, string target, bool generateTests = false)
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
