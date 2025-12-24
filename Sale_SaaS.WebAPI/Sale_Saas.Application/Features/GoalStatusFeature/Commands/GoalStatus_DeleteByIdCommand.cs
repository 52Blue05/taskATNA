using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.GoalStatusFeature.Commands
{
    public record GoalStatus_DeleteByIdCommand(Guid userId, DeleteRequest RequestData) : IRequest<Result<string>>;
    public class GoalStatus_DeleteByIdCommandHandler : IRequestHandler<GoalStatus_DeleteByIdCommand, Result<string>>
    {

        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public GoalStatus_DeleteByIdCommandHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<string>> Handle(GoalStatus_DeleteByIdCommand request, CancellationToken cancellationToken)
        {
            string result = string.Empty;

            if (request.RequestData.Ids == null) throw new ApplicationException("Không tìm thấy tham số Id.");
            List<Guid> ids = request.RequestData.Ids.Select(m => Guid.Parse(m)).ToList();
            var query = await _context.GoalStatuses.Where(m => ids.Contains(m.Id)).ToListAsync();
            if (query == null || query.Count == 0) throw new ApplicationException($"Không tìm thấy trong dữ liệu có Id: {string.Join(";", request.RequestData.Ids)}");

            foreach (var item in query)
            {
                item.DeleteFlag = true;
                item.LastModifiedDate = DateTime.Now;
                item.LastModifiedApplicationUserId = request.RequestData.ApplicationUserId;
            }

            _context.GoalStatuses.UpdateRange(query);

            await _context.SaveChangesAsync(cancellationToken);

            var eventLog = await _eventLogService.Create("GoalStatusFeature", "GoalStatusFeature",
                                    "GoalStatus_DeleteByIdCommand", request.userId);

            return Result<string>.Success(string.Empty);
        }
    }
}
