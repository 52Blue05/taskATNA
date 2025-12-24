using Sale_Saas.Application.Features.SupplierFeature.Dto;

namespace Sale_Saas.Application.Features.SupplierFeature.Queries
{
    public record Supplier_GetByIdQuery(Guid Id) : IRequest<Result<SupplierDto>>;
    public class Supplier_GetByIdQueryHandler : IRequestHandler<Supplier_GetByIdQuery, Result<SupplierDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public Supplier_GetByIdQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<SupplierDto>> Handle(Supplier_GetByIdQuery request, CancellationToken cancellationToken)
        {
            SupplierDto? Supplier = await (from sup in _context.Suppliers
                                           where sup.DeleteFlag != true && sup.Id == request.Id
                                           select new SupplierDto()
                                           {
                                               Id = sup.Id,
                                               Code = sup.Code ?? "",
                                               Name = sup.Name ?? "",
                                               Review = sup.Review ?? "",
                                               Description = sup.Description ?? ""
                                           }).AsNoTracking().FirstOrDefaultAsync();
            return Result<SupplierDto>.Success(Supplier);
        }
    }
}
