using Sale_Saas.Application.Features.MailServerInfoFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;


namespace Sale_Saas.Application.Features.MailServerInfoFeature.Queries
{
    public record MailServerInfo_GetAllQuery(Guid userId, GetAllQueryRequest RequestData) : IRequest<Result<IEnumerable<MailServerInfoDto>>>;
    public class MailServerInfo_GetAllQueryHandler : IRequestHandler<MailServerInfo_GetAllQuery, Result<IEnumerable<MailServerInfoDto>>>
    {				
		private readonly IApplicationDbContext _context;
		private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public MailServerInfo_GetAllQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)		
		{			
			_context = context;
			_mapper = mapper;
            _eventLogService = eventLogService;
		}
        public async Task<Result<IEnumerable<MailServerInfoDto>>> Handle(MailServerInfo_GetAllQuery request, CancellationToken cancellationToken)
        {
            var eventLog = await _eventLogService.Create("MailServerInfoFeature", "MailServerInfoFeature",
                            "MailServerInfo_GetAllQuery", request.userId);

            return Result<IEnumerable<MailServerInfoDto>>.Success((await (from cv in _context.MailServerInfos
                                                               where cv.DeleteFlag != true
                                                               select new MailServerInfoDto()
                                                               {
                                                                   Id = cv.Id,
                                                                   Ten = cv.Ten ?? string.Empty,
                                                               })
                                                          .AsNoTracking()
                                                          .ToListAsync())
                                                          .AsReadOnly());
        }
	}
}
