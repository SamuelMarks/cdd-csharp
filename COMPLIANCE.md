# Specification Compliance (`cdd-csharp`)

This document tracks `cdd-csharp`'s compliance with **OpenAPI 3.2.0**, **Swagger 2.0**, **Arazzo 1.1.0**, and **Model Context Protocol (MCP) 1.0.0**.

Detailed target-by-target conformance matrices are available under the [`conformance/`](conformance/) directory:
- [OpenAPI 3.2.0 (`conformance/openapi-3.2.0/`)](conformance/openapi-3.2.0/)
- [Swagger 2.0 (`conformance/swagger-2.0/`)](conformance/swagger-2.0/)
- [Arazzo 1.1.0 (`conformance/arazzo-1.1.0/`)](conformance/arazzo-1.1.0/)
- [MCP 1.0.0 (`conformance/mcp-1.0.0/`)](conformance/mcp-1.0.0/)

## Support Matrix

| Specification Concept                           | Status |
| ----------------------------------------------- | ------ |
| OpenAPI 3.2.0 Info / Server / Tag Objects       | ✅     |
| OpenAPI 3.2.0 Components & `$ref` Resolution    | ✅     |
| OpenAPI 3.2.0 Paths, PathItem & AdditionalOps   | ✅     |
| OpenAPI 3.2.0 Operation, Parameter & RequestBody| ✅     |
| OpenAPI 3.2.0 Responses, Headers & MediaTypes   | ✅     |
| OpenAPI 3.2.0 JSON Schema (`oneOf`/`anyOf`/`allOf`/discriminators) | ✅ |
| OpenAPI 3.2.0 Security Schemes & OAuth2 Flows   | ✅     |
| OpenAPI 3.2.0 Links, Callbacks & Webhooks       | ✅     |
| Swagger 2.0 Automatic Upgrade & Bidirectional Sync | ✅  |
| Arazzo 1.1.0 SourceDescriptions & Workflows     | ✅     |
| Arazzo 1.1.0 Step DAGs, Runtime Expressions & Criteria | ✅ |
| Arazzo 1.1.0 Success & Failure Actions (`goto`/`retry`/`end`) | ✅ |
| Model Context Protocol (MCP) Stdio Server & Tool/Resource Generation | ✅ |

Please open an issue for any compliance gaps.
