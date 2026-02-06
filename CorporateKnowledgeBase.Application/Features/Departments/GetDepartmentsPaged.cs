namespace CorporateKnowledgeBase.Application.Features.Departments;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetDepartmentsPagedQuery : IRequest<PagedResult<DepartmentDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
}

public class GetDepartmentsPagedHandler(IApplicationDbContext context)
    : IRequestHandler<GetDepartmentsPagedQuery, PagedResult<DepartmentDto>>
{
    public async Task<PagedResult<DepartmentDto>> Handle(
        GetDepartmentsPagedQuery request, CancellationToken cancellationToken)
    {
        var query = context.Departments.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(d =>
                d.Name.ToLower().Contains(searchTerm) ||
                (d.Description != null && d.Description.ToLower().Contains(searchTerm)));
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "description" => request.SortDescending
                ? query.OrderByDescending(d => d.Description)
                : query.OrderBy(d => d.Description),
            _ => request.SortDescending
                ? query.OrderByDescending(d => d.Name)
                : query.OrderBy(d => d.Name)
        };

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and project
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(d => new DepartmentDto(d.Id, d.Name, d.Description))
            .ToListAsync(cancellationToken);

        return PagedResult<DepartmentDto>.Create(items, request.Page, request.PageSize, totalCount);
    }
}
