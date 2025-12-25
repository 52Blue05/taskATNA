using Sale_Saas.Application.Features.BenefitFeature.Dto;
using Sale_Saas.Application.Features.BenefitStatusFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Models.Identity;

namespace Sale_Saas.Application.Features.BenefitFeature.Queries
{
    public record Benefit_GetByIdQuery(Guid userId, Guid Id) : IRequest<Result<BenefitDto>>;
    public class Benefit_GetByIdQueryHandler : IRequestHandler<Benefit_GetByIdQuery, Result<BenefitDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public Benefit_GetByIdQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<BenefitDto>> Handle(Benefit_GetByIdQuery request, CancellationToken cancellationToken)
        {
            BenefitDto? Benefit = await (from con in _context.Benefits
                                         join user in _context.ApplicationUsers on con.ApplicationUserId equals user.Id
                                         join status in _context.BenefitStatuses on con.BenefitStatusId equals status.Id
										 where con.DeleteFlag != true && con.Id == request.Id && user.DeleteFlag != true
										 select new BenefitDto()
                                         {
                                             Id = con.Id,
                                             TotalBenefit = con.TotalBenefit ?? 0,
                                             MonthlySalary = con.MonthlySalary ?? 0,
                                             TargetSalary = con.TargetSalary ?? 0,
                                             TotalSalary = con.TotalSalary ?? 0,
                                             SuggestTotalSalary = con.SuggestTotalSalary ?? 0,
                                             SuggestMonthlySalary = con.SuggestMonthlySalary ?? 0,
                                             SuggestTargetSalary = con.SuggestTargetSalary ?? 0,
                                             ApplicationUser = new UserBasicInfoDto()
                                             {
                                                 Id = user.Id,
                                                 FirstName = user.FirstName ?? "",
                                                 LastName = user.LastName ?? "",
                                                 FullName = user.FullName ?? ""
                                             },
                                             BenefitStatus = new BenefitStatusDto()
                                             {
                                                 Id = status.Id,
                                                 Code = status.Code ?? "",
                                                 Name = status.Name ?? ""
                                             },
                                         }).AsNoTracking().FirstOrDefaultAsync();

            var eventLog = await _eventLogService.Create("BenefitFeature", "BenefitFeature","Benefit_GetByIdQuery", request.userId);

            return Result<BenefitDto>.Success(Benefit);
        }
    }
}
