using Sale_Saas.Application.Features.MailServerInfoFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.MailServerInfoFeature.Queries
{
    public record MailServerInfo_GetByIdQuery(Guid userId, Guid Id) : IRequest<Result<MailServerInfoDto>>;
    public class MailServerInfo_GetByIdQueryHandler : IRequestHandler<MailServerInfo_GetByIdQuery, Result<MailServerInfoDto>>
    {        
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public MailServerInfo_GetByIdQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
        {            
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<MailServerInfoDto>> Handle(MailServerInfo_GetByIdQuery request, CancellationToken cancellationToken)
        {
            var eventLog = await _eventLogService.Create("MailServerInfoFeature", "MailServerInfoFeature",
                            "MailServerInfo_GetByIdQuery", request.userId);

            return Result<MailServerInfoDto>.Success(await (from cv in _context.MailServerInfos
                                                    where cv.DeleteFlag != true && cv.Id == request.Id
                                                    select new MailServerInfoDto()
                                                    {
                                                        Id = cv.Id,
                                                        Ten = cv.Ten ?? string.Empty,
                                                    })
                                                    .AsNoTracking()
                                                    .FirstOrDefaultAsync());
        }
    }
}
