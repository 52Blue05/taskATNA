using Sale_Saas.Application.Features.RelationshipFeature.Dto;
using Sale_Saas.Application.Features.RelationshipFeature.Services;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Domain.Constants.API;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.RelationshipFeature.Commands
{
     public record Relationship_AddOrUpdateCommand(Guid UserId, List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<RelationshipDto>>>;

     public class Relationship_AddOrUpdateCommandHandler : IRequestHandler<Relationship_AddOrUpdateCommand, Result<List<RelationshipDto>>>
     {
          private readonly IApplicationDbContext _context;
          private readonly IMapper _mapper;
          private readonly IInternalService _internalService;
          private readonly IApplicationUserService _userService;
          private readonly IFeaturePermissionService _permissionService;
          public Relationship_AddOrUpdateCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService,
                                                            IApplicationUserService userService,
                                                            IFeaturePermissionService permissionService)
          {
               _context = context;
               _mapper = mapper;
               _internalService = internalService;
               _userService = userService;
               _permissionService = permissionService;
          }

          public async Task<Result<List<RelationshipDto>>> Handle(Relationship_AddOrUpdateCommand request, CancellationToken cancellationToken)
          {
               Relationship? obj = null;
               List<RelationshipDto> updatedSuccess = new List<RelationshipDto>();

               foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
               {
                    var tmp = new Relationship();
                    if (addOrUpdateRequest.Data == null)
                    {
                         throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
                    }
                    Validate(addOrUpdateRequest);


                    if (addOrUpdateRequest.Id == null)
                    {
                         await _permissionService.HasPermission(MenuType.Sale_MQH, FeatureType.CREATE, request.UserId, true);
                         obj = new Relationship()
                         {
                              CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId
                         };
                    }
                    else
                    {
                         await _permissionService.HasPermission(MenuType.Sale_MQH, FeatureType.UPDATE, request.UserId, true);
                         obj = await RelationshipService.GetRelationship(addOrUpdateRequest.Id.Value, _context);
                         PropertiesExtension.Copy(obj, tmp);
                    }

                    obj = (Relationship)_internalService.MapValueToObject(new Relationship(), addOrUpdateRequest.Data, obj);
                    obj.LastModifiedDate = DateTime.Now;
                    obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;
                    obj.ApplicationUserId = tmp.ApplicationUserId ?? obj.ApplicationUserId;
                    obj.CurrentRelationshipId = tmp.CurrentRelationshipId ?? obj.CurrentRelationshipId;
                    obj.TargetRelationshipId = tmp.TargetRelationshipId ?? obj.TargetRelationshipId;
                    obj.RelationshipStatusId = tmp.RelationshipStatusId ?? obj.RelationshipStatusId;
                    obj.ActualPoint = tmp.ActualPoint ?? obj.ActualPoint;

                    if (obj.CustomerId != null)
                    {
                         var existContract = _context.Customers.Where(s => s.Id == obj.CustomerId).FirstOrDefault();
                         if (existContract == null) throw new ApplicationException($"Không tìm thấy Công ty có id: {obj.CustomerId}");
                    }

                    if (obj.CurrentRelationshipId != null)
                    {
                         var level = await RelationshipService.GetLevel(obj.CurrentRelationshipId, _context);
                         obj.CurrentRelationship = level;
                         obj.CurrentRelationshipId = level.Id;
                    }

                    if (obj.TargetRelationshipId != null)
                    {
                         var level = await RelationshipService.GetLevel(obj.TargetRelationshipId, _context);
                         obj.TargetRelationship = level;
                         obj.TargetRelationshipId = level.Id;
                    }

                    var user = await _userService.FindAsync(obj.ApplicationUserId);

                    if (addOrUpdateRequest.Id == null)
                    {
                         var isManager = await _permissionService.ContainsPosition(user.Id, RolePositionEnum.MANAGER.ToString());
                         if (isManager)
                         {
                              await SetStatus(obj, RelationshipStatusEnum.CONFIRMED.ToString());
                         }
                         else
                         {
                              await SetStatus(obj, RelationshipStatusEnum.PENDING.ToString());
                         }
                         obj.ActualPoint = null;
                         _context.Relationships.Add(obj);
                    }
                    else
                    {
                         _context.Relationships.Update(obj);
                    }

                    updatedSuccess.Add(_mapper.Map<RelationshipDto>(obj));
               }

               await _context.SaveChangesAsync(cancellationToken);

               return Result<List<RelationshipDto>>.Success(updatedSuccess);
          }

          private async Task SetStatus(Relationship obj, string status)
          {
               var m_status = await RelationshipService.GetStatus(status, _context);
               obj.RelationshipStatus = m_status;
               obj.RelationshipStatusId = m_status.Id;
          }

          private void Validate(AddOrUpdateRequest addOrUpdateRequest)
          {
               StringHelper.IsValidLength(StringInfoConstant.NameLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "CustomerName"), true);
               StringHelper.IsValidLength(StringInfoConstant.NameLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "Position"), true);
               StringHelper.IsValidLength(StringInfoConstant.DesLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "Reason"), true);

               StringHelper.IsNumberOutOfRange(StringHelper.DictGetValue(addOrUpdateRequest.Data, "Point"), true);
               StringHelper.IsNumberOutOfRange(StringHelper.DictGetValue(addOrUpdateRequest.Data, "ActualPoint"), true);
          }
     }
}
