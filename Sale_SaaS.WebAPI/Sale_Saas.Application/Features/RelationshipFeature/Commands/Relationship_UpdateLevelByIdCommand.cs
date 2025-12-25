using Sale_Saas.Application.Features.RelationshipFeature.Dto;
using Sale_Saas.Application.Features.RelationshipFeature.RequestModels;
using Sale_Saas.Application.Features.RelationshipFeature.Services;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.RelationshipFeature.Commands
{
    public record Relationship_UpdateLevelByIdCommand(UpdateLevelRequest RequestData) : IRequest<Result<RelationshipDto>>;

    public class Relationship_UpdateLevelByIdCommandHandler : IRequestHandler<Relationship_UpdateLevelByIdCommand, Result<RelationshipDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IApplicationUserService _UserService;
        private readonly IMapper _mapper;
        public Relationship_UpdateLevelByIdCommandHandler(IMapper mapper, IApplicationDbContext context, IApplicationUserService UserService)
        {
            _context = context;
            _mapper = mapper;
            _UserService = UserService;
        }

        public async Task<Result<RelationshipDto>> Handle(Relationship_UpdateLevelByIdCommand request, CancellationToken cancellationToken)
        {
            request.RequestData.Level = request.RequestData.Level.ToUpper();
            var data = await RelationshipService.GetRelationship(request.RequestData.Id, _context);
            var level = await RelationshipService.GetLevel(request.RequestData.Level, _context);

            switch (Enum.Parse<RelationshipLevelEnum>(request.RequestData.Level ?? ""))
            {
                case RelationshipLevelEnum.A:
                    if (data.CurrentRelationship!.Code == RelationshipLevelEnum.A.ToString())
                    {
                        throw new ApplicationException($"Level không hợp lệ: {request.RequestData.Level}");
                    }
                    break;
                case RelationshipLevelEnum.B:
                    if (data.CurrentRelationship!.Code == RelationshipLevelEnum.B.ToString())
                    {
                        throw new ApplicationException($"Level không hợp lệ: {request.RequestData.Level}");
                    }
                    break;
                case RelationshipLevelEnum.C:
                    if (data.CurrentRelationship!.Code == RelationshipLevelEnum.B.ToString())
                    {
                        throw new ApplicationException($"Level không hợp lệ: {request.RequestData.Level}");
                    }
                    break;
                case RelationshipLevelEnum.D:
                    if (data.CurrentRelationship!.Code == RelationshipLevelEnum.B.ToString())
                    {
                        throw new ApplicationException($"Level không hợp lệ: {request.RequestData.Level}");
                    }
                    break;
                case RelationshipLevelEnum.E:
                    if (data.CurrentRelationship!.Code == RelationshipLevelEnum.B.ToString())
                    {
                        throw new ApplicationException($"Level không hợp lệ: {request.RequestData.Level}");
                    }
                    break;
                case RelationshipLevelEnum.F:
                    if (data.CurrentRelationship!.Code == RelationshipLevelEnum.B.ToString())
                    {
                        throw new ApplicationException($"Level không hợp lệ: {request.RequestData.Level}");
                    }
                    break;
                default:
                    break;
            }

            await RelationshipService.AddHistory(data.Id, data.CurrentRelationshipId ?? Guid.Empty, level.Id, request.RequestData.ApplicationUserId, _context);
            data.CurrentRelationship = level;
            data.LastModifiedDate = DateTime.Now;
            data.LastModifiedApplicationUserId = request.RequestData.ApplicationUserId;

            await _context.SaveChangesAsync(cancellationToken);

            return Result<RelationshipDto>.Success(_mapper.Map<RelationshipDto>(data));
        }
    }
}
