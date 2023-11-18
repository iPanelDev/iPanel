using System.Text.Encodings.Web;
using System.Text.Json;

namespace iPanel.Utils.Json;

public static class JsonSerializerOptionsFactory
{
    public static readonly JsonSerializerOptions CamelCase =
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
}
