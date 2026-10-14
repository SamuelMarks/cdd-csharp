// <copyright file="CddException.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;

namespace Cdd.OpenApi.Errors
{
    /// <summary>
    /// Enumeration of error codes produced by the CDD OpenAPI toolchain.
    /// </summary>
    public enum CddErrorCode
    {
        /// <summary>
        /// An unspecified or general error occurred.
        /// </summary>
        GeneralError = 0,

        /// <summary>
        /// The provided OpenAPI JSON or YAML document is malformed.
        /// </summary>
        InvalidSpecification = 1,

        /// <summary>
        /// A required input file or directory was not found.
        /// </summary>
        InputNotFound = 2,

        /// <summary>
        /// An error occurred during C# AST parsing or code generation.
        /// </summary>
        CodeGenerationError = 3,

        /// <summary>
        /// An error occurred while communicating over HTTP or JSON-RPC.
        /// </summary>
        TransportError = 4,

        /// <summary>
        /// An I/O error occurred while reading or writing files or directories.
        /// </summary>
        IoError = 5,

        /// <summary>
        /// An error occurred while fetching an OpenAPI document over HTTP.
        /// </summary>
        HttpFetchError = 6
    }

    /// <summary>
    /// Represents a strongly-typed domain exception thrown by the CDD OpenAPI toolchain.
    /// </summary>
    public class CddException : Exception
    {
        /// <summary>
        /// Gets the strongly-typed <see cref="CddErrorCode"/> categorizing this failure.
        /// </summary>
        public CddErrorCode ErrorCode { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CddException"/> class with an error code and message.
        /// </summary>
        /// <param name="errorCode">The structured error code.</param>
        /// <param name="message">The human-readable error message.</param>
        public CddException(CddErrorCode errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CddException"/> class with an error code, message, and inner exception.
        /// </summary>
        /// <param name="errorCode">The structured error code.</param>
        /// <param name="message">The human-readable error message.</param>
        /// <param name="innerException">The underlying exception that caused this error.</param>
        public CddException(CddErrorCode errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
