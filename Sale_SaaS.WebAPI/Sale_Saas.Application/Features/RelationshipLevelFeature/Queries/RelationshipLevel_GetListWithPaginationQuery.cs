using Sale_Saas.Application.Common.Mappings;
using Sale_Saas.Application.Features.RelationshipLevelFeature.Dto;

namespace Sale_Saas.Application.Features.RelationshipLevelFeature.Queries
{
    public record RelationshipLevel_GetListWithPaginationQuery(GetListWithPaginationQueryRequest RequestData) : IRequest<Result<PaginatedList<RelationshipLevelDto>>>;

    public class RelationshipLevel_GetListWithPaginationQueryHandler : IRequestHandler<RelationshipLevel_GetListWithPaginationQuery, Result<PaginatedList<RelationshipLevelDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        public RelationshipLevel_GetListWithPaginationQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<PaginatedList<RelationshipLevelDto>>> Handle(RelationshipLevel_GetListWithPaginationQuery request, CancellationToken cancellationToken)
        {
            var query = _context.RelationshipLevels.Where(m => m.DeleteFlag != true)
                                                   .OrderBy(x => x.SortOrder)
                                                   .ProjectTo<RelationshipLevelDto>(_mapper.ConfigurationProvider)
                                                   .AsNoTracking();

            if (!string.IsNullOrEmpty(request.RequestData.TextSearch))
            {
                query = query.Where(s => s.Code.ToLower().Contains(request.RequestData.TextSearch.ToLower()) ||
                                         s.Description.ToLower().Contains(request.RequestData.TextSearch.ToLower()) ||
                                         s.Review.ToLower().Contains(request.RequestData.TextSearch.ToLower()));
            }

            return Result<PaginatedList<RelationshipLevelDto>>.Success(await query.PaginatedListAsync(request.RequestData.PageIndex, request.RequestData.PageSize));
        }
    }
}
