using Sale_Saas.Application.Features.OpportunityFeature.Dto;
using Sale_Saas.Application.Features.OpportunityFeature.Requests;
using Sale_Saas.Application.Features.OpportunityFeature.Services;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Domain.Constants.API;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.OpportunityFeature.Commands;
public record Opportunity_AddV2Command(Guid UserId, CreateOrUpdateOpportunityV2Request RequestData) : IRequest<Result<OpportunityDto>>;

public class Opportunity_AddV2CommandHandler : IRequestHandler<Opportunity_AddV2Command, Result<OpportunityDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IInternalService _internalService;
    private readonly IFeaturePermissionService _permissionService;
    private readonly IApplicationRoleService _roleService;

    public Opportunity_AddV2CommandHandler(IMapper mapper,
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

    public async Task<Result<OpportunityDto>> Handle(Opportunity_AddV2Command request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy user");
        }

        // check permission
        await _permissionService.HasPermission(MenuType.Sale_CH, FeatureType.CREATE, request.UserId, true);

        // get current role
        string currentRole = await _roleService.GetCurrentRoleOfUser(request.UserId);

        var opportunity = _mapper.Map<Opportunity>(request.RequestData);
        opportunity.ApplicationUserId = request.UserId;
        opportunity.CreatedApplicationUserId = currentRole == RolePositionEnum.MANAGER.ToString() ? request.UserId : null;
        opportunity.LastModifiedApplicationUserId = request.UserId;

        var statusString = OpportunityStatusEnum.PENDING.ToString();

        var status = await OpportunityService.GetStatus(statusString, _context);
        opportunity.OpportunityStatusId = status.Id;
        opportunity.OpportunityStatus = status;
        opportunity.Customer = await OpportunityService.GetCustomer(opportunity.CustomerId ?? Guid.Empty, _context);
        opportunity.CustomerName = opportunity.Customer.Fullname;

        var applicationUser = await OpportunityService.GetApplicationUser(opportunity.ApplicationUserId ?? Guid.Empty, _context);
        opportunity.ApplicationUser = applicationUser;

        _context.Opportunities.Add(opportunity);

        await _context.SaveChangesAsync(cancellationToken);

        var result = _mapper.Map<OpportunityDto>(opportunity);

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
