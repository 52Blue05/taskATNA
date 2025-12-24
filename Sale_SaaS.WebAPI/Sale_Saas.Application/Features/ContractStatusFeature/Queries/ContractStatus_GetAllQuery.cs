using Sale_Saas.Application.Features.ContractStatusFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.ContractStatusFeature.Queries
{
    public record ContractStatus_GetAllQuery(Guid userId, GetAllQueryRequest RequestData) : IRequest<Result<IEnumerable<ContractStatusDto>>>;
    public class ContractStatus_GetAllQueryHandler : IRequestHandler<ContractStatus_GetAllQuery, Result<IEnumerable<ContractStatusDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public ContractStatus_GetAllQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<ContractStatusDto>>> Handle(ContractStatus_GetAllQuery request, CancellationToken cancellationToken)
        {
            IEnumerable<ContractStatusDto> customers = (await (from con in _context.ContractStatuses
                                                              where con.DeleteFlag != true
                                                              select new ContractStatusDto()
                                                              {
                                                                  Id = con.Id,
                                                                  Code = con.Code ?? "",
                                                                  Name = con.Name ?? ""
                                                              }).AsNoTracking().ToListAsync()).AsReadOnly();

            var eventLog = await _eventLogService.Create("ContractStatusFeature", "ContractStatusFeature",
                                       "ContractStatus_GetAllQuery", request.userId);

            return Result<IEnumerable<ContractStatusDto>>.Success(customers);
        }
    }
}
