using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Parse
{
    /// <summary>JsonContext</summary>
    [JsonSerializable(typeof(Cdd.OpenApi.Models.OpenApiDocument))]
    public partial class OpenApiJsonContext : JsonSerializerContext
    {
    }
}
