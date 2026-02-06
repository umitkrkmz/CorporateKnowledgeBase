namespace CorporateKnowledgeBase.Application.Features.Categories;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetCategoriesPagedQuery : IRequest<PagedResult<CategoryDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
}

public class GetCategoriesPagedHandler(IApplicationDbContext context)
    : IRequestHandler<GetCategoriesPagedQuery, PagedResult<CategoryDto>>
{
    public async Task<PagedResult<CategoryDto>> Handle(
        GetCategoriesPagedQuery request, CancellationToken cancellationToken)
    {
        var query = context.Categories.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(searchTerm) ||
                (c.Description != null && c.Description.ToLower().Contains(searchTerm)));
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "description" => request.SortDescending
                ? query.OrderByDescending(c => c.Description)
                : query.OrderBy(c => c.Description),
            _ => request.SortDescending
                ? query.OrderByDescending(c => c.Name)
                : query.OrderBy(c => c.Name)
        };

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and project
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Description))
            .ToListAsync(cancellationToken);

        return PagedResult<CategoryDto>.Create(items, request.Page, request.PageSize, totalCount);
    }
}
