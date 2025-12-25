using Sale_Saas.Application.Features.GoalFeature.Dto;
using Sale_Saas.Application.Features.GoalFeature.Services;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.Notification;
using Sale_Saas.Domain.Constants.API;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.GoalFeature.Commands
{
    public record Goal_AddOrUpdateCommand(Guid userId, AddOrUpdateMediatrRequest requestData) : IRequest<Result<List<GoalDto>>>;

    public class Goal_AddOrUpdateCommandHandler : IRequestHandler<Goal_AddOrUpdateCommand, Result<List<GoalDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;
        private readonly IApplicationUserService _UserService;
        private readonly IFeaturePermissionService _PermissionService;
        private readonly INotificationService _notification;
        private readonly IEventLogService _eventLogService;

        public Goal_AddOrUpdateCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService,
                                                IFeaturePermissionService permissionService,
                                                IApplicationUserService UserService,
                                                INotificationService notification, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;
            _PermissionService = permissionService;
            _UserService = UserService;
            _notification = notification;
            _eventLogService = eventLogService;
        }

        public async Task<Result<List<GoalDto>>> Handle(Goal_AddOrUpdateCommand request, CancellationToken cancellationToken)
        {
            Goal? obj = null;
            List<Guid> managerIds = new List<Guid>();
            List<GoalDto> updatedSuccess = new List<GoalDto>();
            List<PushNotificationRequest> notifications = new List<PushNotificationRequest>();

            var userModified = await _UserService.FindAsync(request.requestData.UserId);
            var index = 0;
            foreach (AddOrUpdateRequest addOrUpdateRequest in request.requestData.List)
            {
                Goal tmp = new Goal();
                if (addOrUpdateRequest.Data == null)
                {
                    throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
                }

                ValidateData(addOrUpdateRequest);

                if (addOrUpdateRequest.Id == null)
                {
                    await _PermissionService.HasPermission(MenuType.Sale_MT, FeatureType.CREATE, request.requestData.UserId, true);
                    obj = new Goal()
                    {
                        CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId
                    };
                }
                else
                {
                    await _PermissionService.HasPermission(MenuType.Sale_MT, FeatureType.UPDATE, request.requestData.UserId, true);
                    obj = await GoalService.GetGoal(addOrUpdateRequest.Id.Value, _context);
                    PropertiesExtension.Copy(obj, tmp);
                }

                obj = (Goal)_internalService.MapValueToObject(new Goal(), addOrUpdateRequest.Data, obj);
                obj.LastModifiedDate = DateTime.Now;
                obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;
                obj.SuggestTargetKPI = tmp.SuggestTargetKPI ?? obj.SuggestTargetKPI;
                obj.SuggestTargetPoint = tmp.SuggestTargetPoint ?? obj.SuggestTargetPoint;
                obj.SuggestEndTime = tmp.SuggestEndTime ?? obj.SuggestEndTime;
                obj.SendExpiredNotification = tmp.SendExpiredNotification ?? false;
                obj.UserSuggestId = tmp.UserSuggestId ?? obj.UserSuggestId;
                obj.GoalStatusId = tmp.GoalStatusId ?? obj.GoalStatusId;
                obj.GoalStatus = tmp.GoalStatus ?? obj.GoalStatus;
                obj.Review = tmp.Review ?? obj.Review;
                obj.SuggestStartTime = tmp.SuggestStartTime ?? obj.SuggestStartTime;
                if (obj.ApplicationRoleId != null)
                {
                    obj.ApplicationRoleId = tmp.ApplicationRoleId ?? obj.ApplicationRoleId;
                    obj.ApplicationRole = tmp.ApplicationRole ?? obj.ApplicationRole;
                }
                else
                {
                    var userRole = await _UserService.GetApplicationRolesByUserIdAsync(obj.UserSuggestId.Value);
                    if (userRole != null)
                    {
                        obj.ApplicationRoleId = userRole[0].Id;
                    }
                }

                if (obj.EndTime != null)
                {
                    obj.EndTime = DateTimeExtensions.SetToEndOfDay(obj.EndTime);
                }
                if (obj.EndTime != null && obj.StartTime != null && obj.StartTime > obj.EndTime)
                {
                    throw new ApplicationException($"Thời gian bắt đầu không thể nhỏ hơn thời gian kết thúc");
                }
                var existUser = await _UserService.FindAsync(obj.UserSuggestId);
                if (addOrUpdateRequest.Id == null)
                {
                    await SetStatus(obj, tmp, existUser);
                    _context.Goals.Add(obj);
                    await CreateNotification(notifications, obj, managerIds, request.requestData.TenantId);

                }
                else
                {
                    _context.Goals.Update(obj);
                    ModifiedNotification(notifications, obj, userModified, request.requestData.TenantId);
                    // Remove List Custoner from Relatinship
                    var listRelationExisted = await _context.Relationships.Where(x => x.GoalId == obj.Id).ToListAsync();
                    if (listRelationExisted.Any())
                    {
                        _context.Relationships.RemoveRange(listRelationExisted);
                        await _context.SaveChangesAsync(cancellationToken);
                    }

                }
                updatedSuccess.Add(_mapper.Map<GoalDto>(obj));
                //Add List Custoner to Relatinship
                if (addOrUpdateRequest.Customers != null && addOrUpdateRequest.Customers.Any())
                {
                    updatedSuccess[index].Customers = new List<CustomerByGoal>();
                    foreach (var item in addOrUpdateRequest.Customers)
                    {
                        var newRelation = new Relationship()
                        {
                            CustomerName = item.FullName,
                            ApplicationUserId = request.requestData.UserId,
                            GoalId = obj.Id,
                        };
                        var resultRela = _context.Relationships.Add(newRelation);
                        updatedSuccess[index].Customers.Add(new CustomerByGoal() { Id = newRelation.Id, FullName = newRelation.CustomerName });
                    }

                }
                index += 1;
            }

            var eventLog = await _eventLogService.Create("GoalFeature", "GoalFeature",
                                                            "Goal_AddOrUpdateCommand", request.userId);

            await _context.SaveChangesAsync(cancellationToken);
            await _notification.PushAsync(notifications);

            return Result<List<GoalDto>>.Success(updatedSuccess);
        }


        private void ValidateData(AddOrUpdateRequest addOrUpdateRequest)
        {
            if (string.IsNullOrEmpty(StringHelper.DictGetValue(addOrUpdateRequest.Data!, "UserSuggestId")))
            {
                throw new ApplicationException($"Không có dữ liệu tài khoản đề xuất gửi đến máy chủ");
            }

            StringHelper.IsValidLength(StringInfoConstant.DesLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "Criteria"), true);
            StringHelper.IsValidLength(StringInfoConstant.DesLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "Review"), true);
            //StringHelper.IsValidLength(StringInfoConstant.DesLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "Calculate"), true);

            StringHelper.IsNumberOutOfRange(StringHelper.DictGetValue(addOrUpdateRequest.Data, "TargetKPI"), true);
            StringHelper.IsNumberOutOfRange(StringHelper.DictGetValue(addOrUpdateRequest.Data, "TargetPoint"), true);
        }

        private async Task SetStatus(Goal obj, Goal tmp, ApplicationUser user)
        {
            var position = await _PermissionService.GetPositionByUser(user.Id);
            if (position == RolePositionEnum.MANAGER.ToString())
            {
                tmp.GoalStatus = await _context.GoalStatuses
                                    .FirstOrDefaultAsync(s => s.Code == GoalStatusEnum.PROCESSING.ToString() && !s.DeleteFlag);
            }
            else if (position == RolePositionEnum.EMPLOYEE.ToString() && obj.EndTime != null && obj.EndTime < DateTime.Now)
            {
                tmp.GoalStatus = await _context.GoalStatuses
                                    .FirstOrDefaultAsync(s => s.Code == GoalStatusEnum.PROCESSING.ToString() && !s.DeleteFlag);
            }
            else
            {
                tmp.GoalStatus = await _context.GoalStatuses
                                    .FirstOrDefaultAsync(s => s.Code == GoalStatusEnum.PENDING.ToString() && !s.DeleteFlag);
            }

            obj.GoalStatusId = tmp.GoalStatus?.Id ?? obj.GoalStatusId;
            obj.Review = "";
        }

        private void ModifiedNotification(List<PushNotificationRequest> notifications, Goal obj, ApplicationUser user, string tenant)
        {
            try
            {
                if (user.Id != obj.UserSuggestId)
                {
                    var noti = new PushNotificationRequest(
                        NotificationAction.GoalMsg.Title,
                        NotificationAction.GoalMsg.Modified(user.FullName ?? "", DateTime.Now),
                        NotificationAction.GoalMsg.Type,
                        obj.Id.ToString(),
                        obj.UserSuggestId,
                        tenant
                    );
                    notifications.Add(noti);
                }
            }
            catch (Exception ex) { }
        }

        private async Task CreateNotification(List<PushNotificationRequest> notifications, Goal obj, List<Guid> ids, string tenant)
        {
            try
            {
                var position = await _PermissionService.GetPositionByUser(obj.UserSuggest!.Id);
                if (position == RolePositionEnum.EMPLOYEE.ToString())
                {
                    if (ids.Count == 0)
                    {
                        ids = await _UserService.GetUserIdByRolePositionAndPermission(RolePositionEnum.MANAGER.ToString(), MenuType.Sale_MT, FeatureType.CREATE);
                    }
                    foreach (var id in ids)
                    {
                        var noti = new PushNotificationRequest(
                            NotificationAction.GoalMsg.Title,
                            NotificationAction.GoalMsg.Create(obj.UserSuggest!.FullName ?? "", DateTime.Now),
                            NotificationAction.GoalMsg.Type,
                            obj.Id.ToString(),
                            id, tenant
                        );
                        notifications.Add(noti);
                    }
                }
            }
            catch (Exception ex) { }
        }
    }
}
