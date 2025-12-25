using Sale_Saas.Application.Features.GoalFeature.Dto;
using Sale_Saas.Application.Features.GoalFeature.Services;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.Notification;
using Sale_Saas.Domain.Constants.API;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.GoalFeature.Commands;

public record GoalMobile_AddOrUpdateCommand(Guid userId, AddOrUpdateMediatrRequest requestData) : IRequest<Result<List<GoalDto>>>;

public class GoalMobile_AddOrUpdateCommandHandler : IRequestHandler<GoalMobile_AddOrUpdateCommand, Result<List<GoalDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IInternalService _internalService;
    private readonly IApplicationUserService _UserService;
    private readonly IFeaturePermissionService _PermissionService;
    private readonly INotificationService _notification;
    private readonly IEventLogService _eventLogService;
    private readonly IApplicationRoleService _roleService;

    public GoalMobile_AddOrUpdateCommandHandler(IMapper mapper,
                                            IApplicationDbContext context,
                                            IInternalService internalService,
                                            IFeaturePermissionService permissionService,
                                            IApplicationUserService UserService,
                                            IApplicationRoleService roleService,
                                            INotificationService notification, IEventLogService eventLogService)
    {
        _context = context;
        _mapper = mapper;
        _internalService = internalService;
        _PermissionService = permissionService;
        _UserService = UserService;
        _notification = notification;
        _eventLogService = eventLogService;
        _roleService = roleService;
    }

    public async Task<Result<List<GoalDto>>> Handle(GoalMobile_AddOrUpdateCommand request, CancellationToken cancellationToken)
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

            if (obj.CriteriaId != null)
            {
                var criteria = await GoalService.GetCriteria(obj.CriteriaId != null ? obj.CriteriaId : tmp.CriteriaId, _context);
                obj.Criteria = criteria;
                obj.CriteriaId = criteria.Id;
                obj.CriteriaName = criteria.Name;
                obj.CriteriaType = criteria.Code;
            }

            if (obj.ApplicationRoleId != null)
            {
                var role = await GoalService.GetApplicationRole(obj.ApplicationRoleId, _context);
                obj.ApplicationRole = role;
                obj.ApplicationRoleId = role.Id;
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

                // Remove List Customer from Relationship
                if (obj.CriteriaType != CriteriaEnum.CRITERIA_CUSTOMER.ToString())
                {
                    var listRelationExisted = await _context.Relationships.Where(x => x.GoalId == obj.Id).ToListAsync();

                    if (listRelationExisted.Any())
                    {
                        foreach (var relation in listRelationExisted)
                        {
                            relation.DeleteFlag = true;
                            relation.LastModifiedApplicationUserId = request.userId;
                            relation.LastModifiedDate = DateTime.Now;

                            _context.Relationships.Update(relation);

                            var relationshipCustomer = await _context.RelationshipCustomers.Where(x => x.Id == relation.RelationshipCustomerId).FirstOrDefaultAsync();

                            if (relationshipCustomer == null)
                            {
                                throw new ApplicationException("Không tìm thấy khách hàng");
                            }

                            relationshipCustomer.DeleteFlag = true;
                            relationshipCustomer.LastModifiedApplicationUserId = request.userId;
                            relationshipCustomer.LastModifiedDate = DateTime.Now;

                            _context.RelationshipCustomers.Update(relationshipCustomer);
                        }

                        await _context.SaveChangesAsync(cancellationToken);
                    }
                }
            }

            updatedSuccess.Add(_mapper.Map<GoalDto>(obj));

            //Add List Customer to Relationship
            if (obj.CriteriaType == CriteriaEnum.CRITERIA_CUSTOMER.ToString()
                && addOrUpdateRequest.Customers != null && addOrUpdateRequest.Customers.Any())
            {
                var listRelationshipRequestIds = addOrUpdateRequest.Customers.Where(x => x.Id != null)
                                                                             .Select(x => x.Id)
                                                                             .ToList();

                var listRelationshipCustomerRequestIds = await _context.Relationships.Where(x => x.DeleteFlag != true
                                                                                            && listRelationshipRequestIds != null
                                                                                            && listRelationshipRequestIds.Contains(x.Id))
                                                                                    .AsNoTracking()
                                                                                    .Select(x => x.RelationshipCustomerId)
                                                                                    .ToListAsync();

                var listRelationshipByCustomer = await _context.Relationships.Where(x => x.DeleteFlag != true
                                                                                      && x.GoalId == obj.Id
                                                                                      && !listRelationshipRequestIds.Contains(x.Id))
                                                                             .ToListAsync();

                foreach (var relationshipByCustomer in listRelationshipByCustomer)
                {
                    // remove customer
                    relationshipByCustomer.DeleteFlag = true;
                    relationshipByCustomer.LastModifiedDate = DateTime.Now;

                    _context.Relationships.Update(relationshipByCustomer);

                    // remove relationship
                    var relationshipCustomer = await _context.RelationshipCustomers.Where(x => x.Id == relationshipByCustomer.RelationshipCustomerId
                                                                                            && x.DeleteFlag != true)
                                                                                    .FirstOrDefaultAsync();

                    if (relationshipCustomer == null)
                    {
                        continue;
                    }

                    relationshipCustomer.DeleteFlag = true;
                    relationshipCustomer.LastModifiedDate = DateTime.Now;

                    _context.RelationshipCustomers.Update(relationshipCustomer);

                    await _context.SaveChangesAsync(cancellationToken);
                }

                updatedSuccess[index].Customers = new List<CustomerByGoal>();
                foreach (var item in addOrUpdateRequest.Customers)
                {
                    if (item.Id == null || item.Id == Guid.Empty)
                    {
                        var relationshipCustomer = new RelationshipCustomer()
                        {
                            Name = item.FullName,
                        };

                        _context.RelationshipCustomers.Add(relationshipCustomer);

                        await _context.SaveChangesAsync(cancellationToken);

                        var newRelationship = new Relationship()
                        {
                            CustomerName = relationshipCustomer.Name,
                            RelationshipCustomerId = relationshipCustomer.Id,
                            ApplicationUserId = request.requestData.UserId,
                            GoalId = obj.Id,
                        };

                        _context.Relationships.Add(newRelationship);

                        await _context.SaveChangesAsync(cancellationToken);

                        updatedSuccess[index].Customers.Add(new CustomerByGoal() { Id = newRelationship.Id, FullName = newRelationship.CustomerName });
                    }
                    else
                    {
                        // relationship customer has id
                        // get list relationship customer
                        var listRelationshipCustomerIds = await _context.Relationships.Where(x => x.GoalId == obj.Id && x.DeleteFlag != true
                                                                                                && x.Id == item.Id)
                                                                                      .AsNoTracking()
                                                                                      .Select(x => x.RelationshipCustomerId)
                                                                                      .ToListAsync();

                        // check update
                        foreach (var relationshipCustomerId in listRelationshipCustomerIds)
                        {
                            var relationshipCustomer = await _context.RelationshipCustomers.Where(x => x.Id == relationshipCustomerId && x.DeleteFlag != true)
                                                                                           .FirstOrDefaultAsync();
                            if (relationshipCustomer == null)
                            {
                                throw new ApplicationException("Không tìm thấy khách hàng");
                            }

                            if (listRelationshipCustomerRequestIds.Contains(relationshipCustomerId))
                            {

                                relationshipCustomer.Name = item.FullName ?? relationshipCustomer.Name;
                                relationshipCustomer.LastModifiedDate = DateTime.Now;

                                _context.RelationshipCustomers.Update(relationshipCustomer);

                                updatedSuccess[index].Customers.Add(new CustomerByGoal() { Id = item.Id, FullName = relationshipCustomer.Name ?? "" });
                            }


                            await _context.SaveChangesAsync(cancellationToken);
                        }
                    }
                }

            }
            index += 1;
        }

        var eventLog = await _eventLogService.Create("GoalFeature", "GoalFeature",
                                                        "GoalMobile_AddOrUpdateCommand", request.userId);

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
        var roleName = await _roleService.GetRoleById(obj.ApplicationRoleId ?? Guid.Empty);

        if (position == RolePositionEnum.MANAGER.ToString() && roleName != RolePositionEnum.EMPLOYEE.ToString())
        {
            tmp.GoalStatus = await _context.GoalStatuses
                                //.AsNoTracking()
                                .FirstOrDefaultAsync(s => s.Code == GoalStatusEnum.PROCESSING.ToString() && !s.DeleteFlag);
        }
        else if (position == RolePositionEnum.EMPLOYEE.ToString() && obj.EndTime != null && obj.EndTime < DateTime.Now)
        {
            tmp.GoalStatus = await _context.GoalStatuses
                                //.AsNoTracking()
                                .FirstOrDefaultAsync(s => s.Code == GoalStatusEnum.PROCESSING.ToString() && !s.DeleteFlag);
        }
        else
        {
            tmp.GoalStatus = await _context.GoalStatuses
                                //.AsNoTracking()
                                .FirstOrDefaultAsync(s => s.Code == GoalStatusEnum.PENDING.ToString() && !s.DeleteFlag);
        }

        obj.GoalStatus = tmp.GoalStatus;
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
