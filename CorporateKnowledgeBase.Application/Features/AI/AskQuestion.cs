namespace CorporateKnowledgeBase.Application.Features.AI;

using CorporateKnowledgeBase.Application.Common.Interfaces;
using MediatR;

public record AskQuestionQuery(string Question) : IRequest<string>;

public class AskQuestionHandler(IAIAssistantService aiService)
    : IRequestHandler<AskQuestionQuery, string>
{
    public async Task<string> Handle(
        AskQuestionQuery request, CancellationToken cancellationToken)
    {
        return await aiService.AskSimpleAsync(request.Question);
    }
}
