using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Parse
{
/// <summary>Auto-generated documentation for OpenApiParser.</summary>
    public class OpenApiParser
    {
/// <summary>Auto-generated documentation for OpenApiParser.</summary>
        public OpenApiParser()
        {
        }

/// <summary>Auto-generated documentation for ParseJson.</summary>
        public OpenApiDocument ParseJson(string jsonContent)
        {
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                throw new ArgumentException("JSON content cannot be null or empty.", nameof(jsonContent));
            }

            try
            {
                var document = JsonSerializer.Deserialize<OpenApiDocument>(jsonContent, FallbackOptions);
                return document ?? new OpenApiDocument();
            }
            catch (JsonException ex)
            {
                throw new FormatException($"Failed to parse OpenAPI JSON: {ex.Message}", ex);
            }
        }

        private static readonly JsonSerializerOptions FallbackOptions = new JsonSerializerOptions
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };
    }
}