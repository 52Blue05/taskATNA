using Sale_Saas.Application.Features.RelationshipLevelFeature.Dto;

namespace Sale_Saas.Application.Features.RelationshipLevelFeature.Queries
{
    public record RelationshipLevel_GetListQuery(FilterQueryRequest RequestData) : IRequest<Result<IEnumerable<RelationshipLevelDto>>>;

    public class RelationshipLevel_GetListQueryHandler : IRequestHandler<RelationshipLevel_GetListQuery, Result<IEnumerable<RelationshipLevelDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public RelationshipLevel_GetListQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<RelationshipLevelDto>>> Handle(RelationshipLevel_GetListQuery request, CancellationToken cancellationToken)
        {
            var query = from rl in _context.RelationshipLevels
                        where rl.DeleteFlag != true
                        select new RelationshipLevelDto()
                        {
                            Id = rl.Id,
                            Code = rl.Code ?? "",
                            Description = rl.Description ?? "",
                            Review = rl.Review ?? "",
                            PointFrom = rl.PointFrom ?? 0,
                            PointTo = rl.PointTo ?? 0
                        };

            if (!string.IsNullOrEmpty(request.RequestData.TextSearch))
            {
                query = query.Where(x => x.Review.Contains(request.RequestData.TextSearch) ||
                                         x.Description.Contains(request.RequestData.TextSearch));
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

            return Result<IEnumerable<RelationshipLevelDto>>.Success(data);
        }
    }
}
