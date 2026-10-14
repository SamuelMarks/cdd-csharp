// <copyright file="IrModels.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Cdd.OpenApi.Common;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Ir
{
    /// <summary>
    /// Represents a strongly-typed Intermediate Representation (IR) of a type reference.
    /// </summary>
    public class IrTypeRef
    {
        /// <summary>
        /// Gets or sets the OpenAPI type name (e.g., "integer", "string", "array", "object").
        /// </summary>
        public string TypeName { get; set; } = "string";

        /// <summary>
        /// Gets or sets the C# type name (e.g., "int", "string", "List&lt;string&gt;").
        /// </summary>
        public string CSharpType { get; set; } = "string";

        /// <summary>
        /// Gets or sets a value indicating whether the type reference is nullable.
        /// </summary>
        public bool IsNullable { get; set; }
    }

    /// <summary>
    /// Represents a field or property on an <see cref="IrModel"/>.
    /// </summary>
    public class IrField
    {
        /// <summary>
        /// Gets or sets the property name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the field's resolved type reference.
        /// </summary>
        public IrTypeRef Type { get; set; } = new IrTypeRef();

        /// <summary>
        /// Gets or sets a value indicating whether the field is required.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets the human-readable description of the field.
        /// </summary>
        public string? Description { get; set; }
    }

    /// <summary>
    /// Represents a data model (schema/class) in the Intermediate Representation.
    /// </summary>
    public class IrModel
    {
        /// <summary>
        /// Gets or sets the name of the model.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the model.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the list of fields belonging to the model.
        /// </summary>
        public List<IrField> Fields { get; set; } = new List<IrField>();
    }

    /// <summary>
    /// Represents an operation parameter in the Intermediate Representation.
    /// </summary>
    public class IrParameter
    {
        /// <summary>
        /// Gets or sets the parameter name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the parameter location ("path", "query", "header", "cookie").
        /// </summary>
        public string Location { get; set; } = "query";

        /// <summary>
        /// Gets or sets a value indicating whether the parameter is required.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets the parameter's type reference.
        /// </summary>
        public IrTypeRef Type { get; set; } = new IrTypeRef();
    }

    /// <summary>
    /// Represents an HTTP response in the Intermediate Representation.
    /// </summary>
    public class IrResponse
    {
        /// <summary>
        /// Gets or sets the HTTP status code key (e.g., "200", "default").
        /// </summary>
        public string StatusCode { get; set; } = "200";

        /// <summary>
        /// Gets or sets the response description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the response payload type reference, if any.
        /// </summary>
        public IrTypeRef? ContentType { get; set; }
    }

    /// <summary>
    /// Represents a symmetric HTTP endpoint in the Intermediate Representation.
    /// </summary>
    public class IrEndpoint
    {
        /// <summary>
        /// Gets or sets the route path template (e.g., "/pets/{id}").
        /// </summary>
        public string Path { get; set; } = "/";

        /// <summary>
        /// Gets or sets the HTTP verb (e.g., "Get", "Post").
        /// </summary>
        public string HttpMethod { get; set; } = "Get";

        /// <summary>
        /// Gets or sets the unique operation identifier.
        /// </summary>
        public string OperationId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the operation summary.
        /// </summary>
        public string? Summary { get; set; }

        /// <summary>
        /// Gets or sets the list of endpoint parameters.
        /// </summary>
        public List<IrParameter> Parameters { get; set; } = new List<IrParameter>();

        /// <summary>
        /// Gets or sets the list of endpoint responses.
        /// </summary>
        public List<IrResponse> Responses { get; set; } = new List<IrResponse>();
    }

    /// <summary>
    /// Represents the top-level service contract in the Intermediate Representation.
    /// </summary>
    public class IrService
    {
        /// <summary>
        /// Gets or sets the service title.
        /// </summary>
        public string Title { get; set; } = "API";

        /// <summary>
        /// Gets or sets the service version.
        /// </summary>
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// Gets or sets the collection of models defined in the service.
        /// </summary>
        public List<IrModel> Models { get; set; } = new List<IrModel>();

        /// <summary>
        /// Gets or sets the collection of endpoints defined in the service.
        /// </summary>
        public List<IrEndpoint> Endpoints { get; set; } = new List<IrEndpoint>();

        /// <summary>
        /// Constructs an <see cref="IrService"/> from an <see cref="OpenApiDocument"/>.
        /// </summary>
        /// <param name="doc">The source OpenAPI document.</param>
        /// <returns>A populated <see cref="IrService"/> instance.</returns>
        public static IrService FromOpenApi(OpenApiDocument doc)
        {
            var service = new IrService
            {
                Title = doc.Info?.Title ?? "API",
                Version = doc.Info?.Version ?? "1.0.0"
            };

            if (doc.Components?.Schemas != null)
            {
                foreach (var kvp in doc.Components.Schemas)
                {
                    var schema = kvp.Value;
                    var irModel = new IrModel
                    {
                        Name = kvp.Key,
                        Description = schema.Description
                    };

                    if (schema.Properties != null)
                    {
                        foreach (var propKvp in schema.Properties)
                        {
                            var isReq = schema.Required != null && schema.Required.Contains(propKvp.Key);
                            var openApiType = propKvp.Value.Type ?? "object";
                            irModel.Fields.Add(new IrField
                            {
                                Name = propKvp.Key,
                                IsRequired = isReq,
                                Description = propKvp.Value.Description,
                                Type = new IrTypeRef
                                {
                                    TypeName = openApiType,
                                    CSharpType = TypeMapper.MapOpenApiToCSharp(openApiType, "object"),
                                    IsNullable = !isReq
                                }
                            });
                        }
                    }

                    service.Models.Add(irModel);
                }
            }

            if (doc.Paths != null)
            {
                foreach (var pathKvp in doc.Paths)
                {
                    var ops = OperationHelpers.GetOperationsByPascalVerb(pathKvp.Value);
                    foreach (var opKvp in ops)
                    {
                        var op = opKvp.Value;
                        if (op == null) continue;

                        var endpoint = new IrEndpoint
                        {
                            Path = pathKvp.Key,
                            HttpMethod = opKvp.Key,
                            OperationId = op.OperationId ?? $"{opKvp.Key}{pathKvp.Key.Replace("/", "").Replace("{", "").Replace("}", "")}",
                            Summary = op.Summary
                        };

                        if (op.Parameters != null)
                        {
                            foreach (var p in op.Parameters)
                            {
                                var pType = p.Schema?.Type ?? p.Type ?? "string";
                                endpoint.Parameters.Add(new IrParameter
                                {
                                    Name = p.Name,
                                    Location = p.In,
                                    IsRequired = p.Required == true,
                                    Type = new IrTypeRef
                                    {
                                        TypeName = pType,
                                        CSharpType = TypeMapper.MapOpenApiToCSharp(pType, "string"),
                                        IsNullable = p.Required != true
                                    }
                                });
                            }
                        }

                        if (op.Responses != null)
                        {
                            foreach (var rKvp in op.Responses)
                            {
                                IrTypeRef? contentType = null;
                                if (rKvp.Value.Content != null && rKvp.Value.Content.TryGetValue("application/json", out var mediaType) && mediaType.Schema != null)
                                {
                                    var rType = mediaType.Schema.Type ?? "object";
                                    contentType = new IrTypeRef
                                    {
                                        TypeName = rType,
                                        CSharpType = TypeMapper.MapOpenApiToCSharp(rType, "object")
                                    };
                                }

                                endpoint.Responses.Add(new IrResponse
                                {
                                    StatusCode = rKvp.Key,
                                    Description = rKvp.Value.Description ?? "Response",
                                    ContentType = contentType
                                });
                            }
                        }

                        service.Endpoints.Add(endpoint);
                    }
                }
            }

            return service;
        }

        /// <summary>
        /// Converts this <see cref="IrService"/> instance back into an <see cref="OpenApiDocument"/>.
        /// </summary>
        /// <returns>The reconstructed <see cref="OpenApiDocument"/>.</returns>
        public OpenApiDocument ToOpenApi()
        {
            var doc = new OpenApiDocument
            {
                Info = new OpenApiInfo
                {
                    Title = Title,
                    Version = Version
                }
            };

            if (Models.Count > 0)
            {
                var schemas = new Dictionary<string, OpenApiSchema>();
                foreach (var model in Models)
                {
                    var props = new Dictionary<string, OpenApiSchema>();
                    var reqs = new List<string>();
                    foreach (var field in model.Fields)
                    {
                        props[field.Name] = new OpenApiSchema
                        {
                            Type = field.Type.TypeName,
                            Description = field.Description
                        };
                        if (field.IsRequired)
                        {
                            reqs.Add(field.Name);
                        }
                    }

                    schemas[model.Name] = new OpenApiSchema
                    {
                        Type = "object",
                        Description = model.Description,
                        Properties = props.Count > 0 ? props : null,
                        Required = reqs.Count > 0 ? reqs : null
                    };
                }

                doc.Components = new OpenApiComponents { Schemas = schemas };
            }

            if (Endpoints.Count > 0)
            {
                var paths = new OpenApiPaths();
                foreach (var ep in Endpoints)
                {
                    if (!paths.TryGetValue(ep.Path, out var pathItem))
                    {
                        pathItem = new OpenApiPathItem();
                        paths[ep.Path] = pathItem;
                    }

                    var op = new OpenApiOperation
                    {
                        OperationId = ep.OperationId,
                        Summary = ep.Summary
                    };

                    if (ep.Parameters.Count > 0)
                    {
                        op.Parameters = ep.Parameters.Select(p => new OpenApiParameter
                        {
                            Name = p.Name,
                            In = p.Location,
                            Required = p.IsRequired,
                            Schema = new OpenApiSchema { Type = p.Type.TypeName }
                        }).ToList();
                    }

                    if (ep.Responses.Count > 0)
                    {
                        var responses = new OpenApiResponses();
                        foreach (var r in ep.Responses)
                        {
                            var oasResp = new OpenApiResponse { Description = r.Description };
                            if (r.ContentType != null)
                            {
                                oasResp.Content = new Dictionary<string, OpenApiMediaType>
                                {
                                    ["application/json"] = new OpenApiMediaType
                                    {
                                        Schema = new OpenApiSchema { Type = r.ContentType.TypeName }
                                    }
                                };
                            }
                            responses[r.StatusCode] = oasResp;
                        }
                        op.Responses = responses;
                    }

                    if (ep.HttpMethod == "Post") pathItem.Post = op;
                    else if (ep.HttpMethod == "Put") pathItem.Put = op;
                    else if (ep.HttpMethod == "Delete") pathItem.Delete = op;
                    else pathItem.Get = op;
                }

                doc.Paths = paths;
            }

            return doc;
        }
    }
}
