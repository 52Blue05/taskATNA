using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.BenefitFeature.Commands
{
    public record Benefit_ImportCommand(Guid userId, List<Benefit> RequestData) : IRequest<Result<List<Benefit>>>;
    public class Benefit_ImportCommandHandler : IRequestHandler<Benefit_ImportCommand, Result<List<Benefit>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;
        private readonly IEventLogService _eventLogService;

        public Benefit_ImportCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;
            _eventLogService = eventLogService;
        }
        public async Task<Result<List<Benefit>>> Handle(Benefit_ImportCommand request, CancellationToken cancellationToken)
        {
            var eventLog = await _eventLogService.Create("BenefitFeature", "BenefitFeature","Benefit_ImportCommand", request.userId);

            _context.Benefits.AddRange(request.RequestData);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<List<Benefit>>.Success(request.RequestData);
        }
    }
}
