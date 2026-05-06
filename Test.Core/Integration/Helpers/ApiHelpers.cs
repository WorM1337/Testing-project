using System.Text.Json;
using System.Text.Json.Serialization;

namespace Test.Core.Integration.Helpers;

public static class ApiHelpers
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };
}
