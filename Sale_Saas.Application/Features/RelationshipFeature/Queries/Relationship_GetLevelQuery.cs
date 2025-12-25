using Sale_Saas.Application.Features.RelationshipFeature.Services;
using Sale_Saas.Application.Features.RelationshipLevelFeature.Dto;

namespace Sale_Saas.Application.Features.RelationshipFeature.Queries
{
    public record Relationship_GetLevelQuery(Guid Id) : IRequest<Result<List<RelationshipLevelDto>>>;
    public class Relationship_GetLevelQueryHandler : IRequestHandler<Relationship_GetLevelQuery, Result<List<RelationshipLevelDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public Relationship_GetLevelQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<RelationshipLevelDto>>> Handle(Relationship_GetLevelQuery request, CancellationToken cancellationToken)
        {
            var relationship = await RelationshipService.GetRelationship(request.Id, _context);

            if (relationship == null)
            {
                return Result<List<RelationshipLevelDto>>.Success(new List<RelationshipLevelDto>());
            }

            // get by origin and end level
            var sortOrderOriginalLevel = relationship.CurrentRelationship.SortOrder;

            var sortOrderTargetLevel = relationship.TargetRelationship.SortOrder;


            var level = await _context.RelationshipLevels
                                .Where(s => s.DeleteFlag != true && s.SortOrder >= sortOrderOriginalLevel && s.SortOrder <= sortOrderTargetLevel)
                                .Select(s => new RelationshipLevelDto
                                {
                                    Id = s.Id,
                                    Code = s.Code ?? "",
                                    Description = s.Description ?? "",
                                    PointTo = s.PointTo ?? 0,
                                    PointFrom = s.PointFrom ?? 0,
                                    SortOrder = s.SortOrder ?? 0
                                })
                                .OrderBy(s => s.SortOrder)
                                .ToListAsync();

            return Result<List<RelationshipLevelDto>>.Success(level);
        }
    }
}
