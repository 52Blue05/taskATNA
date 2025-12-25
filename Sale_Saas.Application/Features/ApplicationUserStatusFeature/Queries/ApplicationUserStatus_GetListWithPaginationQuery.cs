using Sale_Saas.Application.Common.Mappings;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.ApplicationUserStatusFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;


namespace Sale_Saas.Application.Features.ApplicationUserStatusFeature.Queries
{
    public record ApplicationUserStatus_GetListWithPaginationQuery(Guid userId, GetListWithPaginationQueryRequest RequestData) : IRequest<Result<PaginatedList<ApplicationUserStatusDto>>>;

    public class ApplicationUserStatus_GetListWithPaginationQueryHandler : IRequestHandler<ApplicationUserStatus_GetListWithPaginationQuery, Result<PaginatedList<ApplicationUserStatusDto>>>
    {        
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public ApplicationUserStatus_GetListWithPaginationQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
        {            
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<PaginatedList<ApplicationUserStatusDto>>> Handle(ApplicationUserStatus_GetListWithPaginationQuery request, CancellationToken cancellationToken)
        {
            var eventLog = await _eventLogService.Create("ApplicationUserStatusFeature", "ApplicationUserStatusFeature",
            "ApplicationUserStatus_GetListWithPaginationQuery", request.userId);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<PaginatedList<ApplicationUserStatusDto>>.Success(await _context.ApplicationUserStatuses.Where(m => m.DeleteFlag != true)
                                         .OrderBy(x => x.Id)
                                         .ProjectTo<ApplicationUserStatusDto>(_mapper.ConfigurationProvider)
                                         .PaginatedListAsync(request.RequestData.PageIndex, request.RequestData.PageSize));
        }
	}
}
