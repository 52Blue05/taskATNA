using Sale_Saas.Application.Features.AnswerFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.AnswerFeature.Commands;

public record Answer_AddOrUpdateCommand(Guid? userId, List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<AnswerDto>>>;

public class Answer_AddOrUpdateCommandHandler : IRequestHandler<Answer_AddOrUpdateCommand, Result<List<AnswerDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventLogService _eventLogService;
    private readonly IInternalService _internalService;

    public Answer_AddOrUpdateCommandHandler(IApplicationDbContext context, IEventLogService eventLogService, IInternalService internalService)
    {
        _context = context;
        _eventLogService = eventLogService;
        _internalService = internalService;
    }

    public async Task<Result<List<AnswerDto>>> Handle(Answer_AddOrUpdateCommand request, CancellationToken cancellationToken)
    {
        Answer? obj = null;
        List<AnswerDto> result = new List<AnswerDto>();

        foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
        {
            if (addOrUpdateRequest.Data == null)
            {
                throw new ApplicationException("Không có dữ liệu gửi đến máy chủ");
            }

            if (addOrUpdateRequest.Id == null)
            {
                // add data
                var checkContentDuplicate = await _context.Answers.CountAsync(x => x.DeleteFlag != true
                                                                        && !string.IsNullOrEmpty(x.Content)
                                                                        && x.Content.ToLower().Trim().Equals(addOrUpdateRequest.Data["Content"].ToLower().Trim())
                                                                        && x.QuestionId == Guid.Parse(addOrUpdateRequest.Data["QuestionId"]));

                if (checkContentDuplicate > 0)
                {
                    throw new ApplicationException("Nội dung này đã được sử dụng");
                }

                obj = new Answer()
                {
                    CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId,
                    CreatedDate = DateTime.Now
                };
            }
            else
            {
                // update data
                obj = await _context.Answers.FindAsync(addOrUpdateRequest.Id);

                if (obj == null)
                {
                    throw new ApplicationException("Không tìm thấy câu trả lời để cập nhật");
                }
            }

            obj = (Answer)_internalService.MapValueToObject(new Answer(), addOrUpdateRequest.Data, obj);

            obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;
            obj.LastModifiedDate = DateTime.Now;

            EventLog eventLog = new EventLog()
            {
                Code = "AnswerFeature",
                Name = "AnswerFeature",
                Notes = addOrUpdateRequest.Data.ToPairString(),
                CreatedDate = DateTime.Now,
                CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId,
                LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId,
            };

            if (addOrUpdateRequest.Id == null)
            {
                _context.Answers.Add(obj);
                eventLog.Action = "AnswerFeature_AddCommand";
            }
            else
            {
                _context.Answers.Update(obj);
                eventLog.Action = "AnswerFeature_UpdateCommand";
            }

            _context.EventLogs.Add(eventLog);
            result.Add(new AnswerDto()
            {
                Id = obj.Id
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<List<AnswerDto>>.Success(result);
    }
}
