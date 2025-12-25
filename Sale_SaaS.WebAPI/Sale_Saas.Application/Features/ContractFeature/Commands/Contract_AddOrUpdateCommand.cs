using Sale_Saas.Application.Features.ContractFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Domain.Constants.API;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.ContractFeature.Commands
{
    public record Contract_AddOrUpdateCommand(Guid userId, List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<ContractDto>>>;

    public class Contract_AddOrUpdateCommandHandler : IRequestHandler<Contract_AddOrUpdateCommand, Result<List<ContractDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;
        private readonly IFeaturePermissionService _permissionService;
        private readonly IEventLogService _eventLogService;

        public Contract_AddOrUpdateCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService,
												IFeaturePermissionService permissionService,
                                                IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;
            _permissionService = permissionService;
            _eventLogService = eventLogService;
        }

        public async Task<Result<List<ContractDto>>> Handle(Contract_AddOrUpdateCommand request, CancellationToken cancellationToken)
        {
            Contract? obj = null;
            List<ContractDto> updatedSuccess = new List<ContractDto>();
            
            foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
            {
                var tmp = new Contract();
                if (addOrUpdateRequest.Data == null)
                {
                    throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
                }
                if(string.IsNullOrEmpty(StringHelper.DictGetValue(addOrUpdateRequest.Data, "Code")))
                {
					throw new ApplicationException($"Mã dữ liệu không thể để trống");
				}

				StringHelper.IsValidLength(StringInfoConstant.NameLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "Code"), true);
				StringHelper.IsValidLength(StringInfoConstant.NameLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "Number"), true);
				StringHelper.IsValidLength(StringInfoConstant.NameLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "Name"), true);

				if (addOrUpdateRequest.Id == null)
                {
					await _permissionService.HasPermission(
					    MenuType.DM_HD,
					    FeatureType.CREATE,
					    request.userId,
					    true
				    );

					obj = new Contract()
                    {
                        CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId
                    };
                }
                else
                {
					await _permissionService.HasPermission(
						MenuType.DM_HD,
						FeatureType.UPDATE,
						request.userId,
						true
					);

					obj = await _context.Contracts.Include(s => s.ContractStatus)
                                                  .Include(s => s.Customer)
                                                  .FirstOrDefaultAsync(s => s.Id == addOrUpdateRequest.Id.Value);

                    if (obj == null)
                        throw new ApplicationException($"Không tìm thấy Hợp đồng có id: {addOrUpdateRequest.Id.Value}");

                    PropertiesExtension.Copy(obj, tmp);
                }

                obj = (Contract)_internalService.MapValueToObject(new Contract(), addOrUpdateRequest.Data, obj);
                obj.LastModifiedDate = DateTime.Now;
                obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;
                //obj.CustomerId = tmp.CustomerId ?? obj.CustomerId;
                obj.ContractStatusId = tmp.ContractStatusId ?? obj.ContractStatusId;

                if (obj.ContractStatusId != null)
                {
                    var exist = await _context.ContractStatuses.FindAsync(obj.ContractStatusId);
                    if (exist == null) throw new ApplicationException($"Không tìm thấy Trạng thái có id: {obj.ContractStatusId}");
                }
                if (obj.CustomerId != null)
                {
                    var exist = await _context.Customers.FindAsync(obj.CustomerId);
                    if (exist == null) throw new ApplicationException($"Không tìm thấy Khách hàng có id: {obj.CustomerId}");
                    obj.Customer = exist;
                    obj.CustomerId = exist.Id;
                }

                
                if (obj.StartDate != null && obj.EndDate != null)
                {
                    DateTime now = DateTime.Now;
                    ContractStatus? status = null;
                    if(obj.StartDate <= now && obj.EndDate >= now)
                    {
                        status = await _context.ContractStatuses.FirstOrDefaultAsync(s => s.Code == ContractStatusEnum.PROCESSING.ToString());
                    }
                    if (now > obj.EndDate)
                    {
                        status = await _context.ContractStatuses.FirstOrDefaultAsync(s => s.Code == ContractStatusEnum.COMPLETED.ToString());
                    }
                    if(status != null)
                    {
                        obj.ContractStatus = status;
                        obj.ContractStatusId = status.Id;
                    }
                }

                var query = _context.Contracts.Where(s => s.DeleteFlag != true && s.Code == obj.Code).AsNoTracking();
                if (addOrUpdateRequest.Id == null)
                {
					_context.Contracts.Add(obj);
                }
                else
                {
					query = query.Where(s => s.Id != obj.Id);
                    _context.Contracts.Update(obj);
                }
                var duplicate = await query.CountAsync();
                if (duplicate > 0) throw new ApplicationException($"Mã dữ liệu đã được sử dụng {obj.Code}");

                updatedSuccess.Add(_mapper.Map<ContractDto>(obj));
            }

            var eventLog = await _eventLogService.Create("ContractFeature", "ContractFeature",
                                                               "Contract_AddOrUpdateCommand", request.userId);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<List<ContractDto>>.Success(updatedSuccess);
        }
    }
}
