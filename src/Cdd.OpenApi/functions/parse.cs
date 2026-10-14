// <copyright file="parse.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Cdd.OpenApi.Common;
using Cdd.OpenApi.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cdd.OpenApi.Functions
{
    /// <summary>
    /// Parses C# method declarations into OpenAPI operation definitions.
    /// </summary>
    public static class Parse
    {
        /// <summary>
        /// Converts a <see cref="MethodDeclarationSyntax"/> into an <see cref="OpenApiOperation"/>.
        /// </summary>
        /// <param name="method">The Roslyn method syntax node to inspect.</param>
        /// <returns>The parsed <see cref="OpenApiOperation"/>.</returns>
        public static OpenApiOperation ParseFunction(MethodDeclarationSyntax method)
        {
            var operation = new OpenApiOperation
            {
                OperationId = method.Identifier.Text,
                Summary = Docstrings.Parse.GetSummary(method)
            };

            if (method.ParameterList.Parameters.Any())
            {
                var parameters = new List<OpenApiParameter>();
                foreach (var param in method.ParameterList.Parameters)
                {
                    var typeStr = param.Type != null ? WasmSafeFormatter.Format(param.Type) : "string";
                    parameters.Add(new OpenApiParameter
                    {
                        Name = param.Identifier.Text,
                        In = "query",
                        Required = param.Default == null,
                        Schema = new OpenApiSchema
                        {
                            Type = TypeMapper.MapCSharpToOpenApi(typeStr, "string")
                        }
                    });
                }
                operation.Parameters = parameters;
            }

            var returnTypeStr = WasmSafeFormatter.Format(method.ReturnType);
            if (returnTypeStr != "void" && returnTypeStr != "Task" && returnTypeStr != "System.Threading.Tasks.Task")
            {
                operation.Responses = new OpenApiResponses
                {
                    ["200"] = new OpenApiResponse
                    {
                        Description = "Success",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = new OpenApiMediaType
                            {
                                Schema = new OpenApiSchema
                                {
                                    Type = TypeMapper.MapCSharpToOpenApi(returnTypeStr, "object")
                                }
                            }
                        }
                    }
                };
            }

            return operation;
        }
    }
}
