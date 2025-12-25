using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.GainsQuestionFeature.Commands
{
    public record GainsQuestion_ImportCommand(Guid userId, List<GainsQuestion> RequestData) : IRequest<Result<List<GainsQuestion>>>;
    public class GainsQuestion_ImportCommandHandler : IRequestHandler<GainsQuestion_ImportCommand, Result<List<GainsQuestion>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;
        private readonly IEventLogService _eventLogService;

        public GainsQuestion_ImportCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;
            _eventLogService = eventLogService;
        }
        public async Task<Result<List<GainsQuestion>>> Handle(GainsQuestion_ImportCommand request, CancellationToken cancellationToken)
        {
            var eventLog = await _eventLogService.Create("GainsQuestionFeature", "GainsQuestionFeature",
                        "GainsQuestion_ImportCommand", request.userId);

            _context.GainsQuestions.AddRange(request.RequestData);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<List<GainsQuestion>>.Success(request.RequestData);
        }
    }
}
