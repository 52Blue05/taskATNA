using Sale_Saas.Application.Common.Interfaces;
using Sale_Saas.Application.Common.Mappings;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.MailServerInfoFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;


namespace Sale_Saas.Application.Features.MailServerInfoFeature.Queries
{
    public record MailServerInfo_GetListWithPaginationQuery(Guid userId, GetListWithPaginationQueryRequest RequestData) : IRequest<Result<PaginatedList<MailServerInfoDto>>>;
    public class MailServerInfo_GetListWithPaginationQueryHandler : IRequestHandler<MailServerInfo_GetListWithPaginationQuery, Result<PaginatedList<MailServerInfoDto>>>
    {        
		private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public MailServerInfo_GetListWithPaginationQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
        {            
			_context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<PaginatedList<MailServerInfoDto>>> Handle(MailServerInfo_GetListWithPaginationQuery request, CancellationToken cancellationToken)
        {
            var eventLog = await _eventLogService.Create("MailServerInfoFeature", "MailServerInfoFeature",
                            "MailServerInfo_GetListWithPaginationQuery", request.userId);

            return Result<PaginatedList<MailServerInfoDto>>.Success(await _context.MailServerInfos.Where(m => m.DeleteFlag != true)
                                    .OrderBy(x => x.Id)
                                    .ProjectTo<MailServerInfoDto>(_mapper.ConfigurationProvider)
                                    .PaginatedListAsync(request.RequestData.PageIndex, request.RequestData.PageSize));
        }
	}
}
