using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.CustomerFeature.Commands
{
    public record Customer_ImportCommand(Guid userId, List<Customer> RequestData) : IRequest<Result<List<Customer>>>;
    public class Customer_ImportCommandHandler : IRequestHandler<Customer_ImportCommand, Result<List<Customer>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;
        private readonly IEventLogService _eventLogService;

        public Customer_ImportCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;
            _eventLogService = eventLogService;
        }
        public async Task<Result<List<Customer>>> Handle(Customer_ImportCommand request, CancellationToken cancellationToken)
        {
            var eventLog = await _eventLogService.Create("CustomerFeature", "CustomerFeature",
                           "Customer_ImportCommand", request.userId);

            _context.Customers.AddRange(request.RequestData);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<List<Customer>>.Success(request.RequestData);
        }
    }
}
