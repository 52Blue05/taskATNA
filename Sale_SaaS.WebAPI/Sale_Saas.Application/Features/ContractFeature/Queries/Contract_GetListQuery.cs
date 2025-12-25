using Sale_Saas.Application.Features.ContractFeature.Dto;
using Sale_Saas.Application.Features.ContractStatusFeature.Dto;
using Sale_Saas.Application.Features.CustomerFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.ContractFeature.Queries
{
    public record Contract_GetListQuery(Guid userId, FilterQueryRequest RequestData) : IRequest<Result<IEnumerable<ContractDto>>>;

    public class Contract_GetListQueryHandler : IRequestHandler<Contract_GetListQuery, Result<IEnumerable<ContractDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public Contract_GetListQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<IEnumerable<ContractDto>>> Handle(Contract_GetListQuery request, CancellationToken cancellationToken)
        {
            var query = from con in _context.Contracts
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
                        };

            if (!string.IsNullOrEmpty(request.RequestData.TextSearch))
            {
                query = query.Where(x => x.Code.Contains(request.RequestData.TextSearch) ||
                                         x.Name.Contains(request.RequestData.TextSearch));
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

            var eventLog = await _eventLogService.Create("ContractFeature", "ContractFeature","Contract_GetListQuery", request.userId);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<IEnumerable<ContractDto>>.Success(data);
        }
    }
}
