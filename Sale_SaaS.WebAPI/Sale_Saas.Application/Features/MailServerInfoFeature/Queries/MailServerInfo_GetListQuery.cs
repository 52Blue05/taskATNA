using Sale_Saas.Application.Features.MailServerInfoFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;


namespace Sale_Saas.Application.Features.MailServerInfoFeature.Queries
{
    public record MailServerInfo_GetListQuery(Guid userId, FilterQueryRequest RequestData) : IRequest<Result<IEnumerable<MailServerInfoDto>>>;
    public class MailServerInfo_GetListQueryHandler : IRequestHandler<MailServerInfo_GetListQuery, Result<IEnumerable<MailServerInfoDto>>>
    {				
		private readonly IApplicationDbContext _context;
		private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public MailServerInfo_GetListQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)		
		{			
			_context = context;
			_mapper = mapper;
			_eventLogService = eventLogService;
		}
        public async Task<Result<IEnumerable<MailServerInfoDto>>> Handle(MailServerInfo_GetListQuery request, CancellationToken cancellationToken)
        {
			var query = from cv in _context.MailServerInfos
						where cv.DeleteFlag != true
						select new MailServerInfoDto()
						{
							Id = cv.Id,							
							Ten = cv.Ten ?? string.Empty,
						};

			if (request.RequestData.Skip != null)
			{
				query = query.Skip(request.RequestData.Skip.Value);
			}

			if (request.RequestData.TotalRecord != null)
			{
				query = query.Take(request.RequestData.TotalRecord.Value);
			}

            var eventLog = await _eventLogService.Create("MailServerInfoFeature", "MailServerInfoFeature",
                            "MailServerInfo_GetListQuery", request.userId);

            return Result<IEnumerable<MailServerInfoDto>>.Success((await query.AsNoTracking()
																	.ToListAsync())
																	.AsReadOnly());
        }
	}
}
