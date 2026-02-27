using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    // A dictionary wrapper to handle paths correctly during parsing and serialization
    public class OpenApiPaths : Dictionary<string, OpenApiPathItem>
    {
    }
}
