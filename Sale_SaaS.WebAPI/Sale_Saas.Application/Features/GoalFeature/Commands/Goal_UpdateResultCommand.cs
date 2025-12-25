using Sale_Saas.Application.Features.GoalFeature.Dto;
using Sale_Saas.Application.Features.GoalFeature.Services;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.Notification;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.GoalFeature.Commands
{
	public record Goal_UpdateResultCommand(Guid userId, AddOrUpdateMediatrRequest requestData) : IRequest<Result<List<GoalDto>>>;

	public class Goal_UpdateResultCommandHandler : IRequestHandler<Goal_UpdateResultCommand, Result<List<GoalDto>>>
	{
		private readonly IApplicationDbContext _context;
		private readonly IMapper _mapper;
		private readonly IInternalService _internalService;
		private readonly INotificationService _notification;
        private readonly IEventLogService _eventLogService;

        public Goal_UpdateResultCommandHandler(IMapper mapper,
												IApplicationDbContext context,
												IInternalService internalService,
												INotificationService notification, IEventLogService eventLogService)
		{
			_context = context;
			_mapper = mapper;
			_internalService = internalService;
			_notification = notification;
			_eventLogService = eventLogService;
		}

		public async Task<Result<List<GoalDto>>> Handle(Goal_UpdateResultCommand request, CancellationToken cancellationToken)
		{
			Goal? obj = null;
			List<GoalDto> updatedSuccess = new List<GoalDto>();
			List<PushNotificationRequest> notifications = new List<PushNotificationRequest>();

			foreach (AddOrUpdateRequest addOrUpdateRequest in request.requestData.List)
			{
				if (addOrUpdateRequest.Data == null || addOrUpdateRequest.Id == null)
				{
					throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
				}
				Validate(addOrUpdateRequest);
				obj = await GoalService.GetGoal(addOrUpdateRequest!.Id.Value, _context);

				var tmp = (GoalUpdateResult)_internalService.MapValueToObject(new GoalUpdateResult(), addOrUpdateRequest.Data, new GoalUpdateResult());
				obj.ActualKPI = tmp.ActualKPI;
				obj.ActualPoint = tmp.ActualPoint;
				obj.LastModifiedDate = DateTime.Now;
				obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;

				await UpdateGoalStatus(obj);

				_context.Goals.Update(obj);
				updatedSuccess.Add(_mapper.Map<GoalDto>(obj));
				UpdateResultNotification(notifications, obj, request.requestData.TenantId);
			}

            var eventLog = await _eventLogService.Create("GoalFeature", "GoalFeature",
                                                "Goal_UpdateResultCommand", request.userId);

            await _context.SaveChangesAsync(cancellationToken);
			await _notification.PushAsync(notifications);
			return Result<List<GoalDto>>.Success(updatedSuccess);
		}

		private void Validate(AddOrUpdateRequest addOrUpdateRequest)
		{
			StringHelper.IsNumberOutOfRange(StringHelper.DictGetValue(addOrUpdateRequest.Data, "ActualPoint"), true);
			StringHelper.IsNumberOutOfRange(StringHelper.DictGetValue(addOrUpdateRequest.Data, "ActualKPI"), true);
		}

		private async Task UpdateGoalStatus(Goal obj)
		{
			if (obj.ActualKPI >= obj.TargetKPI && obj.ActualPoint >= obj.TargetPoint)
			{
				obj.GoalStatus = await _context.GoalStatuses.FirstOrDefaultAsync(s => !s.DeleteFlag && s.Code == GoalStatusEnum.COMPLETED.ToString());
			}
			else if (obj.ActualKPI < obj.TargetKPI || obj.ActualPoint < obj.TargetPoint)
			{
				obj.GoalStatus = obj.EndTime < DateTime.Now
					? await _context.GoalStatuses.FirstOrDefaultAsync(s => !s.DeleteFlag && s.Code == GoalStatusEnum.FAILED.ToString())
					: await _context.GoalStatuses.FirstOrDefaultAsync(s => !s.DeleteFlag && s.Code == GoalStatusEnum.PROCESSING.ToString());
			}
		}

		private void UpdateResultNotification(List<PushNotificationRequest> notifications, Goal obj,string tenant)
		{
			PushNotificationRequest noti = new PushNotificationRequest(
						NotificationAction.GoalMsg.Title,
						NotificationAction.GoalMsg.UpdateResult(DateTime.Now),
						NotificationAction.GoalMsg.Type,
						obj.Id.ToString(),
						obj.UserSuggestId,
						tenant
			);
			notifications.Add(noti);
		}
	}
}
