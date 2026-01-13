
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.ApplicationUserStatusFeature.Commands;

public record ApplicationUserStatus_DeleteByIdCommand(Guid userId, DeleteRequest RequestData) : IRequest<Result<string>>;
public class ApplicationUserStatus_DeleteByIdCommandHandler : IRequestHandler<ApplicationUserStatus_DeleteByIdCommand, Result<string>>
{
    
	private readonly IApplicationDbContext _context;
	private readonly IMapper _mapper; 
    private readonly IEventLogService _eventLogService;

	public ApplicationUserStatus_DeleteByIdCommandHandler(IMapper mapper,
										   IApplicationDbContext context, IEventLogService eventLogService)
    {
		_context = context;
        _mapper = mapper;
        _eventLogService = eventLogService;
	}

    public async Task<Result<string>> Handle(ApplicationUserStatus_DeleteByIdCommand request, CancellationToken cancellationToken)
    {
        string result = string.Empty;

        if (request.RequestData.Ids == null) throw new ApplicationException("Không tìm thấy tham số Id.");
        List<Guid> ids = request.RequestData.Ids.Select(m => Guid.Parse(m)).ToList();
        var query = await _context.ApplicationUserStatuses.Where(m => ids.Contains(m.Id)).ToListAsync();
        if (query == null || query.Count == 0) throw new ApplicationException($"Không tìm thấy trong dữ liệu có Id: {string.Join(";", request.RequestData.Ids)}");

        foreach(var item in query)
        {
            //LichSuHeThong lichSuHeThong = new LichSuHeThong()
            //{
            //    Ma = "ApplicationUserStatus",
            //    Ten = "Application User Status",
            //    ThaoTac = "Xóa",
            //    GhiChu = "Id: " + item.Id.ToString() + ", Tên: " + item.Ten,
            //    CreatedApplicationUserId = request.RequestData.ApplicationUserId,
            //    LastModifiedApplicationUserId = request.RequestData.ApplicationUserId
            //};
            //_context.LichSuHeThongs.Add(lichSuHeThong);
        }        

        _context.ApplicationUserStatuses.RemoveRange(query);

        var eventLog = await _eventLogService.Create("ApplicationUserStatusFeature", "ApplicationUserStatusFeature",
                                            "ApplicationUserStatus_DeleteByIdCommand", request.userId);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(string.Empty);
    }
}
