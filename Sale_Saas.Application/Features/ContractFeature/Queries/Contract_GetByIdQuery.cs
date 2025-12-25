using Sale_Saas.Application.Features.ContractFeature.Dto;
using Sale_Saas.Application.Features.ContractStatusFeature.Dto;
using Sale_Saas.Application.Features.CustomerFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
namespace Sale_Saas.Application.Features.ContractFeature.Queries
{
    public record Contract_GetByIdQuery(Guid userId, Guid Id) : IRequest<Result<ContractDto>>;
    public class Contract_GetByIdQueryHandler : IRequestHandler<Contract_GetByIdQuery, Result<ContractDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public Contract_GetByIdQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<ContractDto>> Handle(Contract_GetByIdQuery request, CancellationToken cancellationToken)
        {
            ContractDto? Contract = await (from con in _context.Contracts
                                           join customer in _context.Customers on con.CustomerId equals customer.Id
                                           join status in _context.ContractStatuses on con.ContractStatusId equals status.Id into statusGroup
                                           from status in statusGroup.DefaultIfEmpty()
										   where con.DeleteFlag != true && con.Id == request.Id && customer.DeleteFlag != true
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
                                           }).AsNoTracking().FirstOrDefaultAsync();

            var eventLog = await _eventLogService.Create("ContractFeature", "ContractFeature","Contract_GetByIdQuery", request.userId);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<ContractDto>.Success(Contract);
        }
    }
}
