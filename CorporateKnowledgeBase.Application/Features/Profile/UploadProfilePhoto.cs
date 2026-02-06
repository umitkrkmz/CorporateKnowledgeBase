namespace CorporateKnowledgeBase.Application.Features.Profile;

using MediatR;

public record UploadProfilePhotoCommand(
    string UserId,
    string FileName,
    byte[] FileData) : IRequest<string?>;
