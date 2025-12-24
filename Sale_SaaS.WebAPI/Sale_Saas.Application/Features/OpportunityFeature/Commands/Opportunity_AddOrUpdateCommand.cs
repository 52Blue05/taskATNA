using Sale_Saas.Application.Features.OpportunityFeature.Dto;
using Sale_Saas.Application.Features.OpportunityFeature.Services;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Domain.Constants.API;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.OpportunityFeature.Commands
{
    public record Opportunity_AddOrUpdateCommand(Guid UserId, List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<OpportunityDto>>>;

    public class Opportunity_AddOrUpdateCommandHandler : IRequestHandler<Opportunity_AddOrUpdateCommand, Result<List<OpportunityDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;
        private readonly IFeaturePermissionService _permissionService;
        private readonly IApplicationRoleService _roleService;

        public Opportunity_AddOrUpdateCommandHandler(IMapper mapper,
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

        public async Task<Result<List<OpportunityDto>>> Handle(Opportunity_AddOrUpdateCommand request, CancellationToken cancellationToken)
        {
            Opportunity? obj = null;
            List<OpportunityDto> updatedSuccess = new List<OpportunityDto>();
            Opportunity tmp = new Opportunity();
            string currentRole = await _roleService.GetCurrentRoleOfUser(request.UserId);
            foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
            {
                if (currentRole == RolePositionEnum.MANAGER.ToString())
                {
                    addOrUpdateRequest.CreatedApplicationUserId = request.UserId;
                }
                addOrUpdateRequest.LastModifiedApplicationUserId = request.UserId;
                if (addOrUpdateRequest.Data == null)
                {
                    throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
                }
                Validate(addOrUpdateRequest);

                if (addOrUpdateRequest.Id == null)
                {
                    await _permissionService.HasPermission(MenuType.Sale_CH, FeatureType.CREATE, request.UserId, true);
                    obj = new Opportunity()
                    {
                        CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId,
                        ApplicationUserId = request.UserId
                    };
                }
                else
                {
                    await _permissionService.HasPermission(MenuType.Sale_CH, FeatureType.UPDATE, request.UserId, true);
                    obj = await OpportunityService.GetOpportunity(addOrUpdateRequest.Id.Value, _context);
                    PropertiesExtension.Copy(obj, tmp);
                }

                obj = (Opportunity)_internalService.MapValueToObject(new Opportunity(), addOrUpdateRequest.Data, obj);
                obj.LastModifiedDate = DateTime.Now;
                obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;
                obj.ApplicationUserId = tmp.ApplicationUserId ?? obj.ApplicationUserId;
                obj.ApplicationUser = tmp.ApplicationUser ?? obj.ApplicationUser;
                obj.OpportunityStatusId = tmp.OpportunityStatusId ?? obj.OpportunityStatusId;
                obj.OpportunityStatus = tmp.OpportunityStatus ?? obj.OpportunityStatus;
                obj.CustomerId = tmp.CustomerId ?? obj.CustomerId;
                obj.Customer = tmp.Customer ?? obj.Customer;
                obj.Reason = "";

                if (addOrUpdateRequest.Id == null)
                {
                    var status = await OpportunityService.GetStatus(OpportunityStatusEnum.PENDING.ToString(), _context);
                    obj.OpportunityStatusId = status.Id;
                    obj.OpportunityStatus = status;

                    obj.Customer = await OpportunityService.GetCustomer(obj.CustomerId ?? Guid.Empty, _context);
                    obj.ApplicationUser = await OpportunityService.GetApplicationUser(obj.ApplicationUserId ?? Guid.Empty, _context);

                    _context.Opportunities.Add(obj);
                }
                else
                {
                    obj.Customer = await OpportunityService.GetCustomer(obj.CustomerId ?? Guid.Empty, _context);
                    _context.Opportunities.Update(obj);
                }

                updatedSuccess.Add(_mapper.Map<OpportunityDto>(obj));
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result<List<OpportunityDto>>.Success(updatedSuccess);
        }

        private void Validate(AddOrUpdateRequest addOrUpdateRequest)
        {
            StringHelper.IsValidLength(StringInfoConstant.NameLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "CustomerName"), true);
            StringHelper.IsValidLength(StringInfoConstant.NameLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "Accountable"), true);
            StringHelper.IsValidLength(StringInfoConstant.NameLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "TechnicalLead"), true);
            StringHelper.IsValidLength(StringInfoConstant.NameLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "Beneficiary"), true);
            StringHelper.IsValidLength(StringInfoConstant.DesLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "Reason"), true);
            StringHelper.IsValidLength(StringInfoConstant.DesLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "Need"), true);
            StringHelper.IsValidLength(StringInfoConstant.DesLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "Strategy"), true);
            StringHelper.IsValidLength(StringInfoConstant.DesLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "WinningOppotunity"), true);

            StringHelper.IsNumberOutOfRange(StringHelper.DictGetValue(addOrUpdateRequest.Data, "EstimatedMoney"), true);
            StringHelper.IsNumberOutOfRange(StringHelper.DictGetValue(addOrUpdateRequest.Data, "CommissionMoney"), true);
        }
    }
}
