using Sale_Saas.Application.Features.GainsQuestionFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.GainsQuestionFeature.Queries
{
    public record GainsQuestion_GetListQuery(Guid userId, FilterQueryRequest RequestData) : IRequest<Result<IEnumerable<GainsQuestionDto>>>;

    public class GainsQuestion_GetListQueryHandler : IRequestHandler<GainsQuestion_GetListQuery, Result<IEnumerable<GainsQuestionDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public GainsQuestion_GetListQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<IEnumerable<GainsQuestionDto>>> Handle(GainsQuestion_GetListQuery request, CancellationToken cancellationToken)
        {
            var query = from gq in _context.GainsQuestions
                        where gq.DeleteFlag != true
                        //orderby gq.Code ascending
                        select new GainsQuestionDto()
                        {
                            Id = gq.Id,
                           // Code = gq.Code ?? 0,
                            Description = gq.Description ?? "",
                            Content = gq.Content ?? ""
                        };

            if (!string.IsNullOrEmpty(request.RequestData.TextSearch))
            {
                query = query.Where(x => x.Content.Contains(request.RequestData.TextSearch) ||
                                         x.Description.Contains(request.RequestData.TextSearch));
            }

            if (!string.IsNullOrEmpty(request.RequestData.Code))
            {
                query = query.Where(x => x.Code.ToString().Contains(request.RequestData.Code));
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

            var eventLog = await _eventLogService.Create("GainsQuestionFeature", "GainsQuestionFeature",
                                                                "GainsQuestion_GetListQuery", request.userId);

            return Result<IEnumerable<GainsQuestionDto>>.Success(data);
        }
    }
}
