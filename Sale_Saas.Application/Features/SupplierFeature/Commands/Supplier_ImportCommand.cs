using Sale_Saas.Application.Interfaces.Services;

namespace Sale_Saas.Application.Features.SupplierFeature.Commands
{
    public record Supplier_ImportCommand(List<Supplier> RequestData) : IRequest<Result<List<Supplier>>>;
    public class Supplier_ImportCommandHandler : IRequestHandler<Supplier_ImportCommand, Result<List<Supplier>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;

        public Supplier_ImportCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;

        }
        public async Task<Result<List<Supplier>>> Handle(Supplier_ImportCommand request, CancellationToken cancellationToken)
        {
            _context.Suppliers.AddRange(request.RequestData);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<List<Supplier>>.Success(request.RequestData);
        }
    }
}
