using Sale_Saas.Application.Common.Mappings;
using Sale_Saas.Application.Features.RelationshipHistoryFeature.Dto;
using Sale_Saas.Application.Features.RelationshipLevelFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.RelationshipHistoryFeature.Queries
{
    public record RelationshipHistory_GetByIdQuery(Guid userId, Guid Id, GetListWithPaginationQueryRequest RequestData) : IRequest<Result<PaginatedList<RelationshipHistoryMobileDto>>>;
    public class RelationshipHistory_GetByIdQueryHandler : IRequestHandler<RelationshipHistory_GetByIdQuery, Result<PaginatedList<RelationshipHistoryMobileDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public RelationshipHistory_GetByIdQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<PaginatedList<RelationshipHistoryMobileDto>>> Handle(RelationshipHistory_GetByIdQuery request, CancellationToken cancellationToken)
        {
            var histories = await (from his in _context.RelationshipHistories
                                   join user in _context.ApplicationUsers on his.ApplicationUserId equals user.Id
                                   join pre in _context.RelationshipLevels on his.PreviousLevelId equals pre.Id
                                   join upd in _context.RelationshipLevels on his.UpdatedLevelId equals upd.Id
                                   where his.DeleteFlag != true && his.RelationshipId == request.Id
                                   orderby his.CreatedDate descending
                                   select new RelationshipHistoryMobileDto()
                                   {
                                       Id = his.Id,
                                       ApplicationUser = user.FullName ?? "",
                                       PreviousLevel = new RelationshipLevelMobileDto
                                       {
                                           Id = pre.Id,
                                           Code = pre.Code ?? "",
                                           Description = pre.Description ?? ""
                                       },
                                       UpdatedLevel = new RelationshipLevelMobileDto
                                       {
                                           Id = upd.Id,
                                           Code = upd.Code ?? "",
                                           Description = upd.Description ?? ""
                                       },
                                       CreatedDate = his.CreatedDate
                                   })
                                   .AsNoTracking()
                                   .PaginatedListAsync(request.RequestData.PageIndex, request.RequestData.PageSize);

            var eventLog = await _eventLogService.Create("RelationshipHistoryFeature", "RelationshipHistoryFeature", "RelationshipHistory_GetByIdQuery", request.userId);

            return Result<PaginatedList<RelationshipHistoryMobileDto>>.Success(histories);
        }
    }
}

