/*
 * File: JsonDefaults.cs
 * Description: Shared camelCase JSON options so error bodies match successful responses.
 * Author: Member 1
 * Created: 20/09/2026
 */

using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartSolar.Api.Common;

public static class JsonDefaults
{
    public static readonly JsonSerializerOptions Options = Create();

    // Builds the serializer used by middleware and JWT challenge/forbid handlers.
    private static JsonSerializerOptions Create()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };
    }
}
