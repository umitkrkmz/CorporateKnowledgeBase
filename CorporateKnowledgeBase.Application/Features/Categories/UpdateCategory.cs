namespace CorporateKnowledgeBase.Application.Features.Categories;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record UpdateCategoryCommand(int Id, string Name, string? Description) : IRequest<bool>;

public class UpdateCategoryHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateCategoryCommand, bool>
{
    public async Task<bool> Handle(
        UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null) return false;

        category.Name = request.Name;
        category.Description = request.Description;
        category.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
