using Sale_Saas.Application.Features.RelationshipLevelFeature.Dto;

namespace Sale_Saas.Application.Features.RelationshipLevelFeature.Queries
{
    public record RelationshipLevel_GetByIdQuery(Guid Id) : IRequest<Result<RelationshipLevelDto>>;
    public class RelationshipLevel_GetByIdQueryHandler : IRequestHandler<RelationshipLevel_GetByIdQuery, Result<RelationshipLevelDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public RelationshipLevel_GetByIdQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<RelationshipLevelDto>> Handle(RelationshipLevel_GetByIdQuery request, CancellationToken cancellationToken)
        {
            RelationshipLevelDto? RelationshipLevel = await (from rl in _context.RelationshipLevels
                                                             where rl.DeleteFlag != true && rl.Id == request.Id
                                                             select new RelationshipLevelDto()
                                                             {
                                                                 Id = rl.Id,
                                                                 Code = rl.Code ?? "",
                                                                 Description = rl.Description ?? "",
                                                                 Review = rl.Review ?? "",
                                                                 PointFrom = rl.PointFrom ?? 0,
                                                                 PointTo = rl.PointTo ?? 0
                                                             }).AsNoTracking().FirstOrDefaultAsync();
            return Result<RelationshipLevelDto>.Success(RelationshipLevel);
        }
    }
}
