using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.ApplicationUserStatusFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
namespace Sale_Saas.Application.Features.ApplicationUserStatusFeature.Commands;

public record ApplicationUserStatus_AddOrUpdateCommand(Guid userId, List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<ApplicationUserStatusDto>>>;
public class ApplicationUserStatus_AddOrUpdateCommandHandler : IRequestHandler<ApplicationUserStatus_AddOrUpdateCommand, Result<List<ApplicationUserStatusDto>>>
{
	private readonly IApplicationDbContext _context;
	private readonly IMapper _mapper;
    private readonly IInternalService _internalService;
    private readonly IEventLogService _eventLogService;

    public ApplicationUserStatus_AddOrUpdateCommandHandler(IMapper mapper, 
									        IApplicationDbContext context, 
                                            IInternalService internalService,
                                            IEventLogService eventLogService)
    {
		_context = context;
		_mapper = mapper;
        _internalService = internalService;
        _eventLogService = eventLogService;
    }

    public async Task<Result<List<ApplicationUserStatusDto>>> Handle(ApplicationUserStatus_AddOrUpdateCommand request, CancellationToken cancellationToken)
    {
        ApplicationUserStatus? obj = null;
        List<ApplicationUserStatusDto> updatedSuccess = new List<ApplicationUserStatusDto>();

        foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
        {
            if (addOrUpdateRequest.Data == null)
            {
                throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
            }

            if (addOrUpdateRequest.Id == null)
            {
                if (await _context.ApplicationUserStatuses.CountAsync(m =>m.DeleteFlag!=true &&  !string.IsNullOrEmpty(m.Name)
                                                            && m.Name.ToLower().Trim().Equals(addOrUpdateRequest.Data["Name"].ToLower().Trim())) > 0)
                {
                    throw new ApplicationException($"Tên này đã có trong dữ liệu");
                }
                obj = new ApplicationUserStatus()
                {
                    CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId
                };
            }
            else
            {
                obj = await _context.ApplicationUserStatuses.FindAsync(addOrUpdateRequest.Id.Value);

                if (obj == null)
                    throw new ApplicationException($"Không tìm thấy Chức vụ có id: {addOrUpdateRequest.Id.Value}");



            }

            obj = (ApplicationUserStatus)_internalService.MapValueToObject(new ApplicationUserStatus(), addOrUpdateRequest.Data, obj);

            obj.LastModifiedDate = DateTime.Now;
            obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;

            if (addOrUpdateRequest.Id == null)
            {
                _context.ApplicationUserStatuses.Add(obj);
            }
            else
            {
                _context.ApplicationUserStatuses.Update(obj);
            }

    //        LichSuHeThong lichSuHeThong = new LichSuHeThong()
    //        {
    //            Ma = "ApplicationUserStatus",
    //            Ten = "Application User Status",
				//GhiChu = addOrUpdateRequest.Data.ToPairString(),
				//CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId,
    //            LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId
    //        };

    //        if (addOrUpdateRequest.Id == null)
    //        {
    //            _context.ApplicationUserStatuses.Add(obj);

    //            lichSuHeThong.ThaoTac = "Thêm mới";
    //        }
    //        else
    //        {
    //            _context.ApplicationUserStatuses.Update(obj);

    //            lichSuHeThong.ThaoTac = "Cập nhật";
    //        }

            //_context.LichSuHeThongs.Add(lichSuHeThong);
            updatedSuccess.Add(new ApplicationUserStatusDto() { Id = obj.Id });
        }

        var eventLog = await _eventLogService.Create("ApplicationUserStatusFeature", "ApplicationUserStatusFeature",
                                            "ApplicationUserStatus_AddOrUpdateCommand", request.userId);

        await _context.SaveChangesAsync(cancellationToken);

        return  Result<List<ApplicationUserStatusDto>>.Success(updatedSuccess);
    }
}
