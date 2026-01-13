using Sale_Saas.Application.Features.GoalFeature.Dto;
using Sale_Saas.Application.Features.GoalFeature.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Models.Goal;
using Sale_Saas.Domain.Constants.API;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.GoalFeature.Commands;

public record Goal_UpdateStatusByIdCommand(Guid userId, UpdateStatusGoalRequest RequestData) : IRequest<Result<GoalDto>>;
public class Goal_UpdateStatusByIdCommandHandler : IRequestHandler<Goal_UpdateStatusByIdCommand, Result<GoalDto>>
{

    private readonly IApplicationDbContext _context;
    private readonly IFeaturePermissionService _permissionService;
    private readonly IMapper _mapper;
    private readonly IEventLogService _eventLogService;

    public Goal_UpdateStatusByIdCommandHandler(
        IMapper mapper, IApplicationDbContext context, IFeaturePermissionService permissionService, IEventLogService eventLogService)
    {
        _context = context;
        _mapper = mapper;
        _permissionService = permissionService;
        _eventLogService = eventLogService;
    }

    public async Task<Result<GoalDto>> Handle(Goal_UpdateStatusByIdCommand request, CancellationToken cancellationToken)
    {
        string result = string.Empty;
        Goal? data = await GoalService.GetGoal(request.RequestData.Id, _context);
        GoalStatus? status = await GoalService.GetStatus(request.RequestData.Status, _context);
        Validate(request);

        Guid applicationUserId = request.RequestData.ApplicationUserId;
        decimal? suggestTargetKPI = request.RequestData.SuggestTargetKPI;
        decimal? suggestTargetPoint = request.RequestData.SuggestTargetPoint;
        decimal? targetKPI = request.RequestData.TargetKPI;
        decimal? targetPoint = request.RequestData.TargetPoint;
        string? suggestEndTime = request.RequestData.SuggestEndTime;
        string? criteria = request.RequestData.Criteria;
        string? calculate = request.RequestData.Calculate;
        DateTime? endTime = request.RequestData.EndTime;
        string? suggestStartTime = request.RequestData.SuggestStartTime;

        switch (Enum.Parse<GoalStatusEnum>(request.RequestData.Status ?? ""))
        {
            case GoalStatusEnum.UPDATED:
                // *********** Chấp nhận yêu cầu chỉnh sửa
                if (data.GoalStatus!.Code == GoalStatusEnum.REQUEST.ToString())
                {
                    status = await handleAcceptRequest(data, suggestTargetKPI, suggestTargetPoint, suggestEndTime, suggestStartTime);
                }
                // *********** Điều chỉnh mục tiêu
                else if (data.GoalStatus!.Code == GoalStatusEnum.PENDING.ToString())
                {
                    status = await handleDieuChinhMucTieu(data, status, applicationUserId, criteria, calculate, targetKPI, targetPoint, endTime);
                }
                // *********** Chỉnh sửa mục tiêu
                else if (data.GoalStatus!.Code == GoalStatusEnum.PROCESSING.ToString())
                {
                    status = await handleChinhSuaMucTieu(data, status);
                }
                else throw new ApplicationException($"Trạng thái không hợp lệ: {request.RequestData.Status}");
                break;
            case GoalStatusEnum.REQUEST:
                // *********** Gửi yêu cầu chỉnh sửa
                handleRequest(data, status, suggestTargetKPI, suggestTargetPoint, suggestEndTime);
                break;
            case GoalStatusEnum.PROCESSING:
                // *********** Thực hiện
                handleProcessing(data, status);
                break;
            default:
                break;
        }

        data.GoalStatus = status;
        data.LastModifiedDate = DateTime.Now;
        data.LastModifiedApplicationUserId = request.RequestData.ApplicationUserId;

        var eventLog = await _eventLogService.Create("GoalFeature", "GoalFeature",
                                                "Goal_UpdateStatusByIdCommand", request.userId);
        _context.Goals.UpdateRange(data);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<GoalDto>.Success(_mapper.Map<GoalDto>(data));
    }

    private void Validate(Goal_UpdateStatusByIdCommand request)
    {
        StringHelper.IsNumberOutOfRange(request.RequestData.SuggestTargetKPI.HasValue ? request.RequestData.SuggestTargetKPI.ToString() : "", true);
        StringHelper.IsNumberOutOfRange(request.RequestData.SuggestTargetPoint.HasValue ? request.RequestData.SuggestTargetPoint.ToString() : "", true);

        StringHelper.IsValidLength(StringInfoConstant.DesLimit, request.RequestData.Criteria, true);
        StringHelper.IsValidLength(StringInfoConstant.DesLimit, request.RequestData.Calculate, true);
    }

    private void handleProcessing(Goal data, GoalStatus status)
    {
        var validStatus = new[] { GoalStatusEnum.REQUEST.ToString(), GoalStatusEnum.UPDATED.ToString(), GoalStatusEnum.PENDING.ToString() };

        data.SuggestEndTime = null;
        data.SuggestTargetKPI = null;
        data.SuggestTargetPoint = null;

        if (!validStatus.Contains(data.GoalStatus!.Code))
        {
            throw new ApplicationException($"Trạng thái không hợp lệ: {status.Code}");
        }
    }

    private void handleRequest(Goal data, GoalStatus status, decimal? SuggestTargetKPI, decimal? SuggestTargetPoint, string? SuggestEndTime)
    {
        if (data.GoalStatus!.Code != GoalStatusEnum.PROCESSING.ToString() && data.GoalStatus!.Code != GoalStatusEnum.REQUEST.ToString())
        {
            throw new ApplicationException($"Trạng thái không hợp lệ: {status.Code}");
        }
        data.SuggestTargetKPI = SuggestTargetKPI;
        data.SuggestTargetPoint = SuggestTargetPoint;
        if (!string.IsNullOrEmpty(SuggestEndTime))
        {
            var Endtime = StringHelper.ToDatetime(SuggestEndTime);
            if (Endtime < data.StartTime)
            {
                throw new ApplicationException("Thời gian kết thúc không thể nhỏ hơn thời gian bắt đầu");
            }
            data.SuggestEndTime = Endtime;
        }
    }

    private async Task<GoalStatus> handleAcceptRequest(Goal data, decimal? SuggestTargetKPI, decimal? SuggestTargetPoint, string? SuggestEndTime, string? SuggestStartTime)
    {
        data.TargetKPI = SuggestTargetKPI > 0 ? SuggestTargetKPI : data.TargetKPI;
        data.TargetPoint = SuggestTargetPoint > 0 ? SuggestTargetPoint : data.TargetPoint;
        if (!string.IsNullOrEmpty(SuggestStartTime))
        {
            var startTime = StringHelper.ToDatetime(SuggestStartTime);
            data.SuggestStartTime = DateTimeExtensions.SetToEndOfDay(startTime);
        }
        if (!string.IsNullOrEmpty(SuggestEndTime))
        {
            var Endtime = StringHelper.ToDatetime(SuggestEndTime);
            if (Endtime < data.StartTime)
            {
                throw new ApplicationException("Thời gian kết thúc không thể nhỏ hơn thời gian bắt đầu");
            }
            data.EndTime = DateTimeExtensions.SetToEndOfDay(Endtime);
        }
        data.SuggestStartTime = null;
        data.SuggestEndTime = null;
        data.SuggestTargetKPI = null;
        data.SuggestTargetPoint = null;
        return await GoalService.GetStatus(GoalStatusEnum.PROCESSING.ToString(), _context);
    }

    private async Task<GoalStatus> handleDieuChinhMucTieu(
        Goal data, GoalStatus status, Guid applicationUserId,
        string? criteria, string? calculate, decimal? targetKPI, decimal? targetPoint, DateTime? endTime)
    {
        data.CriteriaName = criteria ?? data.CriteriaName;
        data.TargetKPI = targetKPI > 0 ? targetKPI : data.TargetKPI;
        data.TargetPoint = targetPoint > 0 ? targetPoint : data.TargetPoint;

        if (endTime != null)
        {
            var Endtime = DateTimeExtensions.SetToEndOfDay(endTime);
            if (Endtime < data.StartTime)
            {
                throw new ApplicationException("Thời gian kết thúc không thể nhỏ hơn thời gian bắt đầu");
            }
            data.EndTime = Endtime;
        }

        data.Calculate = calculate ?? data.Calculate;
        data.SuggestStartTime = null;
        data.SuggestEndTime = null;
        data.SuggestTargetKPI = null;
        data.SuggestTargetPoint = null;

        bool isManager = await _permissionService.ContainsPosition((Guid)applicationUserId, RolePositionEnum.MANAGER.ToString());
        if (isManager == false && data.UserSuggestId == applicationUserId)
        {
            return await GoalService.GetStatus(GoalStatusEnum.PENDING.ToString(), _context);
        }
        return await GoalService.GetStatus(GoalStatusEnum.PROCESSING.ToString(), _context);
    }

    private async Task<GoalStatus> handleChinhSuaMucTieu(
        Goal data, GoalStatus status)
    {
        bool isManager = await _permissionService.ContainsPosition((Guid)data.UserSuggestId!, RolePositionEnum.MANAGER.ToString());
        if (isManager == true)
        {
            return await GoalService.GetStatus(GoalStatusEnum.PROCESSING.ToString(), _context);
        }
        else
        {
            return status;
        }
    }
}
