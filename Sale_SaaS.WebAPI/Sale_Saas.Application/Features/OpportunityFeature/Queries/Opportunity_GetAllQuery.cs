using Sale_Saas.Application.Features.CustomerFeature.Dto;
using Sale_Saas.Application.Features.OpportunityFeature.Dto;
using Sale_Saas.Application.Features.OpportunityStatusFeature.Dto;
using Sale_Saas.Application.Models.Identity;

namespace Sale_Saas.Application.Features.OpportunityFeature.Queries
{
    public record Opportunity_GetAllQuery(GetAllQueryRequest RequestData) : IRequest<Result<IEnumerable<OpportunityDto>>>;
    public class Opportunity_GetAllQueryHandler : IRequestHandler<Opportunity_GetAllQuery, Result<IEnumerable<OpportunityDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public Opportunity_GetAllQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<OpportunityDto>>> Handle(Opportunity_GetAllQuery request, CancellationToken cancellationToken)
        {
            IEnumerable<OpportunityDto> customers = (await (from cv in _context.Opportunities
                                                            where cv.DeleteFlag != true
                                                            join user in _context.ApplicationUsers on cv.ApplicationUserId equals user.Id into userGroup
                                                            from user in userGroup.DefaultIfEmpty()
                                                            join status in _context.OpportunityStatuses on cv.OpportunityStatusId equals status.Id
                                                            join customer in _context.Customers on cv.CustomerId equals customer.Id
                                                            select new OpportunityDto()
                                                            {
                                                                Id = cv.Id,
                                                                CustomerName = cv.CustomerName ?? "",
                                                                TechnicalLead = cv.TechnicalLead ?? "",
                                                                Need = cv.Need ?? "",
                                                                Budget = cv.Budget ?? 0,
                                                                Beneficiary = cv.Beneficiary ?? "",
                                                                Opponent1 = cv.Opponent1 ?? "",
                                                                Opponent1Attribute = cv.Opponent1Attribute ?? "",
                                                                Opponent1Strength = cv.Opponent1Strength ?? "",
                                                                Opponent1Weakness = cv.Opponent1Weakness ?? "",
                                                                Opponent2 = cv.Opponent2 ?? "",
                                                                Opponent2Attribute = cv.Opponent2Attribute ?? "",
                                                                Opponent2Strength = cv.Opponent2Strength ?? "",
                                                                Opponent2Weakness = cv.Opponent2Weakness ?? "",
                                                                Opponent3 = cv.Opponent3 ?? "",
                                                                Opponent3Attribute = cv.Opponent3Attribute ?? "",
                                                                Opponent3Strength = cv.Opponent3Strength ?? "",
                                                                Opponent3Weakness = cv.Opponent3Weakness ?? "",
                                                                Reason = cv.Reason ?? "",
                                                                EstimatedTime = cv.EstimatedTime ?? new DateTime(),
                                                                EstimatedMoney = cv.EstimatedMoney ?? 0,
                                                                CommissionMoney = cv.CommissionMoney ?? 0,
                                                                Strategy = cv.Strategy ?? "",
                                                                CreatedDate = cv.CreatedDate,
                                                                LastTimeInteract = cv.LastTimeInteract ?? new DateTime(),
                                                                WinningOppotunity = cv.WinningOppotunity ?? "",
                                                                OpportunityStartDate = cv.OpportunityStartDate ?? new DateTime(),
                                                                OpportunityEndDate = cv.OpportunityEndDate ?? new DateTime(),
                                                                Customer = customer == null || customer.DeleteFlag == true ? new CustomerDto() : new CustomerDto()
                                                                {
                                                                    Id = customer.Id,
                                                                    Fullname = customer.Fullname ?? "",
                                                                    Code = customer.Code ?? ""
                                                                },
                                                                ApplicationUser = user == null || user.DeleteFlag == true ? new UserBasicInfoDto() : new UserBasicInfoDto()
                                                                {
                                                                    Id = user.Id,
                                                                    FirstName = user.FirstName ?? "",
                                                                    LastName = user.LastName ?? "",
                                                                    FullName = (user.FirstName+" " ?? "") + (user.LastName ?? "")
                                                                },
                                                                OpportunityStatus = new OpportunityStatusDto()
                                                                {
                                                                    Id = status.Id,
                                                                    Name = status.Name ?? "",
                                                                    Code = status.Code ?? ""
                                                                }
                                                            }).AsNoTracking().ToListAsync()).AsReadOnly();

            return Result<IEnumerable<OpportunityDto>>.Success(customers);
        }
    }
}
