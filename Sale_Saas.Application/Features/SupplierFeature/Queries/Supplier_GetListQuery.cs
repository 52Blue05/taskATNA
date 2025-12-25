using Sale_Saas.Application.Features.SupplierFeature.Dto;

namespace Sale_Saas.Application.Features.SupplierFeature.Queries
{
    public record Supplier_GetListQuery(FilterQueryRequest RequestData) : IRequest<Result<IEnumerable<SupplierDto>>>;

    public class Supplier_GetListQueryHandler : IRequestHandler<Supplier_GetListQuery, Result<IEnumerable<SupplierDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public Supplier_GetListQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<SupplierDto>>> Handle(Supplier_GetListQuery request, CancellationToken cancellationToken)
        {
            var query = from sup in _context.Suppliers
                        where sup.DeleteFlag != true
                        select new SupplierDto()
                        {
                            Id = sup.Id,
                            Code = sup.Code ?? "",
                            Name = sup.Name ?? "",
                            Review = sup.Review ?? "",
                            Description = sup.Description ?? ""
                        };

            if (!string.IsNullOrEmpty(request.RequestData.TextSearch))
            {
                query = query.Where(x => x.Review.Contains(request.RequestData.TextSearch) ||
                                         x.Description.Contains(request.RequestData.TextSearch));
            }

            if (!string.IsNullOrEmpty(request.RequestData.Code))
            {
                query = query.Where(x => x.Code.Contains(request.RequestData.Code));
            }

            if (request.RequestData.Skip != null)
            {
                query = query.Skip(request.RequestData.Skip.Value);
            }

            if (request.RequestData.TotalRecord != null)
            {
                query = query.Take(request.RequestData.TotalRecord.Value);
            }

            var data = await query.AsNoTracking().ToListAsync();

            return Result<IEnumerable<SupplierDto>>.Success(data);
        }
    }
}
