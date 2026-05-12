using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Parse
{
    /// <summary>JsonContext</summary>
    [JsonSourceGenerationOptions(
        ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonSerializable(typeof(Cdd.OpenApi.Models.OpenApiDocument))]
    public partial class OpenApiJsonContext : JsonSerializerContext
    {
    }
}
