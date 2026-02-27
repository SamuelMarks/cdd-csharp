using System;
using System.Text.Json;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Parse
{
    public class OpenApiParser
    {
        private readonly JsonSerializerOptions _jsonOptions;

        public OpenApiParser()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };
        }

        public OpenApiDocument ParseJson(string jsonContent)
        {
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                throw new ArgumentException("JSON content cannot be null or empty.", nameof(jsonContent));
            }

            try
            {
                var document = JsonSerializer.Deserialize<OpenApiDocument>(jsonContent, _jsonOptions);
                return document ?? new OpenApiDocument();
            }
            catch (JsonException ex)
            {
                throw new FormatException($"Failed to parse OpenAPI JSON: {ex.Message}", ex);
            }
        }
    }
}
