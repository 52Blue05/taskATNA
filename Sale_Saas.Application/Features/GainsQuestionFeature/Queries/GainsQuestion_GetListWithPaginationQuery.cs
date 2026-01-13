using Sale_Saas.Application.Common.Mappings;
using Sale_Saas.Application.Features.GainsQuestionFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.GainsQuestionFeature.Queries
{
    public record GainsQuestion_GetListWithPaginationQuery(Guid userId, GetListWithPaginationQueryRequest RequestData) : IRequest<Result<PaginatedList<GainsQuestionDto>>>;

    public class GainsQuestion_GetListWithPaginationQueryHandler : IRequestHandler<GainsQuestion_GetListWithPaginationQuery, Result<PaginatedList<GainsQuestionDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public GainsQuestion_GetListWithPaginationQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<PaginatedList<GainsQuestionDto>>> Handle(GainsQuestion_GetListWithPaginationQuery request, CancellationToken cancellationToken)
        {
            var query = _context.GainsQuestions.Where(m => m.DeleteFlag != true)
                                               .OrderBy(x => x.CreatedDate)
                                               .ProjectTo<GainsQuestionDto>(_mapper.ConfigurationProvider)
                                               .AsNoTracking();

            if (!string.IsNullOrEmpty(request.RequestData.TextSearch))
            {
                query = query.Where(s => s.Content.Contains(request.RequestData.TextSearch) ||
                                         s.Code.ToString().Contains(request.RequestData.TextSearch));
            }

            var eventLog = await _eventLogService.Create("GainsQuestionFeature", "GainsQuestionFeature",
                                                            "GainsQuestion_GetListWithPaginationQuery", request.userId);

            return Result<PaginatedList<GainsQuestionDto>>.Success(await query.PaginatedListAsync(request.RequestData.PageIndex, request.RequestData.PageSize));
        }
    }
}
