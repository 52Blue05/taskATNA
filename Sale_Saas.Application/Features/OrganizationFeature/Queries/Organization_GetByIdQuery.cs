using Sale_Saas.Application.Features.OrganizationFeature.Dto;

namespace Sale_Saas.Application.Features.OrganizationFeature.Queries
{
    public record Organization_GetByIdQuery(Guid Id) : IRequest<Result<OrganizationDto>>;
    public class Organization_GetByIdQueryHandler : IRequestHandler<Organization_GetByIdQuery, Result<OrganizationDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public Organization_GetByIdQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<OrganizationDto>> Handle(Organization_GetByIdQuery request, CancellationToken cancellationToken)
        {
            OrganizationDto? Organization = await (from sup in _context.Organizations
                                           where sup.DeleteFlag != true && sup.Id == request.Id
                                           select new OrganizationDto()
                                           {
                                               Id = sup.Id,
                                               Code = sup.Code ?? "",
                                               Name = sup.Name ?? ""
                                           }).AsNoTracking().FirstOrDefaultAsync();
            return Result<OrganizationDto>.Success(Organization);
        }
    }
}
