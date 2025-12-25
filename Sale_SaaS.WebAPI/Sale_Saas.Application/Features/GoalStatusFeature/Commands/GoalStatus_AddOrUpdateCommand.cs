using Sale_Saas.Application.Features.GoalStatusFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.GoalStatusFeature.Commands
{
    public record GoalStatus_AddOrUpdateCommand(Guid userId, List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<GoalStatusDto>>>;

    public class GoalStatus_AddOrUpdateCommandHandler : IRequestHandler<GoalStatus_AddOrUpdateCommand, Result<List<GoalStatusDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;
        private readonly IEventLogService _eventLogService;

        public GoalStatus_AddOrUpdateCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;
            _eventLogService = eventLogService;
        }

        public async Task<Result<List<GoalStatusDto>>> Handle(GoalStatus_AddOrUpdateCommand request, CancellationToken cancellationToken)
        {
            GoalStatus? obj = null;
            List<GoalStatusDto> updatedSuccess = new List<GoalStatusDto>();
            foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
            {
                if (addOrUpdateRequest.Data == null)
                {
                    throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
                }

                if (addOrUpdateRequest.Id == null)
                {
                    obj = new GoalStatus()
                    {
                        CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId
                    };
                }
                else
                {
                    obj = await _context.GoalStatuses.FindAsync(addOrUpdateRequest.Id.Value);

                    if (obj == null)
                        throw new ApplicationException($"Không tìm thấy trạng thái có id: {addOrUpdateRequest.Id.Value}");

                }

                obj = (GoalStatus)_internalService.MapValueToObject(new GoalStatus(), addOrUpdateRequest.Data, obj);
                obj.LastModifiedDate = DateTime.Now;
                obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;

                if (addOrUpdateRequest.Id == null)
                {
                    _context.GoalStatuses.Add(obj);
                }
                else
                {
                    _context.GoalStatuses.Update(obj);
                }

                updatedSuccess.Add(new GoalStatusDto() { Id = obj.Id });
            }

            var eventLog = await _eventLogService.Create("GoalStatusFeature", "GoalStatusFeature",
                                                "GoalStatus_AddOrUpdateCommand", request.userId);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<List<GoalStatusDto>>.Success(updatedSuccess);
        }
    }
}
