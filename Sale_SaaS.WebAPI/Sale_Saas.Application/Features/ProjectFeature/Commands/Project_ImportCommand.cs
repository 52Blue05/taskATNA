using Sale_Saas.Application.Interfaces.Services;

namespace Sale_Saas.Application.Features.ProjectFeature.Commands
{
    public record Project_ImportCommand(List<Project> RequestData) : IRequest<Result<List<Project>>>;
    public class Project_ImportCommandHandler : IRequestHandler<Project_ImportCommand, Result<List<Project>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;

        public Project_ImportCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;

        }
        public async Task<Result<List<Project>>> Handle(Project_ImportCommand request, CancellationToken cancellationToken)
        {
            _context.Projects.AddRange(request.RequestData);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<List<Project>>.Success(request.RequestData);
        }
    }
}
