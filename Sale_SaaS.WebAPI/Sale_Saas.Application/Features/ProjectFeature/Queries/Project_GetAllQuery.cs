using Sale_Saas.Application.Features.ProjectFeature.Dto;
using Sale_Saas.Application.Features.ProjectStatusFeature.Dto;
namespace Sale_Saas.Application.Features.ProjectFeature.Queries
{
    public record Project_GetAllQuery(GetAllQueryRequest RequestData) : IRequest<Result<IEnumerable<ProjectDto>>>;
    public class Project_GetAllQueryHandler : IRequestHandler<Project_GetAllQuery, Result<IEnumerable<ProjectDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public Project_GetAllQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<ProjectDto>>> Handle(Project_GetAllQuery request, CancellationToken cancellationToken)
        {
            IEnumerable<ProjectDto> projects = (await (from prj in _context.Projects
                                                       where prj.DeleteFlag != true
                                                       join status in _context.ProjectStatuses on prj.ProjectStatusId equals status.Id into statusGroup
                                                       from status in statusGroup.DefaultIfEmpty()
                                                       join user in _context.ApplicationUsers on prj.ApplicationUserId equals user.Id
                                                       select new ProjectDto()
                                                       {
                                                           Id = prj.Id,
                                                           Code = prj.Code ?? "",
                                                           Name = prj.Name ?? "",
                                                           Result = prj.Result ?? "",
                                                           Type = prj.Type ?? "",
                                                           Point = prj.Point ?? 0,
                                                           Note = prj.Note ?? "",
                                                           Service = prj.Service ?? "",
                                                           ApplicationUser = user.FullName ?? "",
                                                           ProjectStatus = status != null ? new ProjectStatusDto()
                                                           {
                                                               Id = status.Id,
                                                               Code = status.Code ?? "",
                                                               Name = status.Name ?? ""
                                                           } : new ProjectStatusDto()
                                                       }).AsNoTracking().ToListAsync()).AsReadOnly();

            return Result<IEnumerable<ProjectDto>>.Success(projects);
        }
    }
}
