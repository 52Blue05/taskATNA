using Sale_Saas.Application.Features.SupplierFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Domain.Constants.API;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.SupplierFeature.Commands
{
    public record Supplier_AddOrUpdateCommand(Guid userId, List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<SupplierDto>>>;

    public class Supplier_AddOrUpdateCommandHandler : IRequestHandler<Supplier_AddOrUpdateCommand, Result<List<SupplierDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;
        private readonly IFeaturePermissionService _permissionService;

        public Supplier_AddOrUpdateCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService,
                                                IFeaturePermissionService permissionService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;
            _permissionService = permissionService;
        }

        public async Task<Result<List<SupplierDto>>> Handle(Supplier_AddOrUpdateCommand request, CancellationToken cancellationToken)
        {
            Supplier? obj = null;
            List<SupplierDto> updatedSuccess = new List<SupplierDto>();

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
				StringHelper.IsValidLength(StringInfoConstant.NameLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "Name"), true);
				StringHelper.IsValidLength(StringInfoConstant.DesLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "Description"), true);
				StringHelper.IsValidLength(StringInfoConstant.DesLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "Review"), true);


				if (addOrUpdateRequest.Id == null)
                {
                    await _permissionService.HasPermission(
                            MenuType.DM_NCC,
                            FeatureType.CREATE,
                            request.userId,
                            true
                    );

                    obj = new Supplier()
                    {
                        CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId
                    };
                }
                else
                {
                    await _permissionService.HasPermission(
                        MenuType.DM_NCC,
                        FeatureType.UPDATE,
                        request.userId,
                        true
                    );

                    obj = await _context.Suppliers.FindAsync(addOrUpdateRequest.Id.Value);

                    if (obj == null)
                        throw new ApplicationException($"Không tìm thấy Chức vụ có id: {addOrUpdateRequest.Id.Value}");

                }

                obj = (Supplier)_internalService.MapValueToObject(new Supplier(), addOrUpdateRequest.Data, obj);
                obj.LastModifiedDate = DateTime.Now;
                obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;

				var query = _context.Suppliers.Where(s => s.DeleteFlag != true && s.Code == obj.Code).AsNoTracking();
				if (addOrUpdateRequest.Id == null)
                {
                    _context.Suppliers.Add(obj);
                }
                else
                {
					query = query.Where(s => s.Id != obj.Id);
					_context.Suppliers.Update(obj);
                }
				var duplicate = await query.CountAsync();
				if (duplicate > 0) throw new ApplicationException($"Mã dữ liệu đã được sử dụng {obj.Code}");

				updatedSuccess.Add(_mapper.Map<SupplierDto>(obj));
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result<List<SupplierDto>>.Success(updatedSuccess);
        }
    }
}
