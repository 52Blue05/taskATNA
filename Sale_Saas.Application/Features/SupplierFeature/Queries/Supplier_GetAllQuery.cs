using Sale_Saas.Application.Features.SupplierFeature.Dto;

namespace Sale_Saas.Application.Features.SupplierFeature.Queries
{
    public record Supplier_GetAllQuery(GetAllQueryRequest RequestData) : IRequest<Result<IEnumerable<SupplierDto>>>;
    public class Supplier_GetAllQueryHandler : IRequestHandler<Supplier_GetAllQuery, Result<IEnumerable<SupplierDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public Supplier_GetAllQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<SupplierDto>>> Handle(Supplier_GetAllQuery request, CancellationToken cancellationToken)
        {
            IEnumerable<SupplierDto> customers = (await (from sup in _context.Suppliers
                                                         where sup.DeleteFlag != true
                                                         select new SupplierDto()
                                                         {
                                                             Id = sup.Id,
                                                             Code = sup.Code ?? "",
                                                             Name = sup.Name ?? "",
                                                             Review = sup.Review ?? "",
                                                             Description = sup.Description ?? ""
                                                         }).AsNoTracking().ToListAsync()).AsReadOnly();

            return Result<IEnumerable<SupplierDto>>.Success(customers);
        }
    }
}
