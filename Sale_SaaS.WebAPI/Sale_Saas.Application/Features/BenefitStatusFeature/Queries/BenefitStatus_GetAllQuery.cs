using Sale_Saas.Application.Features.BenefitStatusFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.BenefitStatusFeature.Queries
{
    public record BenefitStatus_GetAllQuery(Guid userId, GetAllQueryRequest RequestData) : IRequest<Result<IEnumerable<BenefitStatusDto>>>;
    public class BenefitStatus_GetAllQueryHandler : IRequestHandler<BenefitStatus_GetAllQuery, Result<IEnumerable<BenefitStatusDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public BenefitStatus_GetAllQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<IEnumerable<BenefitStatusDto>>> Handle(BenefitStatus_GetAllQuery request, CancellationToken cancellationToken)
        {
            IEnumerable<BenefitStatusDto> customers = (await (from con in _context.BenefitStatuses
                                                              where con.DeleteFlag != true
                                                              select new BenefitStatusDto()
                                                              {
                                                                  Id = con.Id,
                                                                  Code = con.Code ?? "",
                                                                  Name = con.Name ?? ""
                                                              }).AsNoTracking().ToListAsync()).AsReadOnly();

            var eventLog = await _eventLogService.Create("BenefitStatusFeature", "BenefitStatusFeature","BenefitStatus_GetAllQuery", request.userId);

            return Result<IEnumerable<BenefitStatusDto>>.Success(customers);
        }
    }
}
