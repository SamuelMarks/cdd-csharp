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
                var document = JsonSerializer.Deserialize(jsonContent, OpenApiJsonContext.Default.OpenApiDocument);
                return document ?? new OpenApiDocument();
            }
            catch (Exception ex) when (ex is not JsonException && (ex is MissingMethodException || ex.InnerException is MissingMethodException || ex is TypeInitializationException))
            {
                // Fallback for environments where the source-generated context has BCL version mismatches (e.g., WASI/Wasi.Sdk)
                return DeserializeFallback(jsonContent);
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

        private static OpenApiDocument DeserializeFallback(string jsonContent)
        {
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
    }
}
