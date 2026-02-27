using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    // Callbacks are maps of expressions to Path Items
    public class OpenApiCallback : Dictionary<string, OpenApiPathItem>
    {
    }
}
