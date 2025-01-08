using System.Text.Json;

#nullable enable

namespace OpenQA.Selenium.DevTools.Json;

internal static class DevToolsJsonOptions
{
    public static JsonSerializerOptions DevToolsSerializerOptions { get; } = new JsonSerializerOptions()
    {
        Converters =
        {
            new StringConverter(),
        }
    };
}
