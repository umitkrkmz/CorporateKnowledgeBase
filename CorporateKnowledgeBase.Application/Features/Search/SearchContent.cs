namespace CorporateKnowledgeBase.Application.Features.Search;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record SearchQuery(string Term) : IRequest<SearchResultDto>;

public record SearchResultDto(
    List<SearchItemDto> Documents,
    List<SearchItemDto> BlogPosts);

public record SearchItemDto(
    int Id,
    string Title,
    string Snippet,
    ContentType ContentType,
    DateTime CreatedAt);

public class SearchHandler(IApplicationDbContext context)
    : IRequestHandler<SearchQuery, SearchResultDto>
{
    public async Task<SearchResultDto> Handle(
        SearchQuery request, CancellationToken cancellationToken)
    {
        var term = request.Term.ToLower();

        var documents = await context.Documents
            .Where(d => d.Title.ToLower().Contains(term)
                     || d.Content.ToLower().Contains(term))
            .OrderByDescending(d => d.CreatedAt)
            .Take(20)
            .Select(d => new SearchItemDto(
                d.Id, d.Title,
                d.Content.Length > 150 ? d.Content.Substring(0, 150) + "..." : d.Content,
                ContentType.Document, d.CreatedAt))
            .ToListAsync(cancellationToken);

        var blogPosts = await context.BlogPosts
            .Where(b => b.Title.ToLower().Contains(term)
                     || b.Content.ToLower().Contains(term))
            .OrderByDescending(b => b.CreatedAt)
            .Take(20)
            .Select(b => new SearchItemDto(
                b.Id, b.Title,
                b.Content.Length > 150 ? b.Content.Substring(0, 150) + "..." : b.Content,
                ContentType.BlogPost, b.CreatedAt))
            .ToListAsync(cancellationToken);

        return new SearchResultDto(documents, blogPosts);
    }
}
