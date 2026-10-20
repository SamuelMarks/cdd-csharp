// <copyright file="IrService.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Cdd.OpenApi.Common;
using Cdd.OpenApi.Models;
using Cdd.OpenApi.Models.Arazzo;

namespace Cdd.OpenApi.Ir
{
    /// <summary>
    /// Represents the top-level service contract in the Intermediate Representation.
    /// </summary>
    public class IrService
    {
        /// <summary>
        /// Gets or sets the OpenAPI specification version string (defaults to <c>"3.2.0"</c>).
        /// </summary>
        public string? OpenApiVersion { get; set; } = "3.2.0";

        /// <summary>
        /// Gets or sets the <c>$self</c> URI for the document.
        /// </summary>
        public string? Self { get; set; }

        /// <summary>
        /// Gets or sets the <c>jsonSchemaDialect</c> URI for the document.
        /// </summary>
        public string? JsonSchemaDialect { get; set; }

        /// <summary>
        /// Gets or sets the service title.
        /// </summary>
        public string Title { get; set; } = "API";

        /// <summary>
        /// Gets or sets the service version.
        /// </summary>
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// Gets or sets the short summary of the API.
        /// </summary>
        public string? Summary { get; set; }

        /// <summary>
        /// Gets or sets the full description of the API.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the terms of service URL.
        /// </summary>
        public string? TermsOfService { get; set; }

        /// <summary>
        /// Gets or sets the contact information for the API.
        /// </summary>
        public OpenApiContact? Contact { get; set; }

        /// <summary>
        /// Gets or sets the license information for the API.
        /// </summary>
        public OpenApiLicense? License { get; set; }

        /// <summary>
        /// Gets or sets the top-level server connectivity definitions.
        /// </summary>
        public IList<OpenApiServer>? Servers { get; set; }

        /// <summary>
        /// Gets or sets the Swagger 2.0 base path, if any.
        /// </summary>
        public string? BasePath { get; set; }

        /// <summary>
        /// Gets or sets the top-level webhooks definitions.
        /// </summary>
        public IDictionary<string, OpenApiPathItem>? Webhooks { get; set; }

        /// <summary>
        /// Gets or sets the non-schema reusable components (SecuritySchemes, Parameters, Responses, RequestBodies, Headers, Examples, Links, Callbacks, PathItems, MediaTypes).
        /// </summary>
        public OpenApiComponents? Components { get; set; }

        /// <summary>
        /// Gets or sets the collection of security schemes defined in the service.
        /// </summary>
        public List<IrSecurityScheme> SecuritySchemes { get; set; } = new List<IrSecurityScheme>();

        /// <summary>
        /// Gets or sets the document-level security requirements.
        /// </summary>
        public IList<IDictionary<string, IList<string>>>? Security { get; set; }

        /// <summary>
        /// Gets or sets the document-level tag definitions.
        /// </summary>
        public IList<OpenApiTag>? Tags { get; set; }

        /// <summary>
        /// Gets or sets the document-level external documentation.
        /// </summary>
        public OpenApiExternalDocs? ExternalDocs { get; set; }

        /// <summary>
        /// Gets or sets path-level metadata keyed by path template.
        /// </summary>
        public Dictionary<string, IrPathMetadata> PathMetadata { get; set; } = new Dictionary<string, IrPathMetadata>();

        /// <summary>
        /// Gets or sets a value indicating whether an empty <c>paths: {}</c> object was explicitly present on the source document.
        /// </summary>
        public bool HasEmptyPathsObject { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an empty <c>components.schemas: {}</c> object was explicitly present on the source document.
        /// </summary>
        public bool HasEmptySchemasObject { get; set; }

        /// <summary>
        /// Gets or sets the collection of models defined in the service.
        /// </summary>
        public List<IrModel> Models { get; set; } = new List<IrModel>();

        /// <summary>
        /// Gets or sets the collection of endpoints defined in the service.
        /// </summary>
        public List<IrEndpoint> Endpoints { get; set; } = new List<IrEndpoint>();

        /// <summary>
        /// Gets or sets the collection of Arazzo source descriptions referenced by the service.
        /// </summary>
        public List<IrSourceDescription> SourceDescriptions { get; set; } = new List<IrSourceDescription>();

        /// <summary>
        /// Gets or sets the collection of Arazzo workflows defined in the service.
        /// </summary>
        public List<IrWorkflow> Workflows { get; set; } = new List<IrWorkflow>();

        /// <summary>
        /// Gets or sets the reusable Arazzo components.
        /// </summary>
        public ArazzoComponents? ArazzoComponents { get; set; }

        /// <summary>
        /// Gets or sets top-level Arazzo specification extensions (<c>x-*</c>).
        /// </summary>
        public IDictionary<string, JsonElement>? ArazzoExtensions { get; set; }

        /// <summary>
        /// Gets or sets Arazzo info-level specification extensions (<c>x-*</c>).
        /// </summary>
        public IDictionary<string, JsonElement>? ArazzoInfoExtensions { get; set; }

        /// <summary>
        /// Gets or sets the Arazzo specification version string (defaults to <c>"1.1.0"</c>).
        /// </summary>
        public string ArazzoVersion { get; set; } = "1.1.0";

        /// <summary>
        /// Constructs an <see cref="IrService"/> from an <see cref="ArazzoDocument"/>.
        /// </summary>
        /// <param name="doc">The source Arazzo 1.1.0 document.</param>
        /// <returns>A populated <see cref="IrService"/> instance.</returns>
        public static IrService FromArazzo(ArazzoDocument doc)
        {
            var service = new IrService
            {
                ArazzoVersion = doc.Arazzo,
                Self = doc.Self,
                Title = doc.Info.Title,
                Version = doc.Info.Version,
                Summary = doc.Info.Summary,
                Description = doc.Info.Description,
                ArazzoInfoExtensions = doc.Info.Extensions,
                ArazzoComponents = doc.Components,
                ArazzoExtensions = doc.Extensions
            };

            foreach (var sd in doc.SourceDescriptions)
            {
                service.SourceDescriptions.Add(new IrSourceDescription
                {
                    Name = sd.Name,
                    Url = sd.Url,
                    Type = sd.Type,
                    Extensions = sd.Extensions
                });
            }

            foreach (var wf in doc.Workflows)
            {
                var irWf = new IrWorkflow
                {
                    WorkflowId = wf.WorkflowId,
                    Summary = wf.Summary,
                    Description = wf.Description,
                    Inputs = wf.Inputs,
                    DependsOn = wf.DependsOn != null ? wf.DependsOn.ToList() : new List<string>(),
                    SuccessActions = wf.SuccessActions,
                    FailureActions = wf.FailureActions,
                    Outputs = wf.Outputs,
                    Parameters = wf.Parameters,
                    Extensions = wf.Extensions
                };

                foreach (var step in wf.Steps)
                {
                    irWf.Steps.Add(new IrWorkflowStep
                    {
                        StepId = step.StepId,
                        Description = step.Description,
                        OperationId = step.OperationId,
                        OperationPath = step.OperationPath,
                        ChannelPath = step.ChannelPath,
                        WorkflowId = step.WorkflowId,
                        Parameters = step.Parameters,
                        RequestBody = step.RequestBody,
                        SuccessCriteria = step.SuccessCriteria,
                        OnSuccess = step.OnSuccess,
                        OnFailure = step.OnFailure,
                        Outputs = step.Outputs,
                        Timeout = step.Timeout,
                        CorrelationId = step.CorrelationId,
                        Action = step.Action,
                        DependsOn = step.DependsOn,
                        Extensions = step.Extensions
                    });
                }

                service.Workflows.Add(irWf);
            }

            return service;
        }

        /// <summary>
        /// Converts this <see cref="IrService"/> back into an <see cref="ArazzoDocument"/>.
        /// </summary>
        /// <returns>A synthesized <see cref="ArazzoDocument"/>.</returns>
        public ArazzoDocument ToArazzo()
        {
            var doc = new ArazzoDocument
            {
                Arazzo = ArazzoVersion,
                Self = Self,
                Info = new ArazzoInfo
                {
                    Title = Title,
                    Version = Version,
                    Summary = Summary,
                    Description = Description,
                    Extensions = ArazzoInfoExtensions
                },
                SourceDescriptions = SourceDescriptions.Count > 0
                    ? SourceDescriptions.Select(sd => new ArazzoSourceDescription
                    {
                        Name = sd.Name,
                        Url = sd.Url,
                        Type = sd.Type,
                        Extensions = sd.Extensions
                    }).ToList()
                    : new List<ArazzoSourceDescription>
                    {
                        new ArazzoSourceDescription { Name = "default", Url = "./openapi.json", Type = "openapi" }
                    },
                Workflows = Workflows.Select(wf => new ArazzoWorkflow
                {
                    WorkflowId = wf.WorkflowId,
                    Summary = wf.Summary,
                    Description = wf.Description,
                    Inputs = wf.Inputs,
                    DependsOn = wf.DependsOn.Count > 0 ? wf.DependsOn : null,
                    SuccessActions = wf.SuccessActions,
                    FailureActions = wf.FailureActions,
                    Outputs = wf.Outputs,
                    Parameters = wf.Parameters,
                    Extensions = wf.Extensions,
                    Steps = wf.Steps.Select(s => new ArazzoStep
                    {
                        StepId = s.StepId,
                        Description = s.Description,
                        OperationId = s.OperationId,
                        OperationPath = s.OperationPath,
                        ChannelPath = s.ChannelPath,
                        WorkflowId = s.WorkflowId,
                        Parameters = s.Parameters,
                        RequestBody = s.RequestBody,
                        SuccessCriteria = s.SuccessCriteria,
                        OnSuccess = s.OnSuccess,
                        OnFailure = s.OnFailure,
                        Outputs = s.Outputs,
                        Timeout = s.Timeout,
                        CorrelationId = s.CorrelationId,
                        Action = s.Action,
                        DependsOn = s.DependsOn,
                        Extensions = s.Extensions
                    }).ToList()
                }).ToList(),
                Components = ArazzoComponents,
                Extensions = ArazzoExtensions
            };

            return doc;
        }

        /// <summary>
        /// Constructs an <see cref="IrService"/> from an <see cref="OpenApiDocument"/>.
        /// </summary>
        /// <param name="doc">The source OpenAPI document.</param>
        /// <returns>A populated <see cref="IrService"/> instance.</returns>
        public static IrService FromOpenApi(OpenApiDocument doc)
        {
            var service = new IrService
            {
                OpenApiVersion = doc.OpenApi,
                Self = doc.Self,
                JsonSchemaDialect = doc.JsonSchemaDialect,
                Title = doc.Info?.Title ?? "API",
                Version = doc.Info?.Version ?? "1.0.0",
                Summary = doc.Info?.Summary,
                Description = doc.Info?.Description,
                TermsOfService = doc.Info?.TermsOfService,
                Contact = doc.Info?.Contact,
                License = doc.Info?.License,
                Servers = doc.Servers,
                BasePath = doc.BasePath,
                Webhooks = doc.Webhooks,
                Components = doc.Components,
                Security = doc.Security,
                Tags = doc.Tags,
                ExternalDocs = doc.ExternalDocs,
                HasEmptyPathsObject = doc.Paths != null && doc.Paths.Count == 0,
                HasEmptySchemasObject = doc.Components?.Schemas != null && doc.Components.Schemas.Count == 0
            };

            if (doc.Components?.SecuritySchemes != null)
            {
                foreach (var secKvp in doc.Components.SecuritySchemes)
                {
                    service.SecuritySchemes.Add(IrSecurityScheme.FromOpenApiSecurityScheme(secKvp.Key, secKvp.Value));
                }
            }

            if (doc.Components?.Schemas != null)
            {
                foreach (var kvp in doc.Components.Schemas)
                {
                    service.Models.Add(IrModel.FromOpenApiSchema(kvp.Key, kvp.Value));
                }
            }

            if (doc.Paths != null)
            {
                foreach (var pathKvp in doc.Paths)
                {
                    var pathItem = pathKvp.Value;
                    service.PathMetadata[pathKvp.Key] = new IrPathMetadata
                    {
                        Ref = pathItem.Ref,
                        Summary = pathItem.Summary,
                        Description = pathItem.Description,
                        Servers = pathItem.Servers,
                        Parameters = pathItem.Parameters
                    };

                    var ops = OperationHelpers.GetOperationsByPascalVerb(pathItem);
                    foreach (var opKvp in ops)
                    {
                        var op = opKvp.Value;
                        if (op == null) continue;

                        service.Endpoints.Add(IrEndpoint.FromOpenApiOperation(pathKvp.Key, opKvp.Key, op));
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
                OpenApi = OpenApiVersion,
                Self = Self,
                JsonSchemaDialect = JsonSchemaDialect,
                BasePath = BasePath,
                Info = new OpenApiInfo
                {
                    Title = Title,
                    Version = Version,
                    Summary = Summary,
                    Description = Description,
                    TermsOfService = TermsOfService,
                    Contact = Contact,
                    License = License
                },
                Servers = Servers,
                Webhooks = Webhooks,
                Security = Security,
                Tags = Tags,
                ExternalDocs = ExternalDocs
            };

            if (Models.Count > 0 || HasEmptySchemasObject || SecuritySchemes.Count > 0 || Components?.SecuritySchemes != null)
            {
                IDictionary<string, OpenApiSchema>? schemas = null;
                if (Models.Count > 0 || HasEmptySchemasObject)
                {
                    schemas = new Dictionary<string, OpenApiSchema>();
                    foreach (var model in Models)
                    {
                        schemas[model.Name] = model.ToOpenApiSchema();
                    }
                }

                doc.Components = Components ?? new OpenApiComponents();
                doc.Components.Schemas = schemas;
                if (SecuritySchemes.Count > 0 && doc.Components.SecuritySchemes == null)
                {
                    doc.Components.SecuritySchemes = SecuritySchemes.ToDictionary(s => s.SchemeName, s => s.ToOpenApiSecurityScheme());
                }
            }

            if (Endpoints.Count > 0 || PathMetadata.Count > 0 || HasEmptyPathsObject)
            {
                var paths = new OpenApiPaths();

                foreach (var metaKvp in PathMetadata)
                {
                    paths[metaKvp.Key] = new OpenApiPathItem
                    {
                        Ref = metaKvp.Value.Ref,
                        Summary = metaKvp.Value.Summary,
                        Description = metaKvp.Value.Description,
                        Servers = metaKvp.Value.Servers,
                        Parameters = metaKvp.Value.Parameters
                    };
                }

                foreach (var ep in Endpoints)
                {
                    if (!paths.TryGetValue(ep.Path, out var pathItem))
                    {
                        pathItem = new OpenApiPathItem();
                        paths[ep.Path] = pathItem;
                    }

                    OperationHelpers.SetOperation(pathItem, ep.HttpMethod, ep.ToOpenApiOperation());
                }

                doc.Paths = paths;
            }

            return doc;
        }
    }
}
