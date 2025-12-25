using Sale_Saas.Application.Features.ContractFeature.Dto;
using Sale_Saas.Application.Features.ContractStatusFeature.Dto;
using Sale_Saas.Application.Features.CustomerFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.ContractFeature.Queries
{
    public record Contract_GetAllQuery(Guid userId, GetAllQueryRequest RequestData) : IRequest<Result<IEnumerable<ContractDto>>>;
    public class Contract_GetAllQueryHandler : IRequestHandler<Contract_GetAllQuery, Result<IEnumerable<ContractDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public Contract_GetAllQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<IEnumerable<ContractDto>>> Handle(Contract_GetAllQuery request, CancellationToken cancellationToken)
        {
            IEnumerable<ContractDto> customers = (await (from con in _context.Contracts
                                                         join customer in _context.Customers on con.CustomerId equals customer.Id
                                                         join status in _context.ContractStatuses on con.ContractStatusId equals status.Id into statusGroup
                                                         from status in statusGroup.DefaultIfEmpty()
														 where con.DeleteFlag != true && customer.DeleteFlag != true
														 select new ContractDto()
                                                         {
                                                             Id = con.Id,
                                                             Code = con.Code ?? "",
                                                             Name = con.Name ?? "",
                                                             Number = con.Number ?? "",
                                                             StartDate = con.StartDate ?? new DateTime(),
                                                             EndDate = con.EndDate ?? new DateTime(),
                                                             ContractStatus = status != null ? new ContractStatusDto()
                                                             {
                                                                 Id = status.Id,
                                                                 Code = status.Code ?? "",
                                                                 Name = status.Name ?? ""
                                                             } : new ContractStatusDto(),
                                                             Customer = new CustomerDto()
                                                             {
                                                                 Id = customer.Id,
                                                                 Code = customer.Code ?? "",
                                                                 Fullname = customer.Fullname ?? ""
                                                             }

                                                         }).AsNoTracking().ToListAsync()).AsReadOnly();

            var eventLog = await _eventLogService.Create("ContractFeature", "ContractFeature","Contract_GetAllQuery", request.userId);

            return Result<IEnumerable<ContractDto>>.Success(customers);
        }
    }
}
