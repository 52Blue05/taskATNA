using Sale_Saas.Application.Features.ContractStatusFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.ContractStatusFeature.Queries
{
    public record ContractStatus_GetByNameQuery(Guid userId, string Name) : IRequest<Result<ContractStatusDto>>;
    public class ContractStatus_GetByNameQueryHandler : IRequestHandler<ContractStatus_GetByNameQuery, Result<ContractStatusDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public ContractStatus_GetByNameQueryHandler(IApplicationDbContext context, IMapper mapper, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<ContractStatusDto>> Handle(ContractStatus_GetByNameQuery request, CancellationToken cancellationToken)
        {
            ContractStatusDto? ContractStatus = await (from con in _context.ContractStatuses
                                                       where con.DeleteFlag != true && con.Name == request.Name
                                                       select new ContractStatusDto()
                                                       {
                                                           Id = con.Id,
                                                           Code = con.Code ?? "",
                                                           Name = con.Name ?? ""
                                                       }).AsNoTracking().FirstOrDefaultAsync();
            var eventLog = await _eventLogService.Create("ContractStatusFeature", "ContractStatusFeature",
                                       "ContractStatus_GetByNameQuery", request.userId);

            return Result<ContractStatusDto>.Success(ContractStatus);
        }
    }
}
