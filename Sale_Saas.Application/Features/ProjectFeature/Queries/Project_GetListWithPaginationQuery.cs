using Sale_Saas.Application.Common.Mappings;
using Sale_Saas.Application.Features.ProjectFeature.Dto;

namespace Sale_Saas.Application.Features.ProjectFeature.Queries
{
    public record Project_GetListWithPaginationQuery(GetListWithPaginationQueryRequest RequestData) : IRequest<Result<PaginatedList<ProjectDto>>>;

    public class Project_GetListWithPaginationQueryHandler : IRequestHandler<Project_GetListWithPaginationQuery, Result<PaginatedList<ProjectDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        public Project_GetListWithPaginationQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }
        
        public async Task<Result<PaginatedList<ProjectDto>>> Handle(Project_GetListWithPaginationQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Projects.Where(m => m.DeleteFlag != true)
                                         .OrderBy(x => x.CreatedDate)
                                         .ProjectTo<ProjectDto>(_mapper.ConfigurationProvider)
                                         .AsNoTracking();

            if (!string.IsNullOrEmpty(request.RequestData.TextSearch))
            {
                query = query.Where(s => s.Code.ToLower().Contains(request.RequestData.TextSearch.ToLower()) ||
                                         s.Name.ToLower().Contains(request.RequestData.TextSearch.ToLower()));
            }

            var listResult = await query.PaginatedListAsync(request.RequestData.PageIndex, request.RequestData.PageSize);

            foreach (var item in listResult.Items)
            {
                var service = await _context.Services.Where(s => s.Id == Guid.Parse(item.Service) && s.DeleteFlag != true)
                                                         .AsNoTracking().FirstOrDefaultAsync();

                if (service != null)
                {
                    item.Service = service.Name ?? "";
                }
            }

            return Result<PaginatedList<ProjectDto>>.Success(listResult);
        }
    }
}
