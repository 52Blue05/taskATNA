using Sale_Saas.Application.Interfaces.Services;

namespace Sale_Saas.Application.Features.RelationshipFeature.Commands;

public record Relationship_CreateAvatarCommand(IFormFile Avatar) : IRequest<Result<string>>;
public class Relationship_CreateAvatarCommandHandler : IRequestHandler<Relationship_CreateAvatarCommand, Result<string>>
{
    private readonly IFileStorageService _fileStorageService;

    public Relationship_CreateAvatarCommandHandler(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<string>> Handle(Relationship_CreateAvatarCommand request, CancellationToken cancellationToken)
    {
        string result = string.Empty;
        if (request.Avatar != null)
        {
            var response = await _fileStorageService.UploadFileAsync(new List<IFormFile>() { request.Avatar }, "sass", true);
            if (!response.Any())
            {
                throw new Exception("Thay đổi avatar không thành công");
            }

            result = response[0].ServerPath;
        }

        return Result<string>.Success(result);
    }
}
