using Sale_Saas.Application.Common.Mappings;
using Sale_Saas.Application.Features.CustomerFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.CustomerFeature.Queries
{
    public record Customer_GetListWithPaginationQuery(Guid userId, GetListWithPaginationQueryRequest RequestData) : IRequest<Result<PaginatedList<CustomerDto>>>;

    public class Customer_GetListWithPaginationQueryHandler : IRequestHandler<Customer_GetListWithPaginationQuery, Result<PaginatedList<CustomerDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public Customer_GetListWithPaginationQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<PaginatedList<CustomerDto>>> Handle(Customer_GetListWithPaginationQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Customers.Where(m => m.DeleteFlag != true)
                                          .OrderByDescending(x => x.CreatedDate)
                                          .ProjectTo<CustomerDto>(_mapper.ConfigurationProvider);

            if (!string.IsNullOrEmpty(request.RequestData.TextSearch))
            {
                query = query.Where(s => s.Fullname.ToLower().Contains(request.RequestData.TextSearch.ToLower()) ||
                                         s.Code.ToLower().Contains(request.RequestData.TextSearch.ToLower()));
            }

            PaginatedList<CustomerDto> result = await query.PaginatedListAsync(request.RequestData.PageIndex, request.RequestData.PageSize);

            var eventLog = await _eventLogService.Create("CustomerFeature", "CustomerFeature",
                           "Customer_GetListWithPaginationQuery", request.userId);

            return Result<PaginatedList<CustomerDto>>.Success(await query.PaginatedListAsync(request.RequestData.PageIndex, request.RequestData.PageSize));
        }
    }
}
