using Sale_Saas.Application.Features.BenefitFeature.Dto;
using Sale_Saas.Application.Features.BenefitStatusFeature.Dto;
using Sale_Saas.Application.Features.ContractStatusFeature.Dto;
using Sale_Saas.Application.Features.CustomerFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Models.Identity;

namespace Sale_Saas.Application.Features.BenefitFeature.Queries
{
    public record Benefit_GetListQuery(Guid userId, FilterQueryRequest RequestData) : IRequest<Result<IEnumerable<BenefitDto>>>;

    public class Benefit_GetListQueryHandler : IRequestHandler<Benefit_GetListQuery, Result<IEnumerable<BenefitDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public Benefit_GetListQueryHandler(IMapper mapper, IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _eventLogService = eventLogService;
        }

        public async Task<Result<IEnumerable<BenefitDto>>> Handle(Benefit_GetListQuery request, CancellationToken cancellationToken)
        {
            var query = from con in _context.Benefits
                        join user in _context.ApplicationUsers on con.ApplicationUserId equals user.Id
                        join status in _context.BenefitStatuses on con.BenefitStatusId equals status.Id
						where con.DeleteFlag != true && user.DeleteFlag != true
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
                        };

            if (!string.IsNullOrEmpty(request.RequestData.TextSearch))
            {
                query = query.Where(x => x.ApplicationUser!.LastName.Contains(request.RequestData.TextSearch) ||
                                         x.ApplicationUser!.FirstName.Contains(request.RequestData.TextSearch));
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

            var eventLog = await _eventLogService.Create("BenefitFeature", "BenefitFeature","Benefit_GetListQuery", request.userId);

            return Result<IEnumerable<BenefitDto>>.Success(data);
        }
    }
}
