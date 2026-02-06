namespace CorporateKnowledgeBase.Application.Features.Dashboard;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetDashboardStatsQuery : IRequest<DashboardDto>;

public record DashboardDto(
    int DocumentCount,
    int BlogPostCount,
    int AnnouncementCount,
    int CategoryCount,
    int UserCount,
    List<RecentItemDto> RecentDocuments,
    List<RecentItemDto> RecentBlogPosts,
    List<RecentItemDto> RecentAnnouncements);

public record RecentItemDto(int Id, string Title, string AuthorName, DateTime CreatedAt);

public class GetDashboardStatsHandler(IApplicationDbContext context, IUserNameResolver userNameResolver)
    : IRequestHandler<GetDashboardStatsQuery, DashboardDto>
{
    public async Task<DashboardDto> Handle(
        GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var docCount = await context.Documents.CountAsync(cancellationToken);
        var blogCount = await context.BlogPosts.CountAsync(cancellationToken);
        var annCount = await context.Announcements.CountAsync(cancellationToken);
        var catCount = await context.Categories.CountAsync(cancellationToken);

        var recentDocs = await context.Documents
            .OrderByDescending(d => d.CreatedAt)
            .Take(5)
            .Select(d => new { d.Id, d.Title, d.AuthorId, d.CreatedAt })
            .ToListAsync(cancellationToken);

        var recentBlogs = await context.BlogPosts
            .OrderByDescending(b => b.CreatedAt)
            .Take(5)
            .Select(b => new { b.Id, b.Title, b.AuthorId, b.CreatedAt })
            .ToListAsync(cancellationToken);

        var recentAnns = await context.Announcements
            .OrderByDescending(a => a.CreatedAt)
            .Take(3)
            .Select(a => new { a.Id, a.Title, a.AuthorId, a.CreatedAt })
            .ToListAsync(cancellationToken);

        var docDtos = new List<RecentItemDto>();
        foreach (var d in recentDocs)
            docDtos.Add(new RecentItemDto(d.Id, d.Title, await userNameResolver.GetFullNameAsync(d.AuthorId), d.CreatedAt));

        var blogDtos = new List<RecentItemDto>();
        foreach (var b in recentBlogs)
            blogDtos.Add(new RecentItemDto(b.Id, b.Title, await userNameResolver.GetFullNameAsync(b.AuthorId), b.CreatedAt));

        var annDtos = new List<RecentItemDto>();
        foreach (var a in recentAnns)
            annDtos.Add(new RecentItemDto(a.Id, a.Title, await userNameResolver.GetFullNameAsync(a.AuthorId), a.CreatedAt));

        // User count - get from announcements context (we don't have UserManager here, so approximate)
        var userCount = 0;
        try
        {
            var authorIds = await context.Documents.Select(d => d.AuthorId)
                .Union(context.BlogPosts.Select(b => b.AuthorId))
                .Distinct()
                .CountAsync(cancellationToken);
            userCount = authorIds;
        }
        catch { userCount = 0; }

        return new DashboardDto(
            docCount, blogCount, annCount, catCount, userCount,
            docDtos, blogDtos, annDtos);
    }
}
