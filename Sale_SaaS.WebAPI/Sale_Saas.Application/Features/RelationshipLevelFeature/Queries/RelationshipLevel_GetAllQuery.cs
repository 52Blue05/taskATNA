using Sale_Saas.Application.Features.RelationshipLevelFeature.Dto;

namespace Sale_Saas.Application.Features.RelationshipLevelFeature.Queries
{
    public record RelationshipLevel_GetAllQuery(GetAllQueryRequest RequestData) : IRequest<Result<IEnumerable<RelationshipLevelDto>>>;
    public class RelationshipLevel_GetAllQueryHandler : IRequestHandler<RelationshipLevel_GetAllQuery, Result<IEnumerable<RelationshipLevelDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public RelationshipLevel_GetAllQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<RelationshipLevelDto>>> Handle(RelationshipLevel_GetAllQuery request, CancellationToken cancellationToken)
        {
            var customers = await _context.RelationshipLevels
                                  .Where(rl => rl.DeleteFlag != true)
                                  .OrderBy(rl => rl.SortOrder) // Order by SortOrder first
                                  .Select(rl => new RelationshipLevelDto // Then project into DTO
                                  {
                                      Id = rl.Id,
                                      Code = rl.Code ?? "",
                                      Description = rl.Description ?? "",
                                      Review = rl.Review ?? "",
                                      PointFrom = rl.PointFrom ?? 0,
                                      PointTo = rl.PointTo ?? 0,
                                      SortOrder = rl.SortOrder ?? 0
                                  })
                                  .AsNoTracking()
                                  .ToListAsync(cancellationToken);

            return Result<IEnumerable<RelationshipLevelDto>>.Success(customers);
        }
    }
}
