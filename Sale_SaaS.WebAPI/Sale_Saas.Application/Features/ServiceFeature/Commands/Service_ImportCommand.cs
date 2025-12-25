using Sale_Saas.Application.Interfaces.Services;

namespace Sale_Saas.Application.Features.ServiceFeature.Commands
{
    public record Service_ImportCommand(List<Service> RequestData) : IRequest<Result<List<Service>>>;
    public class Service_ImportCommandHandler : IRequestHandler<Service_ImportCommand, Result<List<Service>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;

        public Service_ImportCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;

        }
        public async Task<Result<List<Service>>> Handle(Service_ImportCommand request, CancellationToken cancellationToken)
        {
            _context.Services.AddRange(request.RequestData);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<List<Service>>.Success(request.RequestData);
        }
    }
}
