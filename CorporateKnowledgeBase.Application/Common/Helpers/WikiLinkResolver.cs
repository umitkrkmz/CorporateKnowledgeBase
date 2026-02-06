namespace CorporateKnowledgeBase.Application.Common.Helpers;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

public interface IWikiLinkResolver
{
    Task<string> ResolveAsync(string content);
}

public class WikiLinkResolver(IApplicationDbContext context) : IWikiLinkResolver
{
    private static readonly Regex WikiLinkPattern = new(@"\[\[(.+?)\]\]", RegexOptions.Compiled);

    public async Task<string> ResolveAsync(string content)
    {
        if (string.IsNullOrEmpty(content)) return content;

        var matches = WikiLinkPattern.Matches(content);
        if (matches.Count == 0) return content;

        // Collect all referenced titles
        var titles = matches.Select(m => m.Groups[1].Value.Trim()).Distinct().ToList();

        // Look up documents by title
        var documents = await context.Documents
            .Where(d => titles.Contains(d.Title))
            .Select(d => new { d.Id, d.Title })
            .ToListAsync();

        var docMap = documents.ToDictionary(d => d.Title, d => d.Id, StringComparer.OrdinalIgnoreCase);

        // Replace [[Title]] with links
        var result = WikiLinkPattern.Replace(content, match =>
        {
            var title = match.Groups[1].Value.Trim();
            if (docMap.TryGetValue(title, out var docId))
            {
                return $"<a href=\"/Documents/Details/{docId}\" class=\"wiki-link\" title=\"{title}\">{title}</a>";
            }
            // Unresolved link - show as red/broken link
            return $"<span class=\"wiki-link-broken\" title=\"Document not found: {title}\">{title}</span>";
        });

        return result;
    }
}
