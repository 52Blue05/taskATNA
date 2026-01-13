using Sale_Saas.Application.Features.ContractStatusFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.ContractStatusFeature.Queries
{
    public record ContractStatus_GetByIdQuery(Guid userId, Guid Id) : IRequest<Result<ContractStatusDto>>;
    public class ContractStatus_GetByIdQueryHandler : IRequestHandler<ContractStatus_GetByIdQuery, Result<ContractStatusDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public ContractStatus_GetByIdQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<ContractStatusDto>> Handle(ContractStatus_GetByIdQuery request, CancellationToken cancellationToken)
        {
            ContractStatusDto? ContractStatus = await (from con in _context.ContractStatuses
                                                       where con.DeleteFlag != true && con.Id == request.Id
                                                       select new ContractStatusDto()
                                                       {
                                                           Id = con.Id,
                                                           Code = con.Code ?? "",
                                                           Name = con.Name ?? ""
                                                       }).AsNoTracking().FirstOrDefaultAsync();

            var eventLog = await _eventLogService.Create("ContractStatusFeature", "ContractStatusFeature",
                                       "ContractStatus_GetByIdQuery", request.userId);

            return Result<ContractStatusDto>.Success(ContractStatus);
        }
    }
}
