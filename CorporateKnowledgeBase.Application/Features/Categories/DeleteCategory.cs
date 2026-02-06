namespace CorporateKnowledgeBase.Application.Features.Categories;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record DeleteCategoryCommand(int Id) : IRequest<bool>;

public class DeleteCategoryHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteCategoryCommand, bool>
{
    public async Task<bool> Handle(
        DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null) return false;

        context.Categories.Remove(category);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
