using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi
{
/// <summary>Auto-generated documentation for SpecGenerator.</summary>
    public static class SpecGenerator
    {
/// <summary>Auto-generated documentation for Generate.</summary>
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
                var classNodes = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();

                foreach (var classNode in classNodes)
                {
                    var hasRoutes = classNode.DescendantNodes().OfType<MethodDeclarationSyntax>()
                        .Any(m => m.AttributeLists.SelectMany(al => al.Attributes).Any(a => a.Name.ToString().StartsWith("Http")));
                        
                    var hasClientMethods = classNode.DescendantNodes().OfType<InvocationExpressionSyntax>()
                        .Any(inv => inv.Expression is MemberAccessExpressionSyntax memberAccess &&
                                    memberAccess.Name.Identifier.Text.EndsWith("Async") &&
                                    (memberAccess.Name.Identifier.Text.StartsWith("Get") ||
                                     memberAccess.Name.Identifier.Text.StartsWith("Post") ||
                                     memberAccess.Name.Identifier.Text.StartsWith("Put") ||
                                     memberAccess.Name.Identifier.Text.StartsWith("Delete")));

                    var isMock = classNode.Identifier.Text.EndsWith("Mock");
                    var isCli = classNode.Identifier.Text.EndsWith("Cli");
                    var isTest = classNode.Identifier.Text.EndsWith("Tests");
                    var isDbContext = classNode.BaseList?.Types.Any(t => t.Type.ToString() == "DbContext" || t.Type.ToString().EndsWith("DbContext")) == true;

                    // Extract Info tags if they exist on a main entry point or prominent class
                    if (classNode.Identifier.Text == "Program" || classNode.Identifier.Text == "Startup" || classNode.Identifier.Text.EndsWith("Api")) {
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
                        
                        if (!string.IsNullOrEmpty(contactName) || !string.IsNullOrEmpty(contactEmail) || !string.IsNullOrEmpty(contactUrl)) {
                            doc.Info.Contact = new OpenApiContact { Name = contactName, Email = contactEmail, Url = contactUrl };
                        }

                        var licenseName = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "license-name");
                        var licenseUrl = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "license-url");
                        var licenseId = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "license-identifier");
                        if (!string.IsNullOrEmpty(licenseName)) {
                            doc.Info.License = new OpenApiLicense { Name = licenseName, Url = licenseUrl, Identifier = licenseId };
                        }

                        var serverUrl = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "server-url");
                        var serverDescription = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "server-description");
                        if (!string.IsNullOrEmpty(serverUrl)) {
                            if (doc.Servers == null) doc.Servers = new List<OpenApiServer>();
                            doc.Servers.Add(new OpenApiServer { Url = serverUrl, Description = serverDescription });
                        }

                        var extDocsUrl = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "externalDocs-url");
                        var extDocsDesc = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "externalDocs-description");
                        if (!string.IsNullOrEmpty(extDocsUrl)) {
                            doc.ExternalDocs = new OpenApiExternalDocs { Url = extDocsUrl, Description = extDocsDesc };
                        }
                        
                        var tagName = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "tag-name");
                        var tagDesc = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "tag-description");
                        if (!string.IsNullOrEmpty(tagName)) {
                            if (doc.Tags == null) doc.Tags = new List<OpenApiTag>();
                            doc.Tags.Add(new OpenApiTag { Name = tagName, Description = tagDesc });
                        }
                    }

                    if (hasRoutes)
                    {
                        var paths = Cdd.OpenApi.Routes.Parse.ToPaths(classNode);
                        foreach (var p in paths)
                        {
                            doc.Paths![p.Key] = p.Value;
                        }

                        // Check for Authorize attribute on class or methods to add simple Bearer auth
                        var hasAuth = classNode.AttributeLists.SelectMany(al => al.Attributes).Any(a => a.Name.ToString().Contains("Authorize")) ||
                                      classNode.DescendantNodes().OfType<MethodDeclarationSyntax>().Any(m => m.AttributeLists.SelectMany(al => al.Attributes).Any(a => a.Name.ToString().Contains("Authorize")));
                        
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

                                // Simple mock scope addition if scopes tag exists
                                var scopesStr = Cdd.OpenApi.Docstrings.Parse.GetTag(classNode, "oauth-scopes");
                                if (!string.IsNullOrEmpty(scopesStr))
                                {
                                    foreach (var s in scopesStr.Split(','))
                                    {
                                        var idx = s.IndexOf(':');
                                        if (idx > 0)
                                        {
                                            var key = s.Substring(0, idx).Trim();
                                            var val = s.Substring(idx + 1).Trim();
                                            // Handle case where scope might be like `read:pets:Read pets` meaning key="read:pets" and val="Read pets"
                                            // We will assume the last colon is the separator if there are multiple.
                                            var lastIdx = s.LastIndexOf(':');
                                            if (lastIdx > 0 && lastIdx != idx) {
                                                key = s.Substring(0, lastIdx).Trim();
                                                val = s.Substring(lastIdx + 1).Trim();
                                            }
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
                                schemeObj.Type = "oauth2"; // Force oauth2 type
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
                    else if (hasClientMethods)
                    {
                        var paths = Cdd.OpenApi.Clients.Parse.ToPaths(classNode);
                        foreach (var p in paths)
                        {
                            doc.Paths![p.Key] = p.Value;
                        }
                    }
                    else if (isMock)
                    {
                        var paths = Cdd.OpenApi.Mocks.Parse.ToPaths(classNode);
                        foreach (var p in paths)
                        {
                            doc.Paths![p.Key] = p.Value;
                        }
                    }
                    else if (isCli)
                    {
                        var paths = Cdd.OpenApi.CliModule.Parse.ToPaths(classNode);
                        foreach (var p in paths)
                        {
                            doc.Paths![p.Key] = p.Value;
                        }
                    }
                    else if (isTest)
                    {
                        var paths = Cdd.OpenApi.TestsModule.Parse.ToPaths(classNode);
                        foreach (var p in paths)
                        {
                            doc.Paths![p.Key] = p.Value;
                        }
                    }
                    else if (isDbContext)
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
                if (doc.Components.Schemas != null && !doc.Components.Schemas.Any()) doc.Components.Schemas = null;
                if (doc.Components.SecuritySchemes != null && !doc.Components.SecuritySchemes.Any()) doc.Components.SecuritySchemes = null;
                
                if (doc.Components.Schemas == null && doc.Components.SecuritySchemes == null)
                {
                    doc.Components = null;
                }
            }

            return doc;
        }
    }
}