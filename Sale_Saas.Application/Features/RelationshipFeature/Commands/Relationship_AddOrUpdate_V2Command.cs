using Sale_Saas.Application.Features.RelationshipFeature.Dto;
using Sale_Saas.Application.Features.RelationshipFeature.Services;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Domain.Constants.API;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.RelationshipFeature.Commands;

public record Relationship_AddOrUpdate_V2Command(Guid UserId, List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<RelationshipDto>>>;

public class Relationship_AddOrUpdate_V2CommandHandler : IRequestHandler<Relationship_AddOrUpdate_V2Command, Result<List<RelationshipDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IInternalService _internalService;
    private readonly IApplicationUserService _userService;
    private readonly IFeaturePermissionService _permissionService;
    public Relationship_AddOrUpdate_V2CommandHandler(IMapper mapper,
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

    public async Task<Result<List<RelationshipDto>>> Handle(Relationship_AddOrUpdate_V2Command request, CancellationToken cancellationToken)
    {
        Relationship? obj = null;
        List<RelationshipDto> updatedSuccess = new List<RelationshipDto>();

        foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
        {
            var tmp = new Relationship();
            Guid relationshipCustomerId = Guid.NewGuid();
            if (addOrUpdateRequest.Data == null)
            {
                throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
            }
            Validate(addOrUpdateRequest);

            if (addOrUpdateRequest.Data.ContainsKey("RelationshipCustomerId"))
            {
                var relationshipCustomer = await RelationshipService.GetById(Guid.Parse(addOrUpdateRequest.Data["RelationshipCustomerId"]), _context);
                if (relationshipCustomer == null)
                    throw new ApplicationException($"ID này không tồn tại");

                addOrUpdateRequest.Data["CustomerName"] = relationshipCustomer.Name ?? "";

                if (addOrUpdateRequest.Id == null)
                {
                    var relationship = await _context.Relationships.Where(x => x.DeleteFlag != true
                           && x.RelationshipCustomerId == Guid.Parse(addOrUpdateRequest.Data["RelationshipCustomerId"]))
                                                               .Include(s => s.YearToDate)
                                                               .Include(s => s.TargetRelationship)
                                                               .OrderByDescending(x => x.CreatedDate)
                                                               .FirstOrDefaultAsync();

                    if (relationship != null)
                    {
                        var newRelationshipLevel = await _context.RelationshipLevels.Where(x => x.DeleteFlag != true
                                                                        && x.Id == Guid.Parse(addOrUpdateRequest.Data["CurrentRelationshipId"]))
                                                                                .FirstOrDefaultAsync();

                        // A => C
                        // true: C => D
                        // not true: B => D
                        if (relationship.YearToDateId != relationship.TargetRelationshipId)
                            throw new ApplicationException($"Điều kiện hiện tại {relationship.YearToDate.Code} chưa đạt {relationship.TargetRelationship.Code}");
                        else if (relationship.YearToDate.SortOrder > newRelationshipLevel.SortOrder)
                        {
                            throw new ApplicationException($"Mức độ quan hệ khởi tạo {relationship.TargetRelationship.Code} không được bé hơn Mức độ quan hệ hiện tại {relationship.YearToDate.Code}");
                        }
                    }
                }
            }

            else
            {
                if (addOrUpdateRequest.Data.ContainsKey("CustomerName"))
                {
                    var isExists = await RelationshipService.IsExistsName(addOrUpdateRequest.Data["CustomerName"], _context);
                    if (isExists != null)
                        throw new ApplicationException($"Tên này đã tồn tại");

                    await RelationshipService.AddRelationshipCustomer(relationshipCustomerId, addOrUpdateRequest.Data["CustomerName"], request.UserId, _context);
                }
            }

            if (addOrUpdateRequest.Id == null)
            {
                await _permissionService.HasPermission(MenuType.Sale_MQH, FeatureType.CREATE, request.UserId, true);

                var gains = new Gains()
                {
                    CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId,
                };
                _context.Gains.Add(gains);
                await _context.SaveChangesAsync(cancellationToken);

                obj = new Relationship()
                {
                    GainsId = gains.Id,
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
            obj.RelationshipCustomerId = obj.RelationshipCustomerId ?? relationshipCustomerId;
            obj.CompletionDate = tmp.CompletionDate ?? obj.CompletionDate;

            if (addOrUpdateRequest.Id == null)
            {
                // initial YearToDate
                obj.YearToDateId = obj.CurrentRelationshipId;
                var level = await RelationshipService.GetLevel(obj.CurrentRelationshipId, _context);
                obj.YearToDate = level;

                obj.ApplicationUserId = obj.ApplicationUserId ?? request.UserId;
            }

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

            if (obj.YearToDateId != null)
            {
                var level = await RelationshipService.GetLevel(obj.YearToDateId, _context);
                obj.YearToDate = level;
                obj.YearToDateId = level.Id;
            }

            if (obj.RelationshipCustomerId != null)
            {
                var relationshipCustomer = await RelationshipService.GetRelationshipCustomer(obj.RelationshipCustomerId ?? Guid.Empty, _context);

                obj.RelationshipCustomer = relationshipCustomer;
                obj.RelationshipCustomerId = relationshipCustomer.Id;
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
                // tmp => current relationship
                // check obj: request vs tmp: entity get from database
                if (obj.YearToDateId != tmp.YearToDateId)
                {
                    if (tmp != null && tmp.CurrentRelationship != null && tmp.TargetRelationship != null && obj != null && obj.YearToDate != null
                        && tmp.CurrentRelationship.SortOrder.HasValue && tmp.TargetRelationship.SortOrder.HasValue && obj.YearToDate.SortOrder.HasValue)
                    {
                        // out of range
                        if (obj.YearToDate.SortOrder.Value < tmp.CurrentRelationship.SortOrder.Value
                                || obj.YearToDate.SortOrder.Value > tmp.TargetRelationship.SortOrder.Value)
                        {
                            throw new ApplicationException($"Quan hệ hiện tại {obj.YearToDate.Code} nằm ngoài phạm vi quan hệ ban đầu và quan hệ mục tiêu");
                        }

                        // in range
                        // update history
                        await RelationshipService.AddHistory(obj.Id, tmp.YearToDateId ?? Guid.Empty, obj.YearToDateId ?? Guid.Empty, request.UserId, _context);
                    }
                }

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
