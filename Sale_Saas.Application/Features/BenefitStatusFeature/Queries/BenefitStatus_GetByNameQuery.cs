
using Sale_Saas.Application.Features.BenefitStatusFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.BenefitStatusFeature.Queries
{
    public record BenefitStatus_GetByNameQuery(Guid userId, string Name) : IRequest<Result<BenefitStatusDto>>;
    public class BenefitStatus_GetByNameQueryHandler : IRequestHandler<BenefitStatus_GetByNameQuery, Result<BenefitStatusDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public BenefitStatus_GetByNameQueryHandler(IApplicationDbContext context, IMapper mapper, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<BenefitStatusDto>> Handle(BenefitStatus_GetByNameQuery request, CancellationToken cancellationToken)
        {
            BenefitStatusDto? benefitStatus = await (from benefit in _context.BenefitStatuses
                                               where benefit.DeleteFlag != true && benefit.Name == request.Name
                                               select new BenefitStatusDto()
                                               {
                                                   Id = benefit.Id,
                                                   Code = benefit.Code ?? "",
                                                   Name = benefit.Name ?? ""
                                               }).AsNoTracking().FirstOrDefaultAsync();

            var eventLog = await _eventLogService.Create("BenefitStatusFeature", "BenefitStatusFeature","BenefitStatus_GetByNameQuery", request.userId);

            return Result<BenefitStatusDto>.Success(benefitStatus);
        }
    }
}
