# Compliance with OpenAPI 3.2.0

The `Cdd.OpenApi` library aims for rigorous adherence to the [OpenAPI Specification 3.2.0](https://spec.openapis.org/oas/v3.1.0) (currently utilizing 3.1 features styled towards 3.2 directions).

## Implemented Objects

The following objects are fully modeled in C# and support complete JSON round-trip Serialization/Deserialization. Their properties map exactly to the specification's expectations, including case sensitivity.

*   `OpenApiDocument` (Root)
*   `OpenApiInfo`
*   `OpenApiContact`
*   `OpenApiLicense`
*   `OpenApiServer`
*   `OpenApiServerVariable`
*   `OpenApiComponents`
*   `OpenApiPaths` (Map of Strings to Path Items)
*   `OpenApiPathItem`
*   `OpenApiOperation`
*   `OpenApiParameter`
*   `OpenApiRequestBody`
*   `OpenApiMediaType`
*   `OpenApiEncoding`
*   `OpenApiResponses`
*   `OpenApiResponse`
*   `OpenApiSchema`
*   `OpenApiExample`
*   `OpenApiReference`
*   `OpenApiSecurityScheme`
*   `OpenApiOAuthFlows`
*   `OpenApiOAuthFlow`
*   `OpenApiLink`
*   `OpenApiCallback`
*   `OpenApiTag`
*   `OpenApiExternalDocs`

## Type Mapping (C# to OpenAPI)
We map native C# primitives to OpenAPI types automatically:
*   `int`, `long`, `short` ➔ `type: "integer"`
*   `double`, `float`, `decimal` ➔ `type: "number"`
*   `bool` ➔ `type: "boolean"`
*   `string` ➔ `type: "string"`

Nullable types in C# (e.g. `int?`) translate to omitted elements in the `required` array of the `OpenApiSchema`. 

## C# Syntax Supported

*   **Models**: Standard POCO classes with `public Type Name { get; set; }` properties.
*   **Controllers/Routes**: Classes with methods decorated with ASP.NET Core style routing attributes (e.g., `[HttpGet("/path/{param}")]`).
*   **Documentation**: `/// <summary>` XML documentation blocks are extracted and emitted as OpenAPI `description` and `summary` fields.

## Future Compliance Targets
To reach 100% full implementation of 3.2.0 edge cases, future updates will need to cover:
1. Deep nested serialization of generic `object` elements and extension fields (`x-*`).
2. Robust deep-merging of `$ref` documents.
3. Callbacks and Webhooks translation to C# delegates/events.