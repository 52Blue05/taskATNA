using Sale_Saas.Application.Common.Mappings;
using Sale_Saas.Application.Features.OrganizationFeature.Dto;

namespace Sale_Saas.Application.Features.OrganizationFeature.Queries
{
    public record Organization_GetListWithPaginationQuery(GetListWithPaginationQueryRequest RequestData) : IRequest<Result<PaginatedList<OrganizationDto>>>;

    public class Organization_GetListWithPaginationQueryHandler : IRequestHandler<Organization_GetListWithPaginationQuery, Result<PaginatedList<OrganizationDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        public Organization_GetListWithPaginationQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<PaginatedList<OrganizationDto>>> Handle(Organization_GetListWithPaginationQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Organizations.Where(m => m.DeleteFlag != true)
                                          .OrderBy(x => x.Id)
                                          .ProjectTo<OrganizationDto>(_mapper.ConfigurationProvider)
                                          .AsNoTracking();

            if (!string.IsNullOrEmpty(request.RequestData.TextSearch))
            {
                query = query.Where(s => s.Code.Contains(request.RequestData.TextSearch) ||
                                         s.Name.Contains(request.RequestData.TextSearch));
            }

            return Result<PaginatedList<OrganizationDto>>.Success(await query.PaginatedListAsync(request.RequestData.PageIndex, request.RequestData.PageSize));
        }
    }
}
