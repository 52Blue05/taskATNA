using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.MailServerInfoFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.MailServerInfoFeature.Commands;

public record MailServerInfo_AddOrUpdateCommand(Guid userId, List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<MailServerInfoDto>>>;
public class MailServerInfo_AddOrUpdateCommandHandler : IRequestHandler<MailServerInfo_AddOrUpdateCommand, Result<List<MailServerInfoDto>>>
{
    
	private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper; 
    private readonly IInternalService _internalService;
    private readonly IEventLogService _eventLogService;

    public MailServerInfo_AddOrUpdateCommandHandler(IMapper mapper, 
											IApplicationDbContext context,
                                            IInternalService internalService, IEventLogService eventLogService)
    {
        
		_context = context;
        _mapper = mapper;
        _internalService = internalService;
        _eventLogService = eventLogService;
    }

    public async Task<Result<List<MailServerInfoDto>>> Handle(MailServerInfo_AddOrUpdateCommand request, CancellationToken cancellationToken)
    {
        MailServerInfo? obj = null;
        List<MailServerInfoDto> updatedSuccess = new List<MailServerInfoDto>();

        foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
        {
            if (addOrUpdateRequest.Data == null)
            {
                throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
            }

            if (addOrUpdateRequest.Data.ContainsKey("TenMailServerInfoGui"))
            {
                throw new ApplicationException($"không có tham số TenMailServerInfoGui.");
            }    

            if (addOrUpdateRequest.Id == null)
            {
                if (await _context.MailServerInfos.CountAsync(m => m.DeleteFlag != true
                                                            && !string.IsNullOrEmpty(m.TenEmailGui)
                                                            && m.TenEmailGui.ToLower().Trim().Equals(addOrUpdateRequest.Data["TenEmailGui"].ToLower().Trim())) > 0)
                {
                    throw new ApplicationException($"Tên này đã có trong dữ liệu");
                }
                obj = new MailServerInfo()
                {
                    CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId
                };
            }
            else
            {
                obj = await _context.MailServerInfos.FindAsync(addOrUpdateRequest.Id.Value);

                if (obj == null)
                    throw new ApplicationException($"Không tìm thấy MailServerInfo có id: {addOrUpdateRequest.Id.Value}");



            }

            obj = (MailServerInfo)_internalService.MapValueToObject(new MailServerInfo(), addOrUpdateRequest.Data, obj);

            obj.LastModifiedDate = DateTime.Now;
            obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;

            //LichSuHeThong lichSuHeThong = new LichSuHeThong()
            //{
            //    Ma = "MailServerInfo",
            //    Ten = "MailServerInfo",
            //    GhiChu = "Tên email gửi: " + addOrUpdateRequest.Data["TenMailServerInfoGui"],
            //    CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId,
            //    LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId
            //};

            //if (addOrUpdateRequest.Id == null)
            //{
            //    _context.MailServerInfos.Add(obj);

            //    lichSuHeThong.ThaoTac = "Thêm mới";
            //}
            //else
            //{
            //    _context.MailServerInfos.Update(obj);

            //    lichSuHeThong.ThaoTac = "Cập nhật";
            //}

           // _context.LichSuHeThongs.Add(lichSuHeThong);
            updatedSuccess.Add(new MailServerInfoDto() { Id = obj.Id });
        }

        await _context.SaveChangesAsync(cancellationToken);

        var eventLog = await _eventLogService.Create("MailServerInfoFeature", "MailServerInfoFeature",
                                    "MailServerInfo_AddOrUpdateCommand", request.userId);

        return Result<List<MailServerInfoDto>>.Success(updatedSuccess);
    }
}
