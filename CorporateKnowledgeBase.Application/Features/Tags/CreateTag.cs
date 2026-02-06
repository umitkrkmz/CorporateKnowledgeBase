namespace CorporateKnowledgeBase.Application.Features.Tags;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using CorporateKnowledgeBase.Domain.Entities;
using MediatR;

public record CreateTagCommand(string Name) : IRequest<int>;

public class CreateTagHandler(IApplicationDbContext context)
    : IRequestHandler<CreateTagCommand, int>
{
    public async Task<int> Handle(
        CreateTagCommand request, CancellationToken cancellationToken)
    {
        var tag = new Tag { Name = request.Name };
        context.Tags.Add(tag);
        await context.SaveChangesAsync(cancellationToken);
        return tag.Id;
    }
}
