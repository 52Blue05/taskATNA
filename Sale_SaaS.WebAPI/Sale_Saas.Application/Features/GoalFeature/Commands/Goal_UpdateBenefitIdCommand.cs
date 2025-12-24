using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.GoalFeature.Dto;
using Sale_Saas.Application.Features.GoalFeature.Services;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.Notification;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.GoalFeature.Commands
{
	public record Goal_UpdateBenefitIdCommand(Guid userId, UpdateBenefitIdRequest requestData) : IRequest<Result<List<GoalDto>>>;

	public class Goal_UpdateBenefitIdCommandHandler : IRequestHandler<Goal_UpdateBenefitIdCommand, Result<List<GoalDto>>>
	{
		private readonly IApplicationDbContext _context;
		private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public Goal_UpdateBenefitIdCommandHandler(IMapper mapper,
												IApplicationDbContext context,
												IInternalService internalService,
												INotificationService notification, IEventLogService eventLogService)
		{
			_context = context;
			_mapper = mapper;
			_eventLogService = eventLogService;
		}

		public async Task<Result<List<GoalDto>>> Handle(Goal_UpdateBenefitIdCommand request, CancellationToken cancellationToken)
		{
			List<GoalDto> updatedSuccess = new List<GoalDto>();
            if (request.requestData.BenefitId == null)
            {
                throw new ApplicationException($"Mã quyền lợi không đúng.");
            }
            var listGoal =await _context.Goals.Where(x => request.requestData.GoalIds.Any(p => p == x.Id)).ToListAsync();
            if (!listGoal.Any())
            {
                throw new ApplicationException($"Không tìm thấy mục tiêu.");
            }
            foreach (var goal in listGoal)
			{
				goal.BenefitId = request.requestData.BenefitId;
				goal.LastModifiedApplicationUserId = request.userId;
                goal.LastModifiedDate = DateTime.UtcNow;
                updatedSuccess.Add(_mapper.Map<GoalDto>(goal));
			}

            var eventLog = await _eventLogService.Create("GoalFeature", "GoalFeature",
                                                "Goal_UpdateBenefitIdCommand", request.userId);

            await _context.SaveChangesAsync(cancellationToken);
			return Result<List<GoalDto>>.Success(updatedSuccess);
		}
	}
}
