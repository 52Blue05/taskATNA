using Sale_Saas.Application.Features.ProjectStatusFeature.Dto;

namespace Sale_Saas.Application.Features.ProjectStatusFeature.Queries
{
    public record ProjectStatus_GetAllQuery(GetAllQueryRequest RequestData) : IRequest<Result<IEnumerable<ProjectStatusDto>>>;
    public class ProjectStatus_GetAllQueryHandler : IRequestHandler<ProjectStatus_GetAllQuery, Result<IEnumerable<ProjectStatusDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ProjectStatus_GetAllQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<ProjectStatusDto>>> Handle(ProjectStatus_GetAllQuery request, CancellationToken cancellationToken)
        {
            IEnumerable<ProjectStatusDto> list = (await (from sup in _context.ProjectStatuses
                                                         where sup.DeleteFlag != true
                                                         select new ProjectStatusDto()
                                                         {
                                                             Id = sup.Id,
                                                             Code = sup.Code ?? "",
                                                             Name = sup.Name ?? ""
                                                         }).AsNoTracking().ToListAsync()).AsReadOnly();

            return Result<IEnumerable<ProjectStatusDto>>.Success(list);
        }
    }
}
