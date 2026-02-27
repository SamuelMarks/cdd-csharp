using System;
using System.Text.Json;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Emit
{
    public class OpenApiEmitter
    {
        private readonly JsonSerializerOptions _jsonOptions;

        public OpenApiEmitter()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
        }

        public string EmitJson(OpenApiDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return JsonSerializer.Serialize(document, _jsonOptions);
        }
    }
}
