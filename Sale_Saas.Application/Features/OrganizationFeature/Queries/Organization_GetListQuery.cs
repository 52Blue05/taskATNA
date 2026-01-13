using Sale_Saas.Application.Features.OrganizationFeature.Dto;

namespace Sale_Saas.Application.Features.OrganizationFeature.Queries
{
    public record Organization_GetListQuery(FilterQueryRequest RequestData) : IRequest<Result<IEnumerable<OrganizationDto>>>;

    public class Organization_GetListQueryHandler : IRequestHandler<Organization_GetListQuery, Result<IEnumerable<OrganizationDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public Organization_GetListQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<OrganizationDto>>> Handle(Organization_GetListQuery request, CancellationToken cancellationToken)
        {
            var query = from sup in _context.Organizations
                        where sup.DeleteFlag != true
                        select new OrganizationDto()
                        {
                            Id = sup.Id,
                            Code = sup.Code ?? "",
                            Name = sup.Name ?? ""
                        };

            if (!string.IsNullOrEmpty(request.RequestData.TextSearch))
            {
                query = query.Where(x => x.Name.Contains(request.RequestData.TextSearch));
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

            return Result<IEnumerable<OrganizationDto>>.Success(data);
        }
    }
}
