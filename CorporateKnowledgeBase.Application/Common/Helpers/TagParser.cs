namespace CorporateKnowledgeBase.Application.Common.Helpers;

using System.Text.Json;

public static class TagParser
{
    /// <summary>
    /// Parses Tagify JSON format [{"value":"tag1"},{"value":"tag2"}] or comma-separated fallback.
    /// </summary>
    public static List<string> Parse(string? tagsInput)
    {
        if (string.IsNullOrWhiteSpace(tagsInput))
            return [];

        tagsInput = tagsInput.Trim();

        // Try Tagify JSON format
        if (tagsInput.StartsWith('['))
        {
            try
            {
                var tagObjects = JsonSerializer.Deserialize<List<TagifyItem>>(tagsInput,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (tagObjects is not null)
                    return tagObjects
                        .Where(t => !string.IsNullOrWhiteSpace(t.Value))
                        .Select(t => t.Value.Trim())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();
            }
            catch (JsonException) { }
        }

        // Fallback: comma-separated
        return tagsInput.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private class TagifyItem
    {
        public string Value { get; set; } = string.Empty;
    }
}
