using Sale_Saas.Application.Features.ServiceFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Domain.Constants.API;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.ServiceFeature.Commands
{
    public record Service_AddOrUpdateCommand(Guid userId, List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<ServiceDto>>>;

    public class Service_AddOrUpdateCommandHandler : IRequestHandler<Service_AddOrUpdateCommand, Result<List<ServiceDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;
        private readonly IFeaturePermissionService _permissionService;

        public Service_AddOrUpdateCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService,
                                                IFeaturePermissionService permissionService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;
            _permissionService = permissionService;
        }

        public async Task<Result<List<ServiceDto>>> Handle(Service_AddOrUpdateCommand request, CancellationToken cancellationToken)
        {
            Service? obj = null;
            List<ServiceDto> updatedSuccess = new List<ServiceDto>();

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
				StringHelper.IsValidLength(StringInfoConstant.NameLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "ShortName"), true);

				if (addOrUpdateRequest.Id == null)
                {
                    await _permissionService.HasPermission(
                                                MenuType.DM_MDV,
                                                FeatureType.CREATE,
                                                request.userId,
                                                true
                                             );

                    obj = new Service()
                    {
                        CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId
                    };
                }
                else
                {
                    await _permissionService.HasPermission(
                            MenuType.DM_MDV,
                            FeatureType.UPDATE,
                            request.userId,
                            true
                         );

                    obj = await _context.Services.FindAsync(addOrUpdateRequest.Id.Value);

                    if (obj == null)
                        throw new ApplicationException($"Không tìm thấy Chức vụ có id: {addOrUpdateRequest.Id.Value}");

                }

                obj = (Service)_internalService.MapValueToObject(new Service(), addOrUpdateRequest.Data, obj);

                obj.LastModifiedDate = DateTime.Now;
                obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;

				var query = _context.Services.Where(s => s.DeleteFlag != true && s.Code == obj.Code).AsNoTracking();
				if (addOrUpdateRequest.Id == null)
                {
                    _context.Services.Add(obj);
                }
                else
                {
					query = query.Where(s => s.Id != obj.Id);
					_context.Services.Update(obj);
                }
				var duplicate = await query.CountAsync();
				if (duplicate > 0) throw new ApplicationException($"Mã dữ liệu đã được sử dụng {obj.Code}");
				updatedSuccess.Add(_mapper.Map<ServiceDto>(obj));
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result<List<ServiceDto>>.Success(updatedSuccess);
        }
    }
}
