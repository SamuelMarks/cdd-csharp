// <copyright file="IrModels.cs" company="Cdd.OpenApi">
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
    /// Represents a source specification binding in the Intermediate Representation.
    /// </summary>
    public class IrSourceDescription
    {
        /// <summary>
        /// Gets or sets the unique name of the source description.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the URL or relative reference to the source specification.
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the specification protocol type (<c>"openapi"</c>, <c>"asyncapi"</c>, or <c>"arazzo"</c>).
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets specification extensions (<c>x-*</c>).
        /// </summary>
        public IDictionary<string, JsonElement>? Extensions { get; set; }
    }

    /// <summary>
    /// Represents a single workflow step in the Intermediate Representation.
    /// </summary>
    public class IrWorkflowStep
    {
        /// <summary>
        /// Gets or sets the unique step identifier within its workflow.
        /// </summary>
        public string StepId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the human-readable description of the step.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the target operationId if invoking an OpenAPI operation by ID.
        /// </summary>
        public string? OperationId { get; set; }

        /// <summary>
        /// Gets or sets the target operationPath if invoking an OpenAPI operation by JSON Pointer path.
        /// </summary>
        public string? OperationPath { get; set; }

        /// <summary>
        /// Gets or sets the target channelPath if invoking an AsyncAPI channel.
        /// </summary>
        public string? ChannelPath { get; set; }

        /// <summary>
        /// Gets or sets the target workflowId if invoking a sub-workflow.
        /// </summary>
        public string? WorkflowId { get; set; }

        /// <summary>
        /// Gets or sets the list of step parameters.
        /// </summary>
        public IList<ArazzoParameter>? Parameters { get; set; }

        /// <summary>
        /// Gets or sets the step request body definition.
        /// </summary>
        public ArazzoRequestBody? RequestBody { get; set; }

        /// <summary>
        /// Gets or sets the step success criteria.
        /// </summary>
        public IList<ArazzoCriterion>? SuccessCriteria { get; set; }

        /// <summary>
        /// Gets or sets the step onSuccess action list.
        /// </summary>
        public IList<ArazzoSuccessAction>? OnSuccess { get; set; }

        /// <summary>
        /// Gets or sets the step onFailure action list.
        /// </summary>
        public IList<ArazzoFailureAction>? OnFailure { get; set; }

        /// <summary>
        /// Gets or sets the step output expressions map.
        /// </summary>
        public IDictionary<string, ArazzoStepOutput>? Outputs { get; set; }

        /// <summary>
        /// Gets or sets the per-step execution timeout in milliseconds (Arazzo 1.1.0).
        /// </summary>
        public int? Timeout { get; set; }

        /// <summary>
        /// Gets or sets the correlation ID runtime expression or header reference (Arazzo 1.1.0).
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Gets or sets the asynchronous message flow intent (<c>"send"</c> or <c>"receive"</c>).
        /// </summary>
        public string? Action { get; set; }

        /// <summary>
        /// Gets or sets the prerequisite step identifiers within the enclosing workflow.
        /// </summary>
        public IList<string>? DependsOn { get; set; }

        /// <summary>
        /// Gets or sets specification extensions (<c>x-*</c>).
        /// </summary>
        public IDictionary<string, JsonElement>? Extensions { get; set; }
    }

    /// <summary>
    /// Represents an executable Arazzo workflow in the Intermediate Representation.
    /// </summary>
    public class IrWorkflow
    {
        /// <summary>
        /// Gets or sets the unique workflow identifier.
        /// </summary>
        public string WorkflowId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the workflow summary.
        /// </summary>
        public string? Summary { get; set; }

        /// <summary>
        /// Gets or sets the workflow description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the JSON Schema describing the workflow inputs.
        /// </summary>
        public OpenApiSchema? Inputs { get; set; }

        /// <summary>
        /// Gets or sets the prerequisite workflow identifiers.
        /// </summary>
        public List<string> DependsOn { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the ordered list of steps in the workflow.
        /// </summary>
        public List<IrWorkflowStep> Steps { get; set; } = new List<IrWorkflowStep>();

        /// <summary>
        /// Gets or sets workflow-level success actions.
        /// </summary>
        public IList<ArazzoSuccessAction>? SuccessActions { get; set; }

        /// <summary>
        /// Gets or sets workflow-level failure actions.
        /// </summary>
        public IList<ArazzoFailureAction>? FailureActions { get; set; }

        /// <summary>
        /// Gets or sets the workflow output expressions map.
        /// </summary>
        public IDictionary<string, string>? Outputs { get; set; }

        /// <summary>
        /// Gets or sets workflow-level default parameters.
        /// </summary>
        public IList<ArazzoParameter>? Parameters { get; set; }

        /// <summary>
        /// Gets or sets specification extensions (<c>x-*</c>).
        /// </summary>
        public IDictionary<string, JsonElement>? Extensions { get; set; }
    }

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
        /// Gets or sets the OpenAPI format modifier (e.g., "int64", "uuid", "date-time").
        /// </summary>
        public string? Format { get; set; }

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

        /// <summary>
        /// Gets or sets the full underlying <see cref="OpenApiSchema"/> for lossless schema metadata preservation.
        /// </summary>
        public OpenApiSchema? Schema { get; set; }
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

        /// <summary>
        /// Gets or sets the full underlying <see cref="OpenApiSchema"/> for lossless schema metadata (XML, Discriminator, JSON Schema 2020-12 keywords).
        /// </summary>
        public OpenApiSchema? Schema { get; set; }
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

        /// <summary>
        /// Gets or sets the parameter description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the parameter is deprecated.
        /// </summary>
        public bool? Deprecated { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether empty values are allowed.
        /// </summary>
        public bool? AllowEmptyValue { get; set; }

        /// <summary>
        /// Gets or sets the serialization style of the parameter.
        /// </summary>
        public string? Style { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether array/object values generate separate parameters.
        /// </summary>
        public bool? Explode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether reserved characters are allowed without percent-encoding.
        /// </summary>
        public bool? AllowReserved { get; set; }

        /// <summary>
        /// Gets or sets the parameter example value.
        /// </summary>
        public OpenApiAny? Example { get; set; }

        /// <summary>
        /// Gets or sets the map of named parameter examples.
        /// </summary>
        public IDictionary<string, OpenApiExample>? Examples { get; set; }

        /// <summary>
        /// Gets or sets the map of media types for complex parameter serialization.
        /// </summary>
        public IDictionary<string, OpenApiMediaType>? Content { get; set; }

        /// <summary>
        /// Gets or sets the full parameter schema.
        /// </summary>
        public OpenApiSchema? Schema { get; set; }
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
        public string? Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the response payload type reference, if any.
        /// </summary>
        public IrTypeRef? ContentType { get; set; }

        /// <summary>
        /// Gets or sets the response headers map.
        /// </summary>
        public IDictionary<string, OpenApiHeader>? Headers { get; set; }

        /// <summary>
        /// Gets or sets the full media-type content map for the response.
        /// </summary>
        public IDictionary<string, OpenApiMediaType>? Content { get; set; }

        /// <summary>
        /// Gets or sets the response links map.
        /// </summary>
        public IDictionary<string, OpenApiLink>? Links { get; set; }
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
        /// Gets or sets a value indicating whether the source operation explicitly specified an <c>operationId</c>.
        /// </summary>
        public bool HasExplicitOperationId { get; set; } = true;

        /// <summary>
        /// Gets or sets the operation summary.
        /// </summary>
        public string? Summary { get; set; }

        /// <summary>
        /// Gets or sets the operation description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the operation tags.
        /// </summary>
        public IList<string>? Tags { get; set; }

        /// <summary>
        /// Gets or sets the operation external documentation.
        /// </summary>
        public OpenApiExternalDocs? ExternalDocs { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the operation is deprecated.
        /// </summary>
        public bool? Deprecated { get; set; }

        /// <summary>
        /// Gets or sets the operation request body.
        /// </summary>
        public OpenApiRequestBody? RequestBody { get; set; }

        /// <summary>
        /// Gets or sets the operation callbacks.
        /// </summary>
        public IDictionary<string, OpenApiCallback>? Callbacks { get; set; }

        /// <summary>
        /// Gets or sets the operation-specific security requirements.
        /// </summary>
        public IList<IDictionary<string, IList<string>>>? Security { get; set; }

        /// <summary>
        /// Gets or sets the operation-specific server overrides.
        /// </summary>
        public IList<OpenApiServer>? Servers { get; set; }

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
    /// Represents path-item level metadata in the Intermediate Representation.
    /// </summary>
    public class IrPathMetadata
    {
        /// <summary>Gets or sets the path-level <c>$ref</c>.</summary>
        public string? Ref { get; set; }

        /// <summary>Gets or sets the path-level summary.</summary>
        public string? Summary { get; set; }

        /// <summary>Gets or sets the path-level description.</summary>
        public string? Description { get; set; }

        /// <summary>Gets or sets the path-level servers.</summary>
        public IList<OpenApiServer>? Servers { get; set; }

        /// <summary>Gets or sets the path-level shared parameters.</summary>
        public IList<OpenApiParameter>? Parameters { get; set; }
    }

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
        /// Gets or sets the top-level webhooks definitions.
        /// </summary>
        public IDictionary<string, OpenApiPathItem>? Webhooks { get; set; }

        /// <summary>
        /// Gets or sets the non-schema reusable components (SecuritySchemes, Parameters, Responses, RequestBodies, Headers, Examples, Links, Callbacks, PathItems, MediaTypes).
        /// </summary>
        public OpenApiComponents? Components { get; set; }

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
                Webhooks = doc.Webhooks,
                Components = doc.Components,
                Security = doc.Security,
                Tags = doc.Tags,
                ExternalDocs = doc.ExternalDocs,
                HasEmptyPathsObject = doc.Paths != null && doc.Paths.Count == 0,
                HasEmptySchemasObject = doc.Components?.Schemas != null && doc.Components.Schemas.Count == 0
            };

            if (doc.Components?.Schemas != null)
            {
                foreach (var kvp in doc.Components.Schemas)
                {
                    var schema = kvp.Value;
                    var irModel = new IrModel
                    {
                        Name = kvp.Key,
                        Description = schema.Description,
                        Schema = schema
                    };

                    if (schema.Properties != null)
                    {
                        foreach (var propKvp in schema.Properties)
                        {
                            var isReq = schema.Required != null && schema.Required.Contains(propKvp.Key);
                            var openApiType = propKvp.Value.Type ?? OpenApiDataType.Object;
                            irModel.Fields.Add(new IrField
                            {
                                Name = propKvp.Key,
                                IsRequired = isReq,
                                Description = propKvp.Value.Description,
                                Schema = propKvp.Value,
                                Type = new IrTypeRef
                                {
                                    TypeName = openApiType,
                                    Format = propKvp.Value.Format,
                                    CSharpType = TypeMapper.MapSchemaToCSharp(propKvp.Value, "object"),
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

                        var endpoint = new IrEndpoint
                        {
                            Path = pathKvp.Key,
                            HttpMethod = opKvp.Key,
                            HasExplicitOperationId = op.OperationId != null,
                            OperationId = op.OperationId ?? $"{opKvp.Key}{pathKvp.Key.Replace("/", "").Replace("{", "").Replace("}", "")}",
                            Summary = op.Summary,
                            Description = op.Description,
                            Tags = op.Tags,
                            ExternalDocs = op.ExternalDocs,
                            Deprecated = op.Deprecated,
                            RequestBody = op.RequestBody,
                            Callbacks = op.Callbacks,
                            Security = op.Security,
                            Servers = op.Servers
                        };

                        if (op.Parameters != null)
                        {
                            foreach (var p in op.Parameters)
                            {
                                var pType = p.Schema?.Type ?? p.Type ?? OpenApiDataType.String;
                                endpoint.Parameters.Add(new IrParameter
                                {
                                    Name = p.Name,
                                    Location = p.In,
                                    IsRequired = p.Required == true,
                                    Description = p.Description,
                                    Deprecated = p.Deprecated,
                                    AllowEmptyValue = p.AllowEmptyValue,
                                    Style = p.Style,
                                    Explode = p.Explode,
                                    AllowReserved = p.AllowReserved,
                                    Example = p.Example,
                                    Examples = p.Examples,
                                    Content = p.Content,
                                    Schema = p.Schema,
                                    Type = new IrTypeRef
                                    {
                                        TypeName = pType,
                                        Format = p.Schema?.Format ?? p.Format,
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
                                    var rType = mediaType.Schema.Type ?? OpenApiDataType.Object;
                                    contentType = new IrTypeRef
                                    {
                                        TypeName = rType,
                                        Format = mediaType.Schema.Format,
                                        CSharpType = TypeMapper.MapOpenApiToCSharp(rType, "object")
                                    };
                                }

                                endpoint.Responses.Add(new IrResponse
                                {
                                    StatusCode = rKvp.Key,
                                    Description = rKvp.Value.Description,
                                    Headers = rKvp.Value.Headers,
                                    Content = rKvp.Value.Content,
                                    Links = rKvp.Value.Links,
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
                OpenApi = OpenApiVersion,
                Self = Self,
                JsonSchemaDialect = JsonSchemaDialect,
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

            if (Models.Count > 0 || HasEmptySchemasObject || Components?.SecuritySchemes != null)
            {
                IDictionary<string, OpenApiSchema>? schemas = null;
                if (Models.Count > 0 || HasEmptySchemasObject)
                {
                    schemas = new Dictionary<string, OpenApiSchema>();
                    foreach (var model in Models)
                    {
                        var props = new Dictionary<string, OpenApiSchema>();
                        var reqs = new List<string>();
                        foreach (var field in model.Fields)
                        {
                            var fieldSchema = field.Schema ?? new OpenApiSchema();
                            fieldSchema.Type = field.Type.TypeName;
                            fieldSchema.Format = field.Type.Format;
                            fieldSchema.Description = field.Description;
                            props[field.Name] = fieldSchema;

                            if (field.IsRequired)
                            {
                                reqs.Add(field.Name);
                            }
                        }

                        var modelSchema = model.Schema ?? new OpenApiSchema { Type = OpenApiDataType.Object };
                        modelSchema.Description = model.Description;
                        if (props.Count > 0) modelSchema.Properties = props;
                        if (reqs.Count > 0) modelSchema.Required = reqs;
                        schemas[model.Name] = modelSchema;
                    }
                }

                doc.Components = Components ?? new OpenApiComponents();
                doc.Components.Schemas = schemas;
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

                    var op = new OpenApiOperation
                    {
                        OperationId = ep.HasExplicitOperationId ? ep.OperationId : null,
                        Summary = ep.Summary,
                        Description = ep.Description,
                        Tags = ep.Tags,
                        ExternalDocs = ep.ExternalDocs,
                        Deprecated = ep.Deprecated,
                        RequestBody = ep.RequestBody,
                        Callbacks = ep.Callbacks,
                        Security = ep.Security,
                        Servers = ep.Servers
                    };

                    if (ep.Parameters.Count > 0)
                    {
                        op.Parameters = ep.Parameters.Select(p => new OpenApiParameter
                        {
                            Name = p.Name,
                            In = p.Location,
                            Required = p.IsRequired,
                            Description = p.Description,
                            Deprecated = p.Deprecated,
                            AllowEmptyValue = p.AllowEmptyValue,
                            Style = p.Style,
                            Explode = p.Explode,
                            AllowReserved = p.AllowReserved,
                            Example = p.Example,
                            Examples = p.Examples,
                            Content = p.Content,
                            Schema = p.Schema
                        }).ToList();
                    }

                    if (ep.Responses.Count > 0)
                    {
                        var responses = new OpenApiResponses();
                        foreach (var r in ep.Responses)
                        {
                            responses[r.StatusCode] = new OpenApiResponse
                            {
                                Description = r.Description!,
                                Headers = r.Headers,
                                Links = r.Links,
                                Content = r.Content
                            };
                        }
                        op.Responses = responses;
                    }

                    OperationHelpers.SetOperation(pathItem, ep.HttpMethod, op);
                }

                doc.Paths = paths;
            }

            return doc;
        }
    }
}
