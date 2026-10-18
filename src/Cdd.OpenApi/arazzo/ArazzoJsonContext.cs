// <copyright file="ArazzoJsonContext.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Cdd.OpenApi.Models;
using Cdd.OpenApi.Models.Arazzo;

namespace Cdd.OpenApi.Arazzo
{
    /// <summary>
    /// Source-generated <see cref="JsonSerializerContext"/> for trimming-safe and NativeAOT/WASM-compatible Arazzo 1.1.0 serialization and deserialization.
    /// </summary>
    [JsonSourceGenerationOptions(
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(ArazzoDocument))]
    [JsonSerializable(typeof(ArazzoInfo))]
    [JsonSerializable(typeof(ArazzoSourceDescription))]
    [JsonSerializable(typeof(ArazzoWorkflow))]
    [JsonSerializable(typeof(ArazzoStep))]
    [JsonSerializable(typeof(ArazzoParameter))]
    [JsonSerializable(typeof(ArazzoSuccessAction))]
    [JsonSerializable(typeof(ArazzoFailureAction))]
    [JsonSerializable(typeof(ArazzoCriterion))]
    [JsonSerializable(typeof(ArazzoExpressionType))]
    [JsonSerializable(typeof(ArazzoCriterionType))]
    [JsonSerializable(typeof(ArazzoSelector))]
    [JsonSerializable(typeof(ArazzoStepOutput))]
    [JsonSerializable(typeof(ArazzoRequestBody))]
    [JsonSerializable(typeof(ArazzoPayloadReplacement))]
    [JsonSerializable(typeof(ArazzoComponents))]
    [JsonSerializable(typeof(OpenApiSchema))]
    [JsonSerializable(typeof(OpenApiAny))]
    [JsonSerializable(typeof(JsonElement))]
    [JsonSerializable(typeof(JsonNode))]
    [JsonSerializable(typeof(JsonObject))]
    [JsonSerializable(typeof(JsonArray))]
    [JsonSerializable(typeof(Dictionary<string, string>))]
    [JsonSerializable(typeof(Dictionary<string, ArazzoStepOutput>))]
    [JsonSerializable(typeof(Dictionary<string, OpenApiSchema>))]
    [JsonSerializable(typeof(Dictionary<string, ArazzoParameter>))]
    [JsonSerializable(typeof(Dictionary<string, ArazzoSuccessAction>))]
    [JsonSerializable(typeof(Dictionary<string, ArazzoFailureAction>))]
    [JsonSerializable(typeof(Dictionary<string, JsonElement>))]
    public partial class ArazzoJsonContext : JsonSerializerContext
    {
    }
}
