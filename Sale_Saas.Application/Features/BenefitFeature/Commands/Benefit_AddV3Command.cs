using Sale_Saas.Application.Features.BenefitFeature.Dto;
using Sale_Saas.Application.Features.BenefitFeature.Requests;
using Sale_Saas.Application.Features.BenefitFeature.Services;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.BenefitFeature.Commands;

public record Benefit_AddV3Command(Guid userId, BenefitMobileAddV3Request RequestData) : IRequest<Result<BenefitDto>>;

public class Benefit_AddV3CommandHandler : IRequestHandler<Benefit_AddV3Command, Result<BenefitDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IInternalService _internalService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IApplicationRoleService _applicationRoleService;
    private readonly IFeaturePermissionService _PermissionService;
    private readonly IEventLogService _eventLogService;

    public Benefit_AddV3CommandHandler(IMapper mapper,
                                            IApplicationDbContext context,
                                            IApplicationUserService applicationUserService,
                                            IApplicationRoleService applicationRoleService,
                                            IInternalService internalService,
                                            IFeaturePermissionService permissionService,
                                            IEventLogService eventLogService)
    {
        _context = context;
        _mapper = mapper;
        _internalService = internalService;
        _applicationUserService = applicationUserService;
        _applicationRoleService = applicationRoleService;
        _PermissionService = permissionService;
        _eventLogService = eventLogService;
    }

    public async Task<Result<BenefitDto>> Handle(Benefit_AddV3Command request, CancellationToken cancellationToken)
    {
        Validate(request.RequestData);

        List<ApplicationRole> userRoles = await _applicationRoleService.GetListRoleByUserId(request.RequestData.ApplicationUserId);

        string rolePositionId = request.RequestData.RolePositionId ?? "";

        if (request.RequestData.ApplicationRoleId == null || request.RequestData.ApplicationRoleId == Guid.Empty)
            request.RequestData.ApplicationRoleId = (await _applicationRoleService.GetIdByRoleName("SaleDirector")).Data;


        if (request.RequestData.RolePositionId == RolePositionEnum.EMPLOYEE.ToString())
        {
            await CheckDuplicatedBenefitOfUser(request.RequestData.ApplicationUserId, request.RequestData.ApplicationRoleId ?? Guid.Empty, userRoles, RolePositionEnum.EMPLOYEE.ToString());
        }
        else if (request.RequestData.RolePositionId == RolePositionEnum.MANAGER.ToString())
        {
            await CheckDuplicatedBenefitOfUser(request.RequestData.ApplicationUserId, request.RequestData.ApplicationRoleId ?? Guid.Empty, userRoles, RolePositionEnum.MANAGER.ToString());
        }

        await _PermissionService.HasPermission(MenuType.Sale_QL, FeatureType.CREATE, request.userId, true);
        var benefit = _mapper.Map<Benefit>(request.RequestData);

        var user = await _applicationUserService.FindAsync(benefit.ApplicationUserId);

        bool isManager = await _PermissionService.ContainsPosition(user.Id, RolePositionEnum.MANAGER.ToString());

        if (isManager)
        {
            await SetStatus(benefit, BenefitStatusEnum.CONFIRMED.ToString());
        }
        else
        {
            await SetStatus(benefit, BenefitStatusEnum.PENDING.ToString());
        }

        _context.Benefits.Add(benefit);

        await _context.SaveChangesAsync(cancellationToken);

        var benefitDto = _mapper.Map<BenefitDto>(benefit);

        var eventLog = await _eventLogService.Create("BenefitFeature", "BenefitFeature",
                "BeneftiMobile_AddOrUpdateV3Command", request.userId);

        return Result<BenefitDto>.Success(benefitDto);
    }

    private async Task SetStatus(Benefit obj, string code)
    {
        var status = await BenefitService.GetStatus(code, _context);
        obj.BenefitStatusId = status.Id;
        obj.BenefitStatus = status;
    }
    private void Validate(BenefitMobileAddV3Request request)
    {
        if (request == null)
        {
            throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
        }

        //StringHelper.IsNumberOutOfRange(StringHelper.DictGetValue(addOrUpdateRequest.Data, "TotalBenefit"), true);
        StringHelper.IsNumberOutOfRange(request.MonthlySalary.ToString(), true);
        StringHelper.IsNumberOutOfRange(request.TargetSalary.ToString(), true);
        StringHelper.IsNumberOutOfRange(request.TotalSalary.ToString(), true);
    }

    private async Task CheckDuplicatedBenefitOfUser(Guid applicationUserId, Guid applicationRoleId, List<ApplicationRole> userRoles, string rolePositionId)
    {
        var getBenefitByApplicationUserId = await _context.Benefits
                 .Where(x => x.ApplicationUserId == applicationUserId
                           && x.RolePositionId == rolePositionId
                           && x.DeleteFlag != true)
                 .Select(x => x.ApplicationRoleId)
                 .ToListAsync();

        var checkRoleId = userRoles.Where(x => x.Id == applicationRoleId);

        if (getBenefitByApplicationUserId.Contains(applicationRoleId))
            throw new ApplicationException($"Vai trò đã được cấp phát quyền lợi.");

        if (!checkRoleId.Any())
        {
            throw new ApplicationException($"Vai trò không hợp lệ.");
        }
    }
}
