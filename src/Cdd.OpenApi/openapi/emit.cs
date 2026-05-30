using System;
using System.Text.Json;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Emit
{
    /// <summary>Auto-generated documentation for OpenApiEmitter.</summary>
    public class OpenApiEmitter
    {
        /// <summary>Auto-generated documentation for OpenApiEmitter.</summary>
        public OpenApiEmitter()
        {
        }

        /// <summary>Auto-generated documentation for EmitJson.</summary>
        public string EmitJson(OpenApiDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return JsonSerializer.Serialize(document, Cdd.OpenApi.Parse.OpenApiJsonContext.Default.OpenApiDocument);
        }
    }
}
