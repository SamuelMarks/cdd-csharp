using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Cdd.OpenApi.DocsJson
{
    /// <summary>
    /// Source generator context for JSON serialization of DocsJson output.
    /// </summary>
    [JsonSerializable(typeof(List<DocsJsonOutput>))]
    public partial class DocsJsonContext : JsonSerializerContext
    {
    }
}
