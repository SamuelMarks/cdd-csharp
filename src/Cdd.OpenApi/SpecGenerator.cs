// <copyright file="SpecGenerator.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Cdd.OpenApi.Common;
using Cdd.OpenApi.Ir;
using Cdd.OpenApi.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cdd.OpenApi
{
    /// <summary>Extracts an <see cref="OpenApiDocument"/> or <see cref="IrService"/> specification from C# source code files.</summary>
    public static class SpecGenerator
    {
        /// <summary>
        /// Parses a collection of C# source code strings and generates a unified <see cref="OpenApiDocument"/>.
        /// </summary>
        /// <param name="csharpSourceCodes">The C# source code texts to analyze.</param>
        /// <returns>The extracted <see cref="OpenApiDocument"/>.</returns>
        public static OpenApiDocument Generate(IEnumerable<string> csharpSourceCodes)
        {
            return GenerateIr(csharpSourceCodes).ToOpenApi();
        }

        /// <summary>
        /// Parses a collection of C# source code strings and generates a unified <see cref="IrService"/>.
        /// </summary>
        /// <param name="csharpSourceCodes">The C# source code texts to analyze.</param>
        /// <returns>The extracted <see cref="IrService"/>.</returns>
        public static IrService GenerateIr(IEnumerable<string> csharpSourceCodes)
        {
            var service = new IrService
            {
                Title = "Generated API",
                Version = "3.2.0"
            };

            var modelsByName = new Dictionary<string, IrModel>();

            foreach (var code in csharpSourceCodes)
            {
                var tree = CSharpSyntaxTree.ParseText(code);
                var classNodes = tree.GetRoot().GetDescendantNodesSafe().OfType<ClassDeclarationSyntax>();

                foreach (var classNode in classNodes)
                {
                    var role = CSharpRoleClassifier.Classify(classNode);

                    if (CSharpRoleClassifier.IsDocumentMetadataHost(classNode))
                    {
                        var selfStr = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "self");
                        if (!string.IsNullOrEmpty(selfStr)) service.Self = selfStr;

                        var dialectStr = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "jsonSchemaDialect");
                        if (!string.IsNullOrEmpty(dialectStr)) service.JsonSchemaDialect = dialectStr;

                        var desc = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "description");
                        if (!string.IsNullOrEmpty(desc)) service.Description = desc;

                        var summary = Cdd.OpenApi.Docstrings.Parse.GetSummary(classNode);
                        if (!string.IsNullOrEmpty(summary)) service.Summary = summary;

                        var version = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "version");
                        if (!string.IsNullOrEmpty(version)) service.Version = version;

                        var terms = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "termsOfService");
                        if (!string.IsNullOrEmpty(terms)) service.TermsOfService = terms;

                        var contactName = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "contact-name");
                        var contactEmail = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "contact-email");
                        var contactUrl = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "contact-url");

                        if (!string.IsNullOrEmpty(contactName) || !string.IsNullOrEmpty(contactEmail) || !string.IsNullOrEmpty(contactUrl))
                        {
                            service.Contact = new OpenApiContact { Name = contactName, Email = contactEmail, Url = contactUrl };
                        }

                        var licenseName = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "license-name");
                        var licenseUrl = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "license-url");
                        var licenseId = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "license-identifier");
                        if (!string.IsNullOrEmpty(licenseName))
                        {
                            service.License = new OpenApiLicense { Name = licenseName, Url = licenseUrl, Identifier = licenseId };
                        }

                        var serverUrl = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "server-url");
                        var serverDescription = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "server-description");
                        if (!string.IsNullOrEmpty(serverUrl))
                        {
                            service.Servers ??= new List<OpenApiServer>();
                            service.Servers.Add(new OpenApiServer { Url = serverUrl, Description = serverDescription });
                        }

                        var extDocsUrl = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "externalDocs-url");
                        var extDocsDesc = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "externalDocs-description");
                        if (!string.IsNullOrEmpty(extDocsUrl))
                        {
                            service.ExternalDocs = new OpenApiExternalDocs { Url = extDocsUrl, Description = extDocsDesc };
                        }

                        var tagName = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "tag-name");
                        var tagDesc = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "tag-description");
                        if (!string.IsNullOrEmpty(tagName))
                        {
                            service.Tags ??= new List<OpenApiTag>();
                            service.Tags.Add(new OpenApiTag { Name = tagName, Description = tagDesc });
                        }
                    }

                    if (role == CSharpClassRole.RouteController)
                    {
                        var (endpoints, pathMeta) = Cdd.OpenApi.Routes.Parse.ToEndpoints(classNode);
                        MergeEndpoints(service, endpoints, pathMeta);

                        var hasAuth = classNode.AttributeLists.SelectMany(al => al.Attributes).Any(a => a.Name.ToString().Contains("Authorize")) ||
                                      classNode.GetDescendantNodesSafe().OfType<MethodDeclarationSyntax>().Any(m => m.AttributeLists.SelectMany(al => al.Attributes).Any(a => a.Name.ToString().Contains("Authorize")));

                        if (hasAuth)
                        {
                            var authName = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "security-name") ?? "Bearer";
                            var authType = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "security-type") ?? "http";
                            var authScheme = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "security-scheme") ?? "bearer";
                            var authBearerFormat = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "security-bearerFormat") ?? "JWT";
                            var authDesc = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "security-description");
                            var authIn = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "security-in");
                            var authOpenId = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "security-openIdConnectUrl");
                            var authOauth2MetadataUrl = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "security-oauth2MetadataUrl");
                            var authDeprecatedStr = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "security-deprecated");
                            bool? authDeprecated = authDeprecatedStr != null && authDeprecatedStr.ToLower() == "true" ? true : null;

                            var schemeObj = new IrSecurityScheme
                            {
                                SchemeName = authName,
                                Type = authType,
                                Scheme = authScheme,
                                BearerFormat = authBearerFormat,
                                Description = authDesc,
                                In = authIn,
                                OpenIdConnectUrl = authOpenId,
                                Oauth2MetadataUrl = authOauth2MetadataUrl,
                                Deprecated = authDeprecated
                            };

                            var oauthFlowType = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "oauth-flow");
                            if (!string.IsNullOrEmpty(oauthFlowType))
                            {
                                var authUrl = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "oauth-authorizationUrl");
                                var tokenUrl = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "oauth-tokenUrl");
                                var refreshUrl = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "oauth-refreshUrl");
                                var deviceUrl = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "oauth-deviceAuthorizationUrl");

                                var flows = new OpenApiOAuthFlows();
                                var flow = new OpenApiOAuthFlow { AuthorizationUrl = authUrl, TokenUrl = tokenUrl, RefreshUrl = refreshUrl, DeviceAuthorizationUrl = deviceUrl, Scopes = new Dictionary<string, string>() };

                                var scopesStr = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "oauth-scopes");
                                if (!string.IsNullOrEmpty(scopesStr))
                                {
                                    foreach (var s in scopesStr.Split(','))
                                    {
                                        var idx = s.IndexOf(':');
                                        if (idx > 0)
                                        {
                                            var lastIdx = s.LastIndexOf(':');
                                            var key = s.Substring(0, lastIdx).Trim();
                                            var val = s.Substring(lastIdx + 1).Trim();
                                            flow.Scopes[key] = val;
                                        }
                                        else
                                        {
                                            flow.Scopes[s.Trim()] = "Access to " + s.Trim();
                                        }
                                    }
                                }

                                if (oauthFlowType == "clientCredentials") flows.ClientCredentials = flow;
                                else if (oauthFlowType == "password") flows.Password = flow;
                                else if (oauthFlowType == "implicit") flows.Implicit = flow;
                                else if (oauthFlowType == "authorizationCode") flows.AuthorizationCode = flow;
                                else if (oauthFlowType == "deviceAuthorization") flows.DeviceAuthorization = flow;

                                schemeObj.Flows = flows;
                                schemeObj.Type = "oauth2";
                                schemeObj.Scheme = null;
                                schemeObj.BearerFormat = null;
                            }

                            service.SecuritySchemes.RemoveAll(s => s.SchemeName == authName);
                            service.SecuritySchemes.Add(schemeObj);

                            service.Security ??= new List<IDictionary<string, IList<string>>>();
                            service.Security.Add(new Dictionary<string, IList<string>> { { authName, new List<string>() } });
                        }
                    }
                    else if (role == CSharpClassRole.HttpClient)
                    {
                        var endpoints = Cdd.OpenApi.Clients.Parse.ToEndpoints(classNode);
                        MergeEndpoints(service, endpoints, null);
                    }
                    else if (role == CSharpClassRole.MockService)
                    {
                        var endpoints = Cdd.OpenApi.Mocks.Parse.ToEndpoints(classNode);
                        MergeEndpoints(service, endpoints, null);
                    }
                    else if (role == CSharpClassRole.CliEntrypoint)
                    {
                        var (endpoints, pathMeta) = Cdd.OpenApi.CliModule.Parse.ToEndpoints(classNode);
                        MergeEndpoints(service, endpoints, pathMeta);
                    }
                    else if (role == CSharpClassRole.TestSuite)
                    {
                        var endpoints = Cdd.OpenApi.TestsModule.Parse.ToEndpoints(classNode);
                        MergeEndpoints(service, endpoints, null);
                    }
                    else if (role == CSharpClassRole.DbContext)
                    {
                        var models = Cdd.OpenApi.Orm.Parse.ToIrModels(classNode);
                        foreach (var m in models)
                        {
                            modelsByName[m.Name] = m;
                        }
                    }
                    else
                    {
                        var model = Cdd.OpenApi.Classes.Parse.ToIrModel(classNode);
                        modelsByName[model.Name] = model;
                    }
                }
            }

            service.Models = modelsByName.Values.ToList();
            return service;
        }

        /// <summary>
        /// Merges extracted <see cref="IrEndpoint"/> instances and optional path metadata into the target <see cref="IrService"/>.
        /// </summary>
        /// <param name="service">The target IR service.</param>
        /// <param name="endpoints">The endpoints to merge.</param>
        /// <param name="pathMeta">Optional path metadata dictionary.</param>
        private static void MergeEndpoints(IrService service, IEnumerable<IrEndpoint> endpoints, Dictionary<string, IrPathMetadata>? pathMeta)
        {
            if (pathMeta != null)
            {
                foreach (var kvp in pathMeta)
                {
                    service.PathMetadata[kvp.Key] = kvp.Value;
                }
            }

            foreach (var ep in endpoints)
            {
                var key = $"{ep.HttpMethod}:{ep.Path}";
                service.Endpoints.RemoveAll(e => $"{e.HttpMethod}:{e.Path}" == key);
                service.Endpoints.Add(ep);
            }
        }
    }
}
