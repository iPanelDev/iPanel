using iPanel.Utils.Json;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace iPanel.Tests;

public static class Utils
{
    public static HttpContent CreateContent<T>(T obj, string? contentType = null)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(
                obj,
                options: new(JsonSerializerOptionsFactory.CamelCase)
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
                }
            )
        )
        {
            Headers = { ContentType = string.IsNullOrEmpty(contentType) ? null : new(contentType) }
        };

        return content;
    }
}
