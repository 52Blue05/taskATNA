using Sale_Saas.Application.Features.BenefitFeature.Dto;
using Sale_Saas.Application.Features.BenefitFeature.Services;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.BenefitFeature.Commands
{
     public record Benefit_AddOrUpdateCommand(Guid userId, List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<BenefitDto>>>;

     public class Benefit_AddOrUpdateCommandHandler : IRequestHandler<Benefit_AddOrUpdateCommand, Result<List<BenefitDto>>>
     {
          private readonly IApplicationDbContext _context;
          private readonly IMapper _mapper;
          private readonly IInternalService _internalService;
          private readonly IApplicationUserService _applicationUserService;
          private readonly IFeaturePermissionService _PermissionService;
          private readonly IEventLogService _eventLogService;

          public Benefit_AddOrUpdateCommandHandler(IMapper mapper,
                                                  IApplicationDbContext context,
                                                              IApplicationUserService applicationUserService,
                                                  IInternalService internalService,
                                                              IFeaturePermissionService permissionService,
                                                  IEventLogService eventLogService)
          {
               _context = context;
               _mapper = mapper;
               _internalService = internalService;
               _applicationUserService = applicationUserService;
               _PermissionService = permissionService;
               _eventLogService = eventLogService;
          }

          public async Task<Result<List<BenefitDto>>> Handle(Benefit_AddOrUpdateCommand request, CancellationToken cancellationToken)
          {
               Benefit? obj = null;
               List<BenefitDto> updatedSuccess = new List<BenefitDto>();

               foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
               {
                    Benefit tmp = new Benefit();
                    Validate(addOrUpdateRequest);

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
                       "Benefit_AddOrUpdateCommand", request.userId);

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
     }
}
