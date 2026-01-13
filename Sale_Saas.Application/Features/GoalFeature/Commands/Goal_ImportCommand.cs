using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.GoalFeature.Commands;

public record Goal_ImportCommand(Guid userId, List<Goal> RequestData) : IRequest<Result<List<Goal>>>;
public class Goal_ImportCommandHandler : IRequestHandler<Goal_ImportCommand, Result<List<Goal>>>
{
	private readonly IApplicationDbContext _context;
	private readonly IMapper _mapper;
	private readonly IInternalService _internalService;
    private readonly IEventLogService _eventLogService;

    public Goal_ImportCommandHandler(IMapper mapper,
											IApplicationDbContext context,
											IInternalService internalService, IEventLogService eventLogService)
	{
		_context = context;
		_mapper = mapper;
		_internalService = internalService;
		_eventLogService = eventLogService;
	}
	public async Task<Result<List<Goal>>> Handle(Goal_ImportCommand request, CancellationToken cancellationToken)
	{
        var eventLog = await _eventLogService.Create("GoalFeature", "GoalFeature",
                                                "Goal_ImportCommand", request.userId);

        _context.Goals.AddRange(request.RequestData);
		await _context.SaveChangesAsync(cancellationToken);
		return Result<List<Goal>>.Success(request.RequestData);
	}
}
