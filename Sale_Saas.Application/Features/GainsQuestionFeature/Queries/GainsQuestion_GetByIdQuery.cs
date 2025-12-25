using Sale_Saas.Application.Features.GainsQuestionFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.GainsQuestionFeature.Queries
{
    public record GainsQuestion_GetByIdQuery(Guid userId, Guid Id) : IRequest<Result<GainsQuestionDto>>;
    public class GainsQuestion_GetByIdQueryHandler : IRequestHandler<GainsQuestion_GetByIdQuery, Result<GainsQuestionDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public GainsQuestion_GetByIdQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<GainsQuestionDto>> Handle(GainsQuestion_GetByIdQuery request, CancellationToken cancellationToken)
        {
            GainsQuestionDto? GainsQuestion = await (from gq in _context.GainsQuestions
                                                     where gq.DeleteFlag != true && gq.Id == request.Id
                                                     select new GainsQuestionDto()
                                                     {
                                                         Id = gq.Id,
                                                         Code = gq.Code ?? 0,
                                                         Description = gq.Description ?? "",
                                                         Content = gq.Content ?? ""
                                                     }).AsNoTracking().FirstOrDefaultAsync();

            var eventLog = await _eventLogService.Create("GainsQuestionFeature", "GainsQuestionFeature",
                                                            "GainsQuestion_GetByIdQuery", request.userId);

            return Result<GainsQuestionDto>.Success(GainsQuestion);
        }
    }
}
