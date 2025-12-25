using Sale_Saas.Application.Features.CustomerFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.CustomerFeature.Queries
{

    public record Customer_GetAllQuery(Guid userId, GetAllQueryRequest RequestData) : IRequest<Result<IEnumerable<CustomerDto>>>;

    public class Customer_GetAllQueryHandler : IRequestHandler<Customer_GetAllQuery, Result<IEnumerable<CustomerDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public Customer_GetAllQueryHandler(IMapper mapper,
                                        IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<IEnumerable<CustomerDto>>> Handle(Customer_GetAllQuery request, CancellationToken cancellationToken)
        {
            var query = from cv in _context.Customers
                        where cv.DeleteFlag != true
                        select new CustomerDto()
                        {
                            Id = cv.Id,
                            Code = cv.Code ?? "",
                            Fullname = cv.Fullname ?? ""
                        };

            if (!string.IsNullOrEmpty(request.RequestData.TextSearch))
            {
                string search = request.RequestData.TextSearch.Trim().ToLower();
                query = query.Where(cv => cv.Fullname.ToLower().Contains(search) || 
                                          cv.Code.ToLower().Contains(search));
            }

            var customers = await query.AsNoTracking().ToListAsync();

            var eventLog = await _eventLogService.Create("CustomerFeature", "CustomerFeature",
                           "Customer_GetAllQuery", request.userId);

            return Result<IEnumerable<CustomerDto>>.Success(customers.AsReadOnly());
        }
    }
}
