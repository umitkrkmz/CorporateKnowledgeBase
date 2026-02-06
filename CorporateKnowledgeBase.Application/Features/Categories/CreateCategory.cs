namespace CorporateKnowledgeBase.Application.Features.Categories;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Domain.Entities;
using MediatR;

public record CreateCategoryCommand(string Name, string? Description) : IRequest<int>;

public class CreateCategoryHandler(IApplicationDbContext context)
    : IRequestHandler<CreateCategoryCommand, int>
{
    public async Task<int> Handle(
        CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new Category
        {
            Name = request.Name,
            Description = request.Description
        };

        context.Categories.Add(category);
        await context.SaveChangesAsync(cancellationToken);
        return category.Id;
    }
}
