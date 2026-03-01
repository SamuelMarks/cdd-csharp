using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.DocsJson
{
    /// <summary>DocsJsonOutput</summary>
    public class DocsJsonOutput
    {
        /// <summary>Language</summary>
        [JsonPropertyName("language")]
        public string Language { get; set; } = "csharp";

        /// <summary>Operations</summary>
        [JsonPropertyName("operations")]
        public List<DocsJsonOperation> Operations { get; set; } = new List<DocsJsonOperation>();
    }

    /// <summary>DocsJsonOperation</summary>
    public class DocsJsonOperation
    {
        /// <summary>Method</summary>
        [JsonPropertyName("method")]
        public string Method { get; set; } = string.Empty;

        /// <summary>Path</summary>
        [JsonPropertyName("path")]
        public string Path { get; set; } = string.Empty;

        /// <summary>OperationId</summary>
        [JsonPropertyName("operationId")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? OperationId { get; set; }

        /// <summary>Code</summary>
        [JsonPropertyName("code")]
        public DocsJsonCode Code { get; set; } = new DocsJsonCode();
    }

    /// <summary>DocsJsonCode</summary>
    public class DocsJsonCode
    {
        /// <summary>Imports</summary>
        [JsonPropertyName("imports")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Imports { get; set; }

        /// <summary>WrapperStart</summary>
        [JsonPropertyName("wrapper_start")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? WrapperStart { get; set; }

        /// <summary>Snippet</summary>
        [JsonPropertyName("snippet")]
        public string Snippet { get; set; } = string.Empty;

        /// <summary>WrapperEnd</summary>
        [JsonPropertyName("wrapper_end")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? WrapperEnd { get; set; }
    }
}