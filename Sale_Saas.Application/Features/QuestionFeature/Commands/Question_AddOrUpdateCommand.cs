using Sale_Saas.Application.Features.QuestionFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.QuestionFeature.Commands;

public record Question_AddOrUpdateCommand(Guid? userId, List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<QuestionDto>>>;

public class Question_AddOrUpdateCommandHandler : IRequestHandler<Question_AddOrUpdateCommand, Result<List<QuestionDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventLogService _eventLogService;
    private readonly IInternalService _internalService;

    public Question_AddOrUpdateCommandHandler(IApplicationDbContext context, IEventLogService eventLogService, IInternalService internalService)
    {
        _context = context;
        _eventLogService = eventLogService;
        _internalService = internalService;
    }

    public async Task<Result<List<QuestionDto>>> Handle(Question_AddOrUpdateCommand request, CancellationToken cancellationToken)
    {
        Question? obj = null;
        List<QuestionDto> result = new List<QuestionDto>();

        foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
        {
            if (addOrUpdateRequest.Data == null)
            {
                throw new ApplicationException("Không có dữ liệu gửi đến máy chủ");
            }

            if (addOrUpdateRequest.Id == null)
            {
                // add data
                //var checkNameDuplicate = await _context.Questions.CountAsync(x => x.DeleteFlag != true
                //                                                        && !string.IsNullOrEmpty(x.Name)
                //                                                        && x.Name.ToLower().Trim().Equals(addOrUpdateRequest.Data["Name"].ToLower().Trim()));

                //if (checkNameDuplicate > 0)
                //{
                //    throw new ApplicationException("Tên này đã được sử dụng");
                //}

                obj = new Question()
                {
                    CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId,
                    CreatedDate = DateTime.Now
                };
            }
            else
            {
                // update data
                obj = await _context.Questions.FindAsync(addOrUpdateRequest.Id);

                if (obj == null)
                {
                    throw new ApplicationException("Không tìm thấy câu hỏi để cập nhật");
                }
            }

            obj = (Question)_internalService.MapValueToObject(new Question(), addOrUpdateRequest.Data, obj);

            obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;
            obj.LastModifiedDate = DateTime.Now;

            //EventLog eventLog = new EventLog()
            //{
            //    Code = "QuestionFeature",
            //    Name = "QuestionFeature",
            //    Notes = addOrUpdateRequest.Data.ToPairString(),
            //    CreatedDate = DateTime.Now,
            //    CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId,
            //    LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId,
            //};

            if (addOrUpdateRequest.Id == null)
            {
                _context.Questions.Add(obj);
            }
            else
            {
                _context.Questions.Update(obj);
            }

            await _eventLogService.Create("QuestionFeature", "QuestionFeature", "QuestionFeature_AddorUpdateCommand", addOrUpdateRequest.CreatedApplicationUserId);

            result.Add(new QuestionDto()
            {
                Id = obj.Id
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<List<QuestionDto>>.Success(result);
    }
}