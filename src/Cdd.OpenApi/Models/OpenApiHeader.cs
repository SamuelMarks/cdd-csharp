using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    public class OpenApiHeader : OpenApiParameter
    {
        // Headers are basically parameters without the 'name' and 'in' properties
        // but since C# requires strictness, we just inherit to re-use properties.
        // The OAS states `name` and `in` are ignored if present for headers.
    }
}
