using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.MenuFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;

namespace Sale_Saas.Application.Features.MenuFeature.Commands;

public record Menu_AddOrUpdateCommand(List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<MenuDto>>>;
public class Menu_AddCommandHandler : IRequestHandler<Menu_AddOrUpdateCommand, Result<List<MenuDto>>>
{
    
	private readonly IApplicationDbContext _context;
	private readonly IMapper _mapper;
    private readonly IInternalService _internalService;
    public Menu_AddCommandHandler(IMapper mapper, 
									IApplicationDbContext context,
                                    IInternalService internalService)
    {
        
		_context = context;
		_mapper = mapper;
        _internalService = internalService;
    }

    public async Task<Result<List<MenuDto>>> Handle(Menu_AddOrUpdateCommand request, CancellationToken cancellationToken)
    {
        Menu? obj = null;
        List<MenuDto> updatedSuccess = new List<MenuDto>();

        foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
        {
            if (addOrUpdateRequest.Data == null)
            {
                throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
            }

            if (addOrUpdateRequest.Id == null)
            {
                if (await _context.Menus.CountAsync(m => m.DeleteFlag != true
                                                            && !string.IsNullOrEmpty(m.Name )
                                                            && m.Name.ToLower().Trim().Equals(addOrUpdateRequest.Data["Name"].ToLower().Trim())) > 0)
                {
                    throw new ApplicationException($"Tên này đã có trong dữ liệu");
                }
                obj = new Menu()
                {
                    Id=Guid.NewGuid(),
                    CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId
                };
            }
            else
            {
                obj = await _context.Menus.FindAsync(addOrUpdateRequest.Id.Value);

                if (obj == null)
                    throw new ApplicationException($"Không tìm thấy Menu có id: {addOrUpdateRequest.Id.Value}");



            }

            obj = (Menu)_internalService.MapValueToObject(new Menu(), addOrUpdateRequest.Data, obj);

            obj.LastModifiedDate = DateTime.Now;
            obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;

            EventLog eventLog = new EventLog()
            {
                Code = "Menu",
                Name = "Menu",
                Notes = addOrUpdateRequest.Data.ToPairString(),
                CreatedDate = DateTime.Now,
                CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId,
                LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId
            };

            if (addOrUpdateRequest.Id == null)
            {
                _context.Menus.Add(obj);

                eventLog.Action = "Thêm mới";
            }
            else
            {
                _context.Menus.Update(obj);

                eventLog.Action = "Cập nhật";
            }

            _context.EventLogs.Add(eventLog);
            updatedSuccess.Add(new MenuDto() { Id = obj.Id });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<List<MenuDto>>.Success(updatedSuccess);
    }
}
