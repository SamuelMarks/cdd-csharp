using System.Text.Json.Serialization;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Parse
{
    /// <summary>
    /// Source generator context for JSON serialization of OpenAPI models.
    /// </summary>
    [JsonSerializable(typeof(OpenApiDocument))]
    [JsonSerializable(typeof(OpenApiSchema))]
    [JsonSerializable(typeof(OpenApiPaths))]
    [JsonSerializable(typeof(OpenApiPathItem))]
    [JsonSerializable(typeof(OpenApiOperation))]
    [JsonSerializable(typeof(OpenApiParameter))]
    [JsonSerializable(typeof(OpenApiRequestBody))]
    [JsonSerializable(typeof(OpenApiResponse))]
    [JsonSerializable(typeof(OpenApiHeader))]
    [JsonSerializable(typeof(OpenApiMediaType))]
    [JsonSerializable(typeof(OpenApiInfo))]
    [JsonSerializable(typeof(OpenApiContact))]
    [JsonSerializable(typeof(OpenApiLicense))]
    [JsonSerializable(typeof(OpenApiComponents))]
    [JsonSerializable(typeof(OpenApiServer))]
    [JsonSerializable(typeof(OpenApiServerVariable))]
    [JsonSerializable(typeof(OpenApiTag))]
    [JsonSerializable(typeof(OpenApiExternalDocs))]
    [JsonSerializable(typeof(OpenApiDiscriminator))]
    [JsonSerializable(typeof(OpenApiXml))]
    [JsonSerializable(typeof(string[]))]
    [JsonSerializable(typeof(System.Collections.Generic.Dictionary<string, string>))]
    [JsonSerializable(typeof(System.Collections.Generic.Dictionary<string, OpenApiSchema>))]
    public partial class OpenApiJsonContext : JsonSerializerContext
    {
    }
}
