using System.Text.Json;

namespace EdgePMO.API.Settings
{
    public static class ContentBlockSerializer
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        public static string Serialize(object content)
        {
            if (content == null)
                return string.Empty;

            return JsonSerializer.Serialize(content, JsonOptions);
        }

        public static object Deserialize(string jsonContent, string blockType)
        {
            if (string.IsNullOrEmpty(jsonContent))
                return blockType == "list" ? new List<string>() : string.Empty;

            return blockType switch
            {
                "paragraph" => JsonSerializer.Deserialize<string>(jsonContent, JsonOptions) ?? string.Empty,
                "list" => JsonSerializer.Deserialize<List<string>>(jsonContent, JsonOptions) ?? new List<string>(),
                _ => throw new InvalidOperationException($"Unknown block type: {blockType}")
            };
        }
    }
}
