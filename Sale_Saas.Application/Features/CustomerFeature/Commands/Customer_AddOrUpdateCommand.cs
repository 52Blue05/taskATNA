using Sale_Saas.Application.Features.CustomerFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Domain.Constants.API;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.CustomerFeature.Commands
{
    public record Customer_AddOrUpdateCommand(Guid userId,List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<CustomerDto>>>;

    public class Customer_AddOrUpdateCommandHandler : IRequestHandler<Customer_AddOrUpdateCommand, Result<List<CustomerDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;
        private readonly IFeaturePermissionService _permissionService;
        private readonly IEventLogService _eventLogService;

        public Customer_AddOrUpdateCommandHandler(IMapper mapper,
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

        public async Task<Result<List<CustomerDto>>> Handle(Customer_AddOrUpdateCommand request, CancellationToken cancellationToken)
        {
            Customer? obj = null;
            List<CustomerDto> updatedSuccess = new List<CustomerDto>();
            bool? access = null;
            foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
            {
                if (addOrUpdateRequest.Data == null)
                {
                    throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
                }
				if (string.IsNullOrEmpty(StringHelper.DictGetValue(addOrUpdateRequest.Data, "Code")))
				{
					throw new ApplicationException($"Mã dữ liệu không thể để trống");
				}

				StringHelper.IsValidLength(StringInfoConstant.NameLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "Code"), true);
				StringHelper.IsValidLength(StringInfoConstant.NameLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "Fullname"), true);

				if (addOrUpdateRequest.Id == null)
                {
					await _permissionService.HasPermission(
						MenuType.DM_KH,
						FeatureType.CREATE,
					    request.userId,
					    true
				    );

					obj = new Customer()
                    {
                        CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId
                    };
                }
                else
                {
					await _permissionService.HasPermission(
						MenuType.DM_KH,
						FeatureType.UPDATE,
						request.userId,
						true
					);

					obj = await _context.Customers.FindAsync(addOrUpdateRequest.Id.Value);

                    if (obj == null)
                        throw new ApplicationException($"Không tìm thấy Chức vụ có id: {addOrUpdateRequest.Id.Value}");

                }

                obj = (Customer)_internalService.MapValueToObject(new Customer(), addOrUpdateRequest.Data, obj);

                obj.LastModifiedDate = DateTime.Now;
                obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;

				var query = _context.Customers.Where(s => s.DeleteFlag != true && s.Code == obj.Code).AsNoTracking();
				if (addOrUpdateRequest.Id == null)
                {
                    _context.Customers.Add(obj);
                }
                else
                {
					query = query.Where(s => s.Id != obj.Id);
					_context.Customers.Update(obj);
                }
				var duplicate = await query.CountAsync();
				if (duplicate > 0) throw new ApplicationException($"Mã dữ liệu đã được sử dụng {obj.Code}");
				updatedSuccess.Add(_mapper.Map<CustomerDto>(obj));
            }

            var eventLog = await _eventLogService.Create("CustomerFeature", "CustomerFeature",
                                       "Customer_AddOrUpdateCommand", request.userId);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<List<CustomerDto>>.Success(updatedSuccess);
        }
    }
}
