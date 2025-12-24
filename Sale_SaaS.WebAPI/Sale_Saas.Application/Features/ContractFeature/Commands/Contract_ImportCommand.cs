using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.ContractFeature.Commands
{
    public record Contract_ImportCommand(Guid userId, List<Contract> RequestData) : IRequest<Result<List<Contract>>>;
    public class Contract_ImportCommandHandler : IRequestHandler<Contract_ImportCommand, Result<List<Contract>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;
        private readonly IEventLogService _eventLogService;

        public Contract_ImportCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService,
                                                IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;
            _eventLogService = eventLogService;
        }
        public async Task<Result<List<Contract>>> Handle(Contract_ImportCommand request, CancellationToken cancellationToken)
        {
            var eventLog = await _eventLogService.Create("ContractFeature", "ContractFeature",
                                                   "Contract_ImportCommand", request.userId);

            _context.Contracts.AddRange(request.RequestData);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<List<Contract>>.Success(request.RequestData);
        }
    }
}
