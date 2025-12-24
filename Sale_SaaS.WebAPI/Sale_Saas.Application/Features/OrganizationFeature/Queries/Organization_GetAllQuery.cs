using Sale_Saas.Application.Features.OrganizationFeature.Dto;

namespace Sale_Saas.Application.Features.OrganizationFeature.Queries
{
    public record Organization_GetAllQuery(GetAllQueryRequest RequestData) : IRequest<Result<IEnumerable<OrganizationDto>>>;
    public class Organization_GetAllQueryHandler : IRequestHandler<Organization_GetAllQuery, Result<IEnumerable<OrganizationDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public Organization_GetAllQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<OrganizationDto>>> Handle(Organization_GetAllQuery request, CancellationToken cancellationToken)
        {
            IEnumerable<OrganizationDto> customers = (await (from sup in _context.Organizations
                                                         where sup.DeleteFlag != true
                                                         select new OrganizationDto()
                                                         {
                                                             Id = sup.Id,
                                                             Code = sup.Code ?? "",
                                                             Name = sup.Name ?? "",                                                            
                                                         }).AsNoTracking().ToListAsync()).AsReadOnly();

            return Result<IEnumerable<OrganizationDto>>.Success(customers);
        }
    }
}
