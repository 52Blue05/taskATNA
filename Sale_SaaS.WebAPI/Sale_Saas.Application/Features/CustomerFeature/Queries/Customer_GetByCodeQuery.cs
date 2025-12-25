using Sale_Saas.Application.Features.CustomerFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.CustomerFeature.Queries
{
    public record Customer_GetByCodeQuery(Guid userId, string code) : IRequest<Result<CustomerDto>>;

    public class Customer_GetByCodeQueryHandler : IRequestHandler<Customer_GetByCodeQuery, Result<CustomerDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public Customer_GetByCodeQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<CustomerDto>> Handle(Customer_GetByCodeQuery request, CancellationToken cancellationToken)
        {
            CustomerDto? customer = await (from cv in _context.Customers
                                           where cv.DeleteFlag != true && cv.Code == request.code
                                           select new CustomerDto()
                                           {
                                               Id = cv.Id,
                                               Code = cv.Code ?? "",
                                               Fullname = cv.Fullname ?? ""
                                           }).AsNoTracking().FirstOrDefaultAsync();
            var eventLog = await _eventLogService.Create("CustomerFeature", "CustomerFeature",
                           "Customer_GetByCodeQuery", request.userId);

            return Result<CustomerDto>.Success(customer);
        }
    }
}
