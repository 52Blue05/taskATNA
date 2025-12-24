using Sale_Saas.Application.Features.ProjectFeature.Dto;
using Sale_Saas.Application.Features.ProjectStatusFeature.Dto;
using Sale_Saas.Application.Features.ServiceFeature.Dto;
using Sale_Saas.Domain.Entities;

namespace Sale_Saas.Application.Features.ProjectFeature.Queries
{
    public record Project_GetByIdQuery(Guid Id) : IRequest<Result<ProjectDto>>;
    public class Project_GetByIdQueryHandler : IRequestHandler<Project_GetByIdQuery, Result<ProjectDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public Project_GetByIdQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<ProjectDto>> Handle(Project_GetByIdQuery request, CancellationToken cancellationToken)
        {
            ProjectDto? Project = await (from prj in _context.Projects
                                         where prj.DeleteFlag != true && prj.Id == request.Id
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
                                             Service = _context.Services.Where(s => s.Id == Guid.Parse(prj.Service) && s.DeleteFlag != true)
                                                         .Select(s => s.Name).AsNoTracking().FirstOrDefault(),
                                             ApplicationUser = user.FullName ?? "",
                                             ProjectStatus = status != null ? new ProjectStatusDto()
                                             {
                                                 Id = status.Id,
                                                 Code = status.Code ?? "",
                                                 Name = status.Name ?? ""
                                             } : new ProjectStatusDto()
                                         }).AsNoTracking().FirstOrDefaultAsync();
            return Result<ProjectDto>.Success(Project);
        }
    }
}
