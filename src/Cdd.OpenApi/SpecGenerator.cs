// <copyright file="SpecGenerator.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Common;
using Cdd.OpenApi.Ir;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi
{
    /// <summary>Extracts an <see cref="OpenApiDocument"/> specification from C# source code files.</summary>
    public static class SpecGenerator
    {
        /// <summary>
        /// Parses a collection of C# source code strings and generates a unified <see cref="OpenApiDocument"/>.
        /// </summary>
        /// <param name="csharpSourceCodes">The C# source code texts to analyze.</param>
        /// <returns>The extracted <see cref="OpenApiDocument"/>.</returns>
        public static OpenApiDocument Generate(IEnumerable<string> csharpSourceCodes)
        {
            var doc = new OpenApiDocument
            {
                Info = new OpenApiInfo { Title = "Generated API", Version = "3.2.0" },
                Paths = new OpenApiPaths(),
                Components = new OpenApiComponents { Schemas = new Dictionary<string, OpenApiSchema>() }
            };

            foreach (var code in csharpSourceCodes)
            {
                var tree = CSharpSyntaxTree.ParseText(code);
                var classNodes = tree.GetRoot().GetDescendantNodesSafe().OfType<ClassDeclarationSyntax>();

                foreach (var classNode in classNodes)
                {
                    var role = CSharpRoleClassifier.Classify(classNode);

                    // Extract Info tags if they exist on a main entry point or prominent class
                    if (CSharpRoleClassifier.IsDocumentMetadataHost(classNode))
                    {
                        var selfStr = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "self");
                        if (!string.IsNullOrEmpty(selfStr)) doc.Self = selfStr;

                        var dialectStr = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "jsonSchemaDialect");
                        if (!string.IsNullOrEmpty(dialectStr)) doc.JsonSchemaDialect = dialectStr;

                        var desc = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "description");
                        if (!string.IsNullOrEmpty(desc)) doc.Info.Description = desc;

                        var summary = Cdd.OpenApi.Docstrings.Parse.GetSummary(classNode);
                        if (!string.IsNullOrEmpty(summary)) doc.Info.Summary = summary;

                        var version = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "version");
                        if (!string.IsNullOrEmpty(version)) doc.Info.Version = version;

                        var terms = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "termsOfService");
                        if (!string.IsNullOrEmpty(terms)) doc.Info.TermsOfService = terms;

                        var contactName = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "contact-name");
                        var contactEmail = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "contact-email");
                        var contactUrl = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "contact-url");

                        if (!string.IsNullOrEmpty(contactName) || !string.IsNullOrEmpty(contactEmail) || !string.IsNullOrEmpty(contactUrl))
                        {
                            doc.Info.Contact = new OpenApiContact { Name = contactName, Email = contactEmail, Url = contactUrl };
                        }

                        var licenseName = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "license-name");
                        var licenseUrl = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "license-url");
                        var licenseId = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "license-identifier");
                        if (!string.IsNullOrEmpty(licenseName))
                        {
                            doc.Info.License = new OpenApiLicense { Name = licenseName, Url = licenseUrl, Identifier = licenseId };
                        }

                        var serverUrl = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "server-url");
                        var serverDescription = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "server-description");
                        if (!string.IsNullOrEmpty(serverUrl))
                        {
                            if (doc.Servers == null) doc.Servers = new List<OpenApiServer>();
                            doc.Servers.Add(new OpenApiServer { Url = serverUrl, Description = serverDescription });
                        }

                        var extDocsUrl = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "externalDocs-url");
                        var extDocsDesc = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "externalDocs-description");
                        if (!string.IsNullOrEmpty(extDocsUrl))
                        {
                            doc.ExternalDocs = new OpenApiExternalDocs { Url = extDocsUrl, Description = extDocsDesc };
                        }

                        var tagName = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "tag-name");
                        var tagDesc = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "tag-description");
                        if (!string.IsNullOrEmpty(tagName))
                        {
                            if (doc.Tags == null) doc.Tags = new List<OpenApiTag>();
                            doc.Tags.Add(new OpenApiTag { Name = tagName, Description = tagDesc });
                        }
                    }

                    if (role == CSharpClassRole.RouteController)
                    {
                        var paths = Cdd.OpenApi.Routes.Parse.ToPaths(classNode);
                        foreach (var p in paths)
                        {
                            doc.Paths![p.Key] = p.Value;
                        }

                        // Check for Authorize attribute on class or methods to add simple Bearer auth
                        var hasAuth = classNode.AttributeLists.SelectMany(al => al.Attributes).Any(a => a.Name.ToString().Contains("Authorize")) ||
                                      classNode.GetDescendantNodesSafe().OfType<MethodDeclarationSyntax>().Any(m => m.AttributeLists.SelectMany(al => al.Attributes).Any(a => a.Name.ToString().Contains("Authorize")));

                        if (hasAuth)
                        {
                            if (doc.Components!.SecuritySchemes == null)
                            {
                                doc.Components.SecuritySchemes = new Dictionary<string, OpenApiSecurityScheme>();
                            }

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

                            var schemeObj = new OpenApiSecurityScheme
                            {
                                Type = authType,
                                Scheme = authScheme,
                                BearerFormat = authBearerFormat,
                                Description = authDesc,
                                In = authIn,
                                OpenIdConnectUrl = authOpenId,
                                Oauth2MetadataUrl = authOauth2MetadataUrl,
                                Deprecated = authDeprecated
                            };

                            var oauthFlowType = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "oauth-flow"); // e.g. clientCredentials
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

                            doc.Components.SecuritySchemes[authName] = schemeObj;

                            if (doc.Security == null)
                            {
                                doc.Security = new List<IDictionary<string, IList<string>>>();
                            }
                            doc.Security.Add(new Dictionary<string, IList<string>> { { authName, new List<string>() } });
                        }
                    }
                    else if (role == CSharpClassRole.HttpClient)
                    {
                        var paths = Cdd.OpenApi.Clients.Parse.ToPaths(classNode);
                        foreach (var p in paths)
                        {
                            doc.Paths![p.Key] = p.Value;
                        }
                    }
                    else if (role == CSharpClassRole.MockService)
                    {
                        var paths = Cdd.OpenApi.Mocks.Parse.ToPaths(classNode);
                        foreach (var p in paths)
                        {
                            doc.Paths![p.Key] = p.Value;
                        }
                    }
                    else if (role == CSharpClassRole.CliEntrypoint)
                    {
                        var paths = Cdd.OpenApi.CliModule.Parse.ToPaths(classNode);
                        foreach (var p in paths)
                        {
                            doc.Paths![p.Key] = p.Value;
                        }
                    }
                    else if (role == CSharpClassRole.TestSuite)
                    {
                        var paths = Cdd.OpenApi.TestsModule.Parse.ToPaths(classNode);
                        foreach (var p in paths)
                        {
                            doc.Paths![p.Key] = p.Value;
                        }
                    }
                    else if (role == CSharpClassRole.DbContext)
                    {
                        var schemas = Cdd.OpenApi.Orm.Parse.ToSchemas(classNode);
                        foreach (var s in schemas)
                        {
                            doc.Components!.Schemas![s.Key] = s.Value;
                        }
                    }
                    else
                    {
                        var schema = Cdd.OpenApi.Classes.Parse.ToSchema(classNode);
                        doc.Components!.Schemas![classNode.Identifier.Text] = schema;
                    }
                }
            }

            if (!doc.Paths!.Any()) doc.Paths = null;
            if (doc.Components != null)
            {
                if (doc.Components.Schemas != null)
                {
                    if (!doc.Components.Schemas.Any())
                    {
                        doc.Components.Schemas = null;
                    }
                }
                if (doc.Components.Schemas == null && doc.Components.SecuritySchemes == null)
                {
                    doc.Components = null;
                }
            }

            return IrService.FromOpenApi(doc).ToOpenApi();
        }
    }
}
