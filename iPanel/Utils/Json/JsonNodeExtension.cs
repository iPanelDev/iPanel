using System.Text.Json;
using System.Text.Json.Nodes;

namespace iPanel.Utils.Json;

public static class JsonNodeExtension
{
    public static T? ToObject<T>(this JsonNode jsonNode, JsonSerializerOptions? options = null)
        where T : notnull
    {
        return JsonSerializer.Deserialize<T>(jsonNode, options);
    }
}
