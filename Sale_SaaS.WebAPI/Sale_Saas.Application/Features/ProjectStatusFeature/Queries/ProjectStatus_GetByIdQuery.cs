using Sale_Saas.Application.Features.ProjectStatusFeature.Dto;
namespace Sale_Saas.Application.Features.ProjectStatusFeature.Queries
{
    public record ProjectStatus_GetByIdQuery(Guid Id) : IRequest<Result<ProjectStatusDto>>;
    public class ProjectStatus_GetByIdQueryHandler : IRequestHandler<ProjectStatus_GetByIdQuery, Result<ProjectStatusDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ProjectStatus_GetByIdQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<ProjectStatusDto>> Handle(ProjectStatus_GetByIdQuery request, CancellationToken cancellationToken)
        {
            ProjectStatusDto? ProjectStatus = await (from sup in _context.ProjectStatuses
                                           where sup.DeleteFlag != true && sup.Id == request.Id
                                           select new ProjectStatusDto()
                                           {
                                               Id = sup.Id,
                                               Code = sup.Code ?? "",
                                               Name = sup.Name ?? ""
                                           }).AsNoTracking().FirstOrDefaultAsync();
            return Result<ProjectStatusDto>.Success(ProjectStatus);
        }
    }
}
