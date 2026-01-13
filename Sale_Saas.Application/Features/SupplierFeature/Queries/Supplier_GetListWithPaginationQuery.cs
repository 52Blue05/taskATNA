using Sale_Saas.Application.Common.Mappings;
using Sale_Saas.Application.Features.SupplierFeature.Dto;

namespace Sale_Saas.Application.Features.SupplierFeature.Queries
{
    public record Supplier_GetListWithPaginationQuery(GetListWithPaginationQueryRequest RequestData) : IRequest<Result<PaginatedList<SupplierDto>>>;

    public class Supplier_GetListWithPaginationQueryHandler : IRequestHandler<Supplier_GetListWithPaginationQuery, Result<PaginatedList<SupplierDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        public Supplier_GetListWithPaginationQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<PaginatedList<SupplierDto>>> Handle(Supplier_GetListWithPaginationQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Suppliers.Where(m => m.DeleteFlag != true)
                                          .OrderBy(x => x.CreatedDate)
                                          .ProjectTo<SupplierDto>(_mapper.ConfigurationProvider)
                                          .AsNoTracking();

            if (!string.IsNullOrEmpty(request.RequestData.TextSearch))
            {
                query = query.Where(s => s.Code.Contains(request.RequestData.TextSearch) ||
                                         s.Name.Contains(request.RequestData.TextSearch));
            }

            return Result<PaginatedList<SupplierDto>>.Success(await query.PaginatedListAsync(request.RequestData.PageIndex, request.RequestData.PageSize));
        }
    }
}
