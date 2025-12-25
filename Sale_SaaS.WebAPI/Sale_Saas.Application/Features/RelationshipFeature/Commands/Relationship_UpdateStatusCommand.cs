using Sale_Saas.Application.Features.RelationshipFeature.Dto;
using Sale_Saas.Application.Features.RelationshipFeature.Services;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.RelationshipFeature.Commands
{
     public record Relationship_UpdateStatusByIdCommand(UpdateStatusRequest RequestData) : IRequest<Result<RelationshipDto>>;
     public class Relationship_UpdateStatusByIdCommandHandler : IRequestHandler<Relationship_UpdateStatusByIdCommand, Result<RelationshipDto>>
     {

          private readonly IApplicationDbContext _context;
          private readonly IApplicationUserService _UserService;
          private readonly IMapper _mapper;
          public Relationship_UpdateStatusByIdCommandHandler(IMapper mapper, IApplicationDbContext context, IApplicationUserService UserService)
          {
               _context = context;
               _mapper = mapper;
               _UserService = UserService;
          }

          public async Task<Result<RelationshipDto>> Handle(Relationship_UpdateStatusByIdCommand request, CancellationToken cancellationToken)
          {
               request.RequestData.Status = request.RequestData.Status.ToUpper();
               var data = await RelationshipService.GetRelationship(request.RequestData.Id, _context);
               var status = await RelationshipService.GetStatus(request.RequestData.Status, _context);

               switch (Enum.Parse<RelationshipStatusEnum>(request.RequestData.Status ?? ""))
               {
                    case RelationshipStatusEnum.CONFIRMED:
                         if (data.RelationshipStatus!.Code != RelationshipStatusEnum.PENDING.ToString())
                         {
                              throw new ApplicationException($"Trạng thái không hợp lệ: {request.RequestData.Status}");
                         }
                         break;
                    case RelationshipStatusEnum.PROCESSING:
                         if (data.RelationshipStatus!.Code != RelationshipStatusEnum.CONFIRMED.ToString())
                         {
                              throw new ApplicationException($"Trạng thái không hợp lệ: {request.RequestData.Status}");
                         }
                         break;
                    case RelationshipStatusEnum.COMPLETED:
                         if (data.CurrentRelationshipId != data.TargetRelationshipId)
                         {
                              throw new ApplicationException($"Trạng thái không hợp lệ: {request.RequestData.Status}");
                         }
                         break;
                    default:
                         break;
               }

               //await RelationshipService.AddHistory(data.Id, data.RelationshipStatusId, status.Id, request.RequestData.ApplicationUserId, _context);
               data.RelationshipStatus = status;
               data.LastModifiedDate = DateTime.Now;
               data.LastModifiedApplicationUserId = request.RequestData.ApplicationUserId;

               await _context.SaveChangesAsync(cancellationToken);

               return Result<RelationshipDto>.Success(_mapper.Map<RelationshipDto>(data));
          }
     }
}
