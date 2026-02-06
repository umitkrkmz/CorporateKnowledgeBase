namespace CorporateKnowledgeBase.Application.Common.Behaviors;

using System.Text.Json;
using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

public class AuditLogBehavior<TRequest, TResponse>(
    IApplicationDbContext context,
    IHttpContextAccessor httpContextAccessor)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next(cancellationToken);

        // Sadece Command'leri logla (Query'leri değil)
        var requestName = typeof(TRequest).Name;
        if (!requestName.EndsWith("Command")) return response;

        var user = httpContextAccessor.HttpContext?.User;
        var userId = user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "System";
        var userName = user?.Identity?.Name ?? "System";

        var auditLog = new AuditLog
        {
            UserId = userId,
            UserName = userName,
            Action = requestName,
            EntityName = requestName.Replace("Command", ""),
            NewValues = JsonSerializer.Serialize(request),
            CreatedAt = DateTime.UtcNow
        };

        context.AuditLogs.Add(auditLog);
        await context.SaveChangesAsync(cancellationToken);

        return response;
    }
}
