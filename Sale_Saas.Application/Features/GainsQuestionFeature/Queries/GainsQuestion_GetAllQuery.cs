using Sale_Saas.Application.Features.GainsQuestionFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.GainsQuestionFeature.Queries
{
    public record GainsQuestion_GetAllQuery(Guid userId, GetAllQueryRequest RequestData) : IRequest<Result<IEnumerable<GainsQuestionDto>>>;
    public class GainsQuestion_GetAllQueryHandler : IRequestHandler<GainsQuestion_GetAllQuery, Result<IEnumerable<GainsQuestionDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public GainsQuestion_GetAllQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<IEnumerable<GainsQuestionDto>>> Handle(GainsQuestion_GetAllQuery request, CancellationToken cancellationToken)
        {
            IEnumerable<GainsQuestionDto> customers = (await (from gq in _context.GainsQuestions
                                                              where gq.DeleteFlag != true
                                                              orderby gq.Code ascending
                                                              select new GainsQuestionDto()
                                                              {
                                                                  Id = gq.Id,
                                                                  Code = gq.Code ?? 0,
                                                                  Description = gq.Description ?? "",
                                                                  Content = gq.Content ?? ""
                                                              }).AsNoTracking().ToListAsync()).AsReadOnly();

            var eventLog = await _eventLogService.Create("GainsQuestionFeature", "GainsQuestionFeature",
                                                            "GainsQuestion_GetAllQuery", request.userId);

            return Result<IEnumerable<GainsQuestionDto>>.Success(customers);
        }
    }
}
