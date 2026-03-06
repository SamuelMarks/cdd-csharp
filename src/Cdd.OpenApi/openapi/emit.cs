using System;
using System.Text.Json;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Emit
{
/// <summary>Auto-generated documentation for OpenApiEmitter.</summary>
    public class OpenApiEmitter
    {
        private readonly JsonSerializerOptions _jsonOptions;

/// <summary>Auto-generated documentation for OpenApiEmitter.</summary>
        public OpenApiEmitter()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
        }

/// <summary>Auto-generated documentation for EmitJson.</summary>
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
