namespace CorporateKnowledgeBase.Application.Common.Extensions;

using System.Linq.Expressions;
using CorporateKnowledgeBase.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Extension methods for IQueryable to support pagination and sorting
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Applies pagination to the query
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<T>.Create(items, page, pageSize, totalCount);
    }

    /// <summary>
    /// Applies pagination using QueryParameters
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        QueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        return await query.ToPagedResultAsync(parameters.Page, parameters.PageSize, cancellationToken);
    }

    /// <summary>
    /// Applies dynamic ordering to the query
    /// </summary>
    public static IQueryable<T> ApplyOrdering<T>(
        this IQueryable<T> query,
        string? sortBy,
        bool descending,
        Expression<Func<T, object>> defaultSort)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return descending
                ? query.OrderByDescending(defaultSort)
                : query.OrderBy(defaultSort);
        }

        var parameter = Expression.Parameter(typeof(T), "x");
        var property = typeof(T).GetProperty(sortBy,
            System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        if (property == null)
        {
            return descending
                ? query.OrderByDescending(defaultSort)
                : query.OrderBy(defaultSort);
        }

        var propertyAccess = Expression.MakeMemberAccess(parameter, property);
        var orderByExpression = Expression.Lambda<Func<T, object>>(
            Expression.Convert(propertyAccess, typeof(object)), parameter);

        return descending
            ? query.OrderByDescending(orderByExpression)
            : query.OrderBy(orderByExpression);
    }
}
