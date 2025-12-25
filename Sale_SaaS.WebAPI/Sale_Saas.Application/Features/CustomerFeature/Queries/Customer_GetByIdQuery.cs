using Sale_Saas.Application.Features.CustomerFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.CustomerFeature.Queries
{
    public record Customer_GetByIdQuery(Guid userId, Guid Id) : IRequest<Result<CustomerDto>>;

    public class Customer_GetByIdQueryHandler : IRequestHandler<Customer_GetByIdQuery, Result<CustomerDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public Customer_GetByIdQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<CustomerDto>> Handle(Customer_GetByIdQuery request, CancellationToken cancellationToken)
        {
            CustomerDto? customer = await (from cv in _context.Customers
                                           where cv.DeleteFlag != true && cv.Id == request.Id
                                           select new CustomerDto()
                                           {
                                               Id = cv.Id,
                                               Code = cv.Code ?? "",
                                               Fullname = cv.Fullname ?? ""
                                           }).AsNoTracking().FirstOrDefaultAsync();

            var eventLog = await _eventLogService.Create("CustomerFeature", "CustomerFeature",
                           "Customer_GetByIdQuery", request.userId);

            return Result<CustomerDto>.Success(customer);
        }
    }
}
