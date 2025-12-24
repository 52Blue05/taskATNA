using Sale_Saas.Application.Features.BenefitStatusFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.BenefitStatusFeature.Queries
{
    public record BenefitStatus_GetByIdQuery(Guid userId, Guid Id) : IRequest<Result<BenefitStatusDto>>;
    public class BenefitStatus_GetByIdQueryHandler : IRequestHandler<BenefitStatus_GetByIdQuery, Result<BenefitStatusDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public BenefitStatus_GetByIdQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<BenefitStatusDto>> Handle(BenefitStatus_GetByIdQuery request, CancellationToken cancellationToken)
        {
            BenefitStatusDto? BenefitStatus = await (from con in _context.BenefitStatuses
                                                     where con.DeleteFlag != true && con.Id == request.Id
                                                     select new BenefitStatusDto()
                                                     {
                                                         Id = con.Id,
                                                         Code = con.Code ?? "",
                                                         Name = con.Name ?? ""
                                                     }).AsNoTracking().FirstOrDefaultAsync();

            var eventLog = await _eventLogService.Create("BenefitStatusFeature", "BenefitStatusFeature","BenefitStatus_GetByIdQuery", request.userId);

            return Result<BenefitStatusDto>.Success(BenefitStatus);
        }
    }
}
