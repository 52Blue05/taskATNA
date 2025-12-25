using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Domain.Entities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Sale_Saas.Application.Features.ApplicationUserStatusFeature.Commands;

public record ApplicationUserStatus_ImportingDataCommand(Guid userId, ImportingDataRequest RequestData) : IRequest<Result<string>>;

public class ApplicationUserStatus_ImportingDataCommandHandler : IRequestHandler<ApplicationUserStatus_ImportingDataCommand, Result<string>>
{
	private readonly IApplicationDbContext _context;
	private readonly IMapper _mapper; 
    private readonly IEventLogService _eventLogService;

	public ApplicationUserStatus_ImportingDataCommandHandler(IMapper mapper,
											                IApplicationDbContext context, IEventLogService eventLogService)
    {        
		_context = context;
		_mapper = mapper;
        _eventLogService = eventLogService;
	}

    public async Task<Result<string>> Handle(ApplicationUserStatus_ImportingDataCommand request, CancellationToken cancellationToken)
    {
        string resultMessage = string.Empty;

        if (request.RequestData.Dt.Rows.Count > 0)
        {
            //LichSuHeThong lichSuHeThong = new LichSuHeThong()
            //{
            //    Ma = "ApplicationUserStatus",
            //    Ten = "Application User Status",
            //    ThaoTac = "Import data",
            //    CreatedApplicationUserId = request.RequestData.CreatedApplicationUserId,
            //    LastModifiedApplicationUserId = request.RequestData.CreatedApplicationUserId
            //};
            //_context.LichSuHeThongs.Add(lichSuHeThong);
            await _context.SaveChangesAsync(cancellationToken);

            int rowIndex = 0;
            while (rowIndex < request.RequestData.Dt.Rows.Count)
            {
                string updateStatus = request.RequestData.Dt.Rows[rowIndex][0].ToString() ?? string.Empty;
                string name = request.RequestData.Dt.Rows[rowIndex][1].ToString() ?? string.Empty;

                if (updateStatus.ToLower() != "y")
                {
                    if (!string.IsNullOrEmpty(name))
                    {
                        ApplicationUserStatus? applicationUserStatus = await _context.ApplicationUserStatuses.FirstOrDefaultAsync(m => m.DeleteFlag != true
                                                                                        && !string.IsNullOrEmpty(m.Name)
                                                                                        && m.Name.ToLower().Trim().Equals(name.Trim()));

                        if (applicationUserStatus == null)
                        {
                            await _context.ApplicationUserStatuses.AddAsync(new ApplicationUserStatus()
                            {
                                Name = name.Trim() ?? string.Empty,
                                CreatedApplicationUserId = request.RequestData.CreatedApplicationUserId,
                                LastModifiedApplicationUserId = request.RequestData.CreatedApplicationUserId
                            });
                        }
                        else
                        {
                            applicationUserStatus.Name = name.Trim();
                            applicationUserStatus.LastModifiedApplicationUserId = request.RequestData.CreatedApplicationUserId;
                            _context.ApplicationUserStatuses.Update(applicationUserStatus);
                        }
                    }
                }
                rowIndex++;
            }

            var eventLog = await _eventLogService.Create("ApplicationUserStatusFeature", "ApplicationUserStatusFeature",
                                            "ApplicationUserStatus_ImportingDataCommand", request.userId);

            await _context.SaveChangesAsync(cancellationToken);
        }
        return Result<string>.Success(resultMessage);
    }
}
