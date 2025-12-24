using Sale_Saas.Application.Common.Mappings;
using Sale_Saas.Application.Features.ServiceFeature.Dto;

namespace Sale_Saas.Application.Features.ServiceFeature.Queries
{
    public record Service_GetListWithPaginationQuery(GetListWithPaginationQueryRequest RequestData) : IRequest<Result<PaginatedList<ServiceDto>>>;

    public class Service_GetListWithPaginationQueryHandler : IRequestHandler<Service_GetListWithPaginationQuery, Result<PaginatedList<ServiceDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        public Service_GetListWithPaginationQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<PaginatedList<ServiceDto>>> Handle(Service_GetListWithPaginationQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Services.Where(m => m.DeleteFlag != true)
                                         .OrderBy(x => x.CreatedDate)
                                         .ProjectTo<ServiceDto>(_mapper.ConfigurationProvider)
                                         .AsNoTracking();

            if (!string.IsNullOrEmpty(request.RequestData.TextSearch))
            {
                query = query.Where(s => s.Code.ToLower().Contains(request.RequestData.TextSearch.ToLower()) ||
                                         s.Name.ToLower().Contains(request.RequestData.TextSearch.ToLower()) ||
                                         s.ShortName.ToLower().Contains(request.RequestData.TextSearch.ToLower()));
            }

            return Result<PaginatedList<ServiceDto>>.Success(await query.PaginatedListAsync(request.RequestData.PageIndex, request.RequestData.PageSize));
        }
    }
}
