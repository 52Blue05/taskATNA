using Sale_Saas.Application.Features.ProjectFeature.Dto;
using Sale_Saas.Application.Features.ProjectStatusFeature.Dto;
using Sale_Saas.Application.Features.ServiceFeature.Dto;

namespace Sale_Saas.Application.Features.ProjectFeature.Queries
{
    public record Project_GetListQuery(FilterQueryRequest RequestData) : IRequest<Result<IEnumerable<ProjectDto>>>;

    public class Project_GetListQueryHandler : IRequestHandler<Project_GetListQuery, Result<IEnumerable<ProjectDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public Project_GetListQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<ProjectDto>>> Handle(Project_GetListQuery request, CancellationToken cancellationToken)
        {
            var query = from prj in _context.Projects
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
                        };

            if (!string.IsNullOrEmpty(request.RequestData.TextSearch))
            {
                query = query.Where(x => x.Code.Contains(request.RequestData.TextSearch) ||
                                         x.Name.Contains(request.RequestData.TextSearch) ||
                                         x.Result.Contains(request.RequestData.TextSearch) ||
                                         x.Type.Contains(request.RequestData.TextSearch) );
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

            return Result<IEnumerable<ProjectDto>>.Success(data);
        }
    }
}
