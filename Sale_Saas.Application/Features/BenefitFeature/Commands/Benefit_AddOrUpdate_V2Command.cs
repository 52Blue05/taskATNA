using Sale_Saas.Application.Features.BenefitFeature.Dto;
using Sale_Saas.Application.Features.BenefitFeature.Services;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Domain.Enums;
using System.Data;

namespace Sale_Saas.Application.Features.BenefitFeature.Commands
{
    public record Benefit_AddOrUpdate_V2Command(Guid userId, List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<BenefitDto>>>;

    public class Benefit_AddOrUpdate_V2CommandHandler : IRequestHandler<Benefit_AddOrUpdate_V2Command, Result<List<BenefitDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;
        private readonly IApplicationUserService _applicationUserService;
        private readonly IApplicationRoleService _applicationRoleService;
        private readonly IFeaturePermissionService _PermissionService;
        private readonly IEventLogService _eventLogService;

        public Benefit_AddOrUpdate_V2CommandHandler(IMapper mapper,
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

        public async Task<Result<List<BenefitDto>>> Handle(Benefit_AddOrUpdate_V2Command request, CancellationToken cancellationToken)
        {
            Benefit? obj = null;
            List<BenefitDto> updatedSuccess = new List<BenefitDto>();

            Guid? applicationRoleId = new Guid();

            foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
            {

                Benefit tmp = new Benefit();
                Validate(addOrUpdateRequest);

                Guid applicationUserId = new Guid();
                applicationUserId = Guid.Parse(addOrUpdateRequest.Data["ApplicationUserId"]);
                List<ApplicationRole> userRoles = await _applicationRoleService.GetListRoleByUserId(applicationUserId);

                string rolePositionId = string.Empty;
                rolePositionId = addOrUpdateRequest.Data["RolePositionId"];

                Guid userRoleId = new Guid();
                if (!addOrUpdateRequest.Data.ContainsKey("ApplicationRoleId"))
                    userRoleId = (await _applicationRoleService.GetIdByRoleName("SaleDirector")).Data;
                else
                    userRoleId = Guid.Parse(addOrUpdateRequest.Data["ApplicationRoleId"]);

                if (rolePositionId == RolePositionEnum.EMPLOYEE.ToString())
                {
                    await CheckDuplicateBenefit(applicationUserId, RolePositionEnum.EMPLOYEE.ToString(), userRoleId, userRoles);
                    applicationRoleId = userRoleId;
                }
                else if (rolePositionId == RolePositionEnum.MANAGER.ToString())
                {
                    await CheckDuplicateBenefit(applicationUserId, RolePositionEnum.MANAGER.ToString(), userRoleId, userRoles);
                    applicationRoleId = userRoleId;
                }
                else
                {
                    applicationRoleId = userRoleId;
                }

                if (addOrUpdateRequest.Id == null)
                {
                    await _PermissionService.HasPermission(MenuType.Sale_QL, FeatureType.CREATE, request.userId, true);
                    obj = new Benefit()
                    {
                        CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId
                    };
                }
                else
                {
                    await _PermissionService.HasPermission(MenuType.Sale_QL, FeatureType.UPDATE, request.userId, true);
                    obj = await BenefitService.GetBenefit(addOrUpdateRequest.Id.Value, _context);
                    PropertiesExtension.Copy(obj, tmp);

                }

                obj = (Benefit)_internalService.MapValueToObject(new Benefit(), addOrUpdateRequest.Data!, obj);
                obj.LastModifiedDate = DateTime.Now;
                obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;
                obj.BenefitStatusId = tmp.BenefitStatusId ?? obj.BenefitStatusId;
                obj.ApplicationUserId = tmp.ApplicationUserId ?? obj.ApplicationUserId;
                obj.TotalBenefit = obj.TotalBenefit ?? tmp.TotalBenefit;
                obj.SuggestTotalSalary = null;
                obj.SuggestMonthlySalary = null;
                obj.SuggestTargetSalary = null;
                obj.ApplicationRoleId = applicationRoleId;

                var user = await _applicationUserService.FindAsync(obj.ApplicationUserId);

                if (addOrUpdateRequest.Id == null)
                {

                    bool isManager = await _PermissionService.ContainsPosition(user.Id, RolePositionEnum.MANAGER.ToString());

                    if (isManager)
                    {
                        await SetStatus(obj, BenefitStatusEnum.CONFIRMED.ToString());
                    }
                    else
                    {
                        await SetStatus(obj, BenefitStatusEnum.PENDING.ToString());
                    }

                    _context.Benefits.Add(obj);
                }
                else
                {

                    if (obj.BenefitStatus != null && obj.BenefitStatus.Code == BenefitStatusEnum.CONFIRMED.ToString())
                    {
                        await SetStatus(obj, BenefitStatusEnum.UPDATED.ToString());
                        obj.SuggestMonthlySalary = null;
                        obj.SuggestTargetSalary = null;
                        obj.SuggestTotalSalary = null;
                    }
                    _context.Benefits.Update(obj);
                }

                updatedSuccess.Add(_mapper.Map<BenefitDto>(obj));
            }
            var eventLog = await _eventLogService.Create("BenefitFeature", "BenefitFeature",
                    "Benefit_AddOrUpdate_V2Command", request.userId);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<List<BenefitDto>>.Success(updatedSuccess);
        }

        private async Task SetStatus(Benefit obj, string code)
        {
            var status = await BenefitService.GetStatus(code, _context);
            obj.BenefitStatusId = status.Id;
            obj.BenefitStatus = status;
        }
        private void Validate(AddOrUpdateRequest addOrUpdateRequest)
        {
            if (addOrUpdateRequest.Data == null)
            {
                throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
            }
            if (string.IsNullOrEmpty(StringHelper.DictGetValue(addOrUpdateRequest.Data, "ApplicationUserId")))
            {
                throw new ApplicationException($"Không có dữ liệu tài khoản");
            }
            StringHelper.IsNumberOutOfRange(StringHelper.DictGetValue(addOrUpdateRequest.Data, "TotalBenefit"), true);
            StringHelper.IsNumberOutOfRange(StringHelper.DictGetValue(addOrUpdateRequest.Data, "MonthlySalary"), true);
            StringHelper.IsNumberOutOfRange(StringHelper.DictGetValue(addOrUpdateRequest.Data, "TargetSalary"), true);
            StringHelper.IsNumberOutOfRange(StringHelper.DictGetValue(addOrUpdateRequest.Data, "TotalSalary"), true);
        }

        private async Task CheckDuplicateBenefit(Guid applicationUserId, string rolePositionId, Guid userRoleId, List<ApplicationRole> userRoles)
        {
            var getBenefitByApplicationUserId = await _context.Benefits
                         .Where(x => x.ApplicationUserId == applicationUserId
                                   && x.RolePositionId == rolePositionId
                                   && x.DeleteFlag != true)
                         .Select(x => x.ApplicationRoleId)
                         .ToListAsync();

            var checkRoleId = userRoles.Where(x => x.Id == userRoleId);
            if (getBenefitByApplicationUserId.Contains(userRoleId))
                throw new ApplicationException($"Vai trò đã được cấp phát quyền lợi.");

            if (!checkRoleId.Any())
            {
                throw new ApplicationException($"Vai trò không hợp lệ.");
            }
        }
    }
}
