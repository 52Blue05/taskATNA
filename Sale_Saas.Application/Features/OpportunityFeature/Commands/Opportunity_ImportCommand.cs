using Sale_Saas.Application.Interfaces.Services;

namespace Sale_Saas.Application.Features.OpportunityFeature.Commands
{
    public record Opportunity_ImportCommand(List<Opportunity> RequestData) : IRequest<Result<List<Opportunity>>>;
    public class Opportunity_ImportCommandHandler : IRequestHandler<Opportunity_ImportCommand, Result<List<Opportunity>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;

        public Opportunity_ImportCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;

        }
        public async Task<Result<List<Opportunity>>> Handle(Opportunity_ImportCommand request, CancellationToken cancellationToken)
        {
            _context.Opportunities.AddRange(request.RequestData);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<List<Opportunity>>.Success(request.RequestData);
        }
    }
}
