using Sale_Saas.Application.Interfaces.Services;

namespace Sale_Saas.Application.Features.RelationshipLevelFeature.Commands
{
    public record RelationshipLevel_ImportCommand(List<RelationshipLevel> RequestData) : IRequest<Result<List<RelationshipLevel>>>;
    public class RelationshipLevel_ImportCommandHandler : IRequestHandler<RelationshipLevel_ImportCommand, Result<List<RelationshipLevel>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;

        public RelationshipLevel_ImportCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;

        }
        public async Task<Result<List<RelationshipLevel>>> Handle(RelationshipLevel_ImportCommand request, CancellationToken cancellationToken)
        {
            _context.RelationshipLevels.AddRange(request.RequestData);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<List<RelationshipLevel>>.Success(request.RequestData);
        }
    }
}
