using Sale_Saas.Application.Features.OpportunityHistoryFeature.Dto;

namespace Sale_Saas.Application.Features.OpportunityHistoryFeature.Queries
{
    public record OpportunityHistory_GetByIdQuery(Guid Id) : IRequest<Result<IEnumerable<OpportunityHistoryDto>>>;
    public class OpportunityHistory_GetByIdQueryHandler : IRequestHandler<OpportunityHistory_GetByIdQuery, Result<IEnumerable<OpportunityHistoryDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public OpportunityHistory_GetByIdQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<OpportunityHistoryDto>>> Handle(OpportunityHistory_GetByIdQuery request, CancellationToken cancellationToken)
        {
            IEnumerable<OpportunityHistoryDto> list = await (from cv in _context.OpportunityHistories
                                                             join user in _context.ApplicationUsers on cv.ApplicationUserId equals user.Id
                                                             where cv.DeleteFlag != true && cv.Id == request.Id
                                                             orderby cv.CreatedDate descending
                                                             select new OpportunityHistoryDto()
                                                             {
                                                                 Id = cv.Id,
                                                                 Goal = cv.Goal ?? "",
                                                                 Activity = cv.Activity ?? "",
                                                                 Result = cv.Result ?? "",
                                                                 Time = cv.Time ?? new DateTime(),
                                                                 ApplicationUser = user.FullName ?? "",
                                                                 OpportunityId = cv.OpportunityId ?? Guid.Empty
                                                             }).AsNoTracking().ToListAsync();
            return Result<IEnumerable<OpportunityHistoryDto>>.Success(list);
        }
    }
}
    