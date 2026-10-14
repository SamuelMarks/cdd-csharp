// <copyright file="OpenApiCallback.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    /// <summary>A map of possible out-of-band callbacks related to the parent operation, keyed by runtime expression.</summary>
    public class OpenApiCallback : Dictionary<string, OpenApiPathItem>
    {
    }
}
