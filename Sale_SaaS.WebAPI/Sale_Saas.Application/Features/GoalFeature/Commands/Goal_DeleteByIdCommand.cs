using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.GoalFeature.Commands;

public record Goal_DeleteByIdCommand(Guid userId, DeleteRequest RequestData) : IRequest<Result<Boolean>>;
public class Goal_DeleteByIdCommandHandler : IRequestHandler<Goal_DeleteByIdCommand, Result<Boolean>>
{

	private readonly IApplicationDbContext _context;
	private readonly IMapper _mapper;
	private readonly IFeaturePermissionService _permissionService;
    private readonly IEventLogService _eventLogService;

    public Goal_DeleteByIdCommandHandler(IMapper mapper, IApplicationDbContext context, 
													IFeaturePermissionService permissionService, IEventLogService eventLogService)
	{
		_context = context;
		_mapper = mapper;
		_permissionService = permissionService;
		_eventLogService = eventLogService;
	}

	public async Task<Result<Boolean>> Handle(Goal_DeleteByIdCommand request, CancellationToken cancellationToken)
	{
		string result = string.Empty;

		if (request.RequestData.Ids == null) throw new ApplicationException("Không tìm thấy tham số Id.");
		List<Guid> ids = request.RequestData.Ids.Select(m => Guid.Parse(m)).ToList();
		var query = await _context.Goals.Include(s => s.GoalStatus).Where(m => ids.Contains(m.Id)).ToListAsync();
		if (query == null || query.Count == 0) throw new ApplicationException($"Không tìm thấy trong dữ liệu có Id: {string.Join(";", request.RequestData.Ids)}");

		foreach (var item in query)
		{
			var permission = await _permissionService.GetPositionByUser(request.RequestData.ApplicationUserId);
			if (permission == RolePositionEnum.EMPLOYEE.ToString())
			{
				if (item.GoalStatus == null || item.GoalStatus.Code != GoalStatusEnum.PENDING.ToString())
					throw new ApplicationException($"Mục tiêu đã được chốt không thể xóa");
			}
			item.DeleteFlag = true;
			item.LastModifiedDate = DateTime.Now;
			item.LastModifiedApplicationUserId = request.RequestData.ApplicationUserId;
		}

        var eventLog = await _eventLogService.Create("GoalFeature", "GoalFeature",
                                                "Goal_DeleteByIdCommand", request.userId);

        _context.Goals.UpdateRange(query);

		await _context.SaveChangesAsync(cancellationToken);

		return Result<Boolean>.Success(true);
	}
}
