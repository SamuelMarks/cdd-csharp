# Swagger 2.0 and OpenAPI 3.2.0 Compliance

This document tracks `cdd-csharp`'s compliance with the Swagger 2.0 and OpenAPI 3.2.0 specification.

Currently, `cdd-csharp` is largely compliant with OpenAPI 3.0.x and 3.1.x, as well as providing baseline compatibility for the initial features defined in the working draft of Swagger 2.0 and OpenAPI 3.2.0.

## Support Matrix

| Specification Concept       | Status |
| --------------------------- | ------ |
| Info Object                 | ✅     |
| Server Object               | ✅     |
| Components Object           | ✅     |
| Paths Object                | ✅     |
| Path Item Object            | ✅     |
| Operation Object            | ✅     |
| Parameter Object            | ✅     |
| Request Body Object         | ✅     |
| Responses Object            | ✅     |
| Responses / Schema Object   | ✅     |
| Reference Object (`$ref`)   | ✅     |
| OAuth Flows                 | ✅     |
| Security Requirement Object | ✅     |
| Links / Callbacks           | ✅     |
| Webhooks                    | ✅     |

### Work in Progress (Swagger 2.0 and OpenAPI 3.2.0 Draft)
- Support for `moonjelly` schemas and recursive subschemas are being reviewed for 3.2.0.

Please open an issue for any compliance gaps.