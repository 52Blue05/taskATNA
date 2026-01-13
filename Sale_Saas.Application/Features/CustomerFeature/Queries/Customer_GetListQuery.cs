using Sale_Saas.Application.Features.CustomerFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.CustomerFeature.Queries
{
    public record Customer_GetListQuery(Guid userId, FilterQueryRequest RequestData) : IRequest<Result<IEnumerable<CustomerDto>>>;

    public class Customer_GetListQueryHandler : IRequestHandler<Customer_GetListQuery, Result<IEnumerable<CustomerDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public Customer_GetListQueryHandler(IMapper mapper,IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<IEnumerable<CustomerDto>>> Handle(Customer_GetListQuery request, CancellationToken cancellationToken)
        {
            var query = from u in _context.Customers
                        where u.DeleteFlag != true
                        select new CustomerDto()
                        {
                            Id = u.Id,
                            Code = u.Code ?? "",
                            Fullname = u.Fullname ?? ""
                        };

            if (!string.IsNullOrEmpty(request.RequestData.TextSearch))
            {
                query = query.Where(x => x.Fullname.Contains(request.RequestData.TextSearch) || 
                                         x.Code.Contains(request.RequestData.TextSearch));
            }

            if (!string.IsNullOrEmpty(request.RequestData.Code))
            {
                query = query.Where(x => x.Code.Contains(request.RequestData.Code));
            }

            if (request.RequestData.Skip != null)
            {
                query = query.Skip(request.RequestData.Skip.Value);
            }

            if (request.RequestData.TotalRecord != null)
            {
                query = query.Take(request.RequestData.TotalRecord.Value);
            }

            var data = await query.AsNoTracking().ToListAsync();

            var eventLog = await _eventLogService.Create("CustomerFeature", "CustomerFeature",
                           "Customer_GetListQuery", request.userId);

            return Result<IEnumerable<CustomerDto>>.Success(data);
        }
    }
}
