using Sale_Saas.Application.Common.Mappings;
using Sale_Saas.Application.Features.RelationshipHistoryFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.RelationshipHistoryFeature.Queries;

public record RelationshipHistory_GetListWithPaginationQuery(Guid userId, GetListWithPaginationQueryRequest RequestData) : IRequest<Result<PaginatedList<RelationshipHistoryDto>>>;

public class RelationshipHistory_GetListWithPaginationQueryHandler : IRequestHandler<RelationshipHistory_GetListWithPaginationQuery, Result<PaginatedList<RelationshipHistoryDto>>>
{
     private readonly IApplicationDbContext _context;
     private readonly IMapper _mapper;
     private readonly IEventLogService _eventLogService;

     public RelationshipHistory_GetListWithPaginationQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
     {
          _context = context;
          _mapper = mapper;
          _eventLogService = eventLogService;
     }

     public async Task<Result<PaginatedList<RelationshipHistoryDto>>> Handle(RelationshipHistory_GetListWithPaginationQuery request, CancellationToken cancellationToken)
     {
          var query = _context.RelationshipHistories.Include(s => s.ApplicationUser).Include(s => s.PreviousLevel).Include(s => s.UpdatedLevel)
                                   .Where(m => m.DeleteFlag != true)
                                   .OrderByDescending(x => x.CreatedDate)
                                   .ProjectTo<RelationshipHistoryDto>(_mapper.ConfigurationProvider)
                                   .AsNoTracking();

          if (!string.IsNullOrEmpty(request.RequestData.TextSearch))
          {
               query = query.Where(s => s.ApplicationUser.ToLower().Contains(request.RequestData.TextSearch.ToLower()));
          }

          var eventLog = await _eventLogService.Create("RelationshipHistoryFeature", "RelationshipHistoryFeature", "RelationshipHistory_GetListWithPaginationQuery", request.userId);

          return Result<PaginatedList<RelationshipHistoryDto>>.Success(await query.PaginatedListAsync(request.RequestData.PageIndex, request.RequestData.PageSize));
     }
}

