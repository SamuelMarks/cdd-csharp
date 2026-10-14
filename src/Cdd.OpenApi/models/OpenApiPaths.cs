// <copyright file="OpenApiPaths.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    /// <summary>Holds the relative paths to the individual endpoints and their operations.</summary>
    public class OpenApiPaths : Dictionary<string, OpenApiPathItem>
    {
    }
}
