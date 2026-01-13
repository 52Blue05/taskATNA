using Sale_Saas.Application.Features.UnitQuestionFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.UnitQuestionFeature.Commands;

public record UnitQuestion_AddOrUpdateCommand(Guid? userId, List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<UnitQuestionDto>>>;

public class UnitQuestion_AddOrUpdateCommandHandler : IRequestHandler<UnitQuestion_AddOrUpdateCommand, Result<List<UnitQuestionDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventLogService _eventLogService;
    private readonly IInternalService _internalService;

    public UnitQuestion_AddOrUpdateCommandHandler(IApplicationDbContext context, IEventLogService eventLogService, IInternalService internalService)
    {
        _context = context;
        _eventLogService = eventLogService;
        _internalService = internalService;
    }

    public async Task<Result<List<UnitQuestionDto>>> Handle(UnitQuestion_AddOrUpdateCommand request, CancellationToken cancellationToken)
    {
        UnitQuestions? obj = null;
        List<UnitQuestionDto> result = new List<UnitQuestionDto>();

        foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
        {
            if (addOrUpdateRequest.Data == null)
            {
                throw new ApplicationException("Không có dữ liệu gửi đến máy chủ");
            }

            if (addOrUpdateRequest.Id == null)
            {
                // add data
                obj = new UnitQuestions()
                {
                    DeleteFlag = false,
                    CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId,
                    CreatedDate = DateTime.Now
                };
            }
            else
            {
                // update data
                obj = await _context.UnitQuestions.FindAsync(addOrUpdateRequest.Id);

                if (obj == null)
                {
                    throw new ApplicationException("Không tìm thấy unit question để cập nhật");
                }
            }

            obj = (UnitQuestions)_internalService.MapValueToObject(new UnitQuestions(), addOrUpdateRequest.Data, obj);

            obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;
            obj.LastModifiedDate = DateTime.Now;

            EventLog eventLog = new EventLog()
            {
                Code = "UnitQuestionFeature",
                Name = "UnitQuestionFeature",
                Notes = addOrUpdateRequest.Data.ToPairString(),
                CreatedDate = DateTime.Now,
                CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId,
                LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId,
            };

            if (addOrUpdateRequest.Id == null)
            {
                _context.UnitQuestions.Add(obj);
                eventLog.Action = "UnitQuestionFeature_AddCommand";
            }
            else
            {
                _context.UnitQuestions.Update(obj);
                eventLog.Action = "UnitQuestionFeature_UpdateCommand";
            }

            _context.EventLogs.Add(eventLog);
            result.Add(new UnitQuestionDto()
            {
                Id = obj.Id
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<List<UnitQuestionDto>>.Success(result);
    }

}