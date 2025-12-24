using Sale_Saas.Application.Common.Mappings;
using Sale_Saas.Application.Features.ContractFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.ContractFeature.Queries
{
    public record Contract_GetListWithPaginationQuery(Guid userId, GetListWithPaginationQueryRequest RequestData) : IRequest<Result<PaginatedList<ContractDto>>>;

    public class Contract_GetListWithPaginationQueryHandler : IRequestHandler<Contract_GetListWithPaginationQuery, Result<PaginatedList<ContractDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public Contract_GetListWithPaginationQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<PaginatedList<ContractDto>>> Handle(Contract_GetListWithPaginationQuery request, CancellationToken cancellationToken)
        {
            var query =  _context.Contracts.Where(m => m.DeleteFlag != true && m.Customer != null && m.Customer.DeleteFlag != true)
										   .Include(s => s.ContractStatus)
                                           .Include(s => s.Customer)
                                           .OrderByDescending(x => x.CreatedDate)
                                           .ProjectTo<ContractDto>(_mapper.ConfigurationProvider)
                                           .AsNoTracking();

            if (!string.IsNullOrEmpty(request.RequestData.TextSearch))
            {
                query = query.Where(s => s.Name.ToLower().Contains(request.RequestData.TextSearch.ToLower()) ||
                                         s.Code.ToLower().Contains(request.RequestData.TextSearch.ToLower()) ||
                                         s.Number.ToLower().Contains(request.RequestData.TextSearch.ToLower()));
            }

            var eventLog = await _eventLogService.Create("ContractFeature", "ContractFeature","Contract_GetListWithPaginationQuery", request.userId);

            return Result<PaginatedList<ContractDto>>.Success(await query.PaginatedListAsync(request.RequestData.PageIndex, request.RequestData.PageSize));
        }
    }
}
