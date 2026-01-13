using Sale_Saas.Application.Features.OpportunityFeature.Dto;
using Sale_Saas.Application.Features.OpportunityFeature.Requests;
using Sale_Saas.Application.Features.OpportunityFeature.Services;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Domain.Constants.API;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.OpportunityFeature.Commands;

public record Opportunity_UpdateV2Command(Guid UserId, CreateOrUpdateOpportunityV2Request RequestData) : IRequest<Result<OpportunityDto>>;

public class Opportunity_UpdateV2CommandHandler : IRequestHandler<Opportunity_UpdateV2Command, Result<OpportunityDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IInternalService _internalService;
    private readonly IFeaturePermissionService _permissionService;
    private readonly IApplicationRoleService _roleService;

    public Opportunity_UpdateV2CommandHandler(IMapper mapper,
                                            IApplicationDbContext context,
                                            IInternalService internalService,
                                            IFeaturePermissionService permissionService,
                                            IApplicationRoleService roleService)
    {
        _context = context;
        _mapper = mapper;
        _internalService = internalService;
        _permissionService = permissionService;
        _roleService = roleService;
    }

    public async Task<Result<OpportunityDto>> Handle(Opportunity_UpdateV2Command request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy user");
        }

        // check permission
        await _permissionService.HasPermission(MenuType.Sale_CH, FeatureType.UPDATE, request.UserId, true);

        var opportunity = await OpportunityService.GetOpportunity(request.RequestData.Id ?? Guid.Empty, _context);

        opportunity.CustomerId = request.RequestData.CustomerId ?? opportunity.CustomerId;
        opportunity.Accountable = request.RequestData.Accountable ?? opportunity.Accountable;
        opportunity.TechnicalLead = request.RequestData.TechnicalLead ?? opportunity.TechnicalLead;
        opportunity.Beneficiary = request.RequestData.Beneficiary ?? opportunity.Beneficiary;
        opportunity.Need = request.RequestData.Need ?? opportunity.Need;
        opportunity.OpportunityStartDate = request.RequestData.OpportunityStartDate ?? opportunity.OpportunityStartDate;
        opportunity.OpportunityEndDate = request.RequestData.OpportunityEndDate ?? opportunity.OpportunityEndDate;
        opportunity.Budget = request.RequestData.Budget ?? opportunity.Budget;
        opportunity.TypeMoney = request.RequestData.TypeMoney ?? opportunity.TypeMoney;
        opportunity.CurrencyConversion = request.RequestData.CurrencyConversion ?? opportunity.CurrencyConversion;
        opportunity.EstimatedMoney = request.RequestData.EstimatedMoney ?? opportunity.EstimatedMoney;
        opportunity.CommissionMoney = request.RequestData.CommissionMoney ?? opportunity.CommissionMoney;
        opportunity.Strategy = request.RequestData.Strategy ?? opportunity.Strategy;
        opportunity.WinningOppotunity = request.RequestData.WinningOppotunity ?? opportunity.WinningOppotunity;
        opportunity.EstimatedTime = request.RequestData.EstimatedTime ?? opportunity.EstimatedTime;
        opportunity.Reason = request.RequestData.Reason ?? opportunity.Reason;

        opportunity.LastModifiedApplicationUserId = request.UserId;

        var status = await OpportunityService.GetStatusById(opportunity.OpportunityStatusId ?? Guid.Empty, _context);
        opportunity.OpportunityStatus = status;

        opportunity.Customer = await OpportunityService.GetCustomer(opportunity.CustomerId ?? Guid.Empty, _context);

        var applicationUser = await OpportunityService.GetApplicationUser(opportunity.ApplicationUserId ?? Guid.Empty, _context);
        opportunity.ApplicationUser = applicationUser;

        _context.Opportunities.Update(opportunity);

        if (request.RequestData.OpportunityOpponents == null || request.RequestData.OpportunityOpponents.Count <= 0)
        {
            // remove all
            foreach (var item in opportunity.OpportunityOpponents)
            {
                _context.OpportunityOpponents.Remove(item);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            // remove not contain
            var requestIds = request.RequestData.OpportunityOpponents.Select(s => s.Id).ToList();
            foreach (var item in opportunity.OpportunityOpponents)
            {
                if (!requestIds.Contains(item.Id))
                {
                    _context.OpportunityOpponents.Remove(item);
                }
                else
                {
                    item.OpportunityId = opportunity.Id;
                    _context.OpportunityOpponents.Update(item);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            // add new
            var listOpponentNeedCreate = request.RequestData.OpportunityOpponents.Where(s => !s.Id.HasValue).ToList();
            var listOpponent = _mapper.Map<List<OpportunityOpponent>>(listOpponentNeedCreate);
            foreach (var item in listOpponent)
            {
                item.OpportunityId = opportunity.Id;
                _context.OpportunityOpponents.Add(item);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        var opponents = await _context.OpportunityOpponents.Where(s => s.DeleteFlag != true && s.OpportunityId == opportunity.Id).ToListAsync();
        var opponentsMap = _mapper.Map<List<OpportunityOpponentDto>>(opponents);

        await _context.SaveChangesAsync(cancellationToken);

        var result = _mapper.Map<OpportunityDto>(opportunity);
        result.OpportunityOpponents = opponentsMap;

        return Result<OpportunityDto>.Success(result);
    }

    private void Validate(CreateOrUpdateOpportunityV2Request request)
    {
        StringHelper.IsValidLength(StringInfoConstant.NameLimit, request.Accountable, true);
        StringHelper.IsValidLength(StringInfoConstant.NameLimit, request.TechnicalLead, true);
        StringHelper.IsValidLength(StringInfoConstant.NameLimit, request.Beneficiary, true);
        StringHelper.IsValidLength(StringInfoConstant.DesLimit, request.Reason, true);
        StringHelper.IsValidLength(StringInfoConstant.DesLimit, request.Need, true);
        StringHelper.IsValidLength(StringInfoConstant.DesLimit, request.Strategy, true);
        StringHelper.IsValidLength(StringInfoConstant.DesLimit, request.WinningOppotunity, true);

        StringHelper.IsNumberOutOfRange(request.EstimatedMoney.ToString(), true);
        StringHelper.IsNumberOutOfRange(request.CommissionMoney.ToString(), true);
    }
}
