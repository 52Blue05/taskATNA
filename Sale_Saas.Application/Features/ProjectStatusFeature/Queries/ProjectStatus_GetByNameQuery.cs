using Sale_Saas.Application.Features.ProjectStatusFeature.Dto;

namespace Sale_Saas.Application.Features.ProjectStatusFeature.Queries
{
    public record ProjectStatus_GetByNameQuery(string Name) : IRequest<Result<ProjectStatusDto>>;
    public class ProjectStatus_GetByNameQueryHandler : IRequestHandler<ProjectStatus_GetByNameQuery, Result<ProjectStatusDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ProjectStatus_GetByNameQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<ProjectStatusDto>> Handle(ProjectStatus_GetByNameQuery request, CancellationToken cancellationToken)
        {
            ProjectStatusDto? ProjectStatus = await (from pro in _context.ProjectStatuses
                                                       where pro.DeleteFlag != true && pro.Name == request.Name
                                                       select new ProjectStatusDto()
                                                       {
                                                           Id = pro.Id,
                                                           Code = pro.Code ?? "",
                                                           Name = pro.Name ?? ""
                                                       }).AsNoTracking().FirstOrDefaultAsync();
            return Result<ProjectStatusDto>.Success(ProjectStatus);
        }
    }
}
