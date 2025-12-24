using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.QuestionFeature.Commands;

public record Question_DeleteCommand(Guid? userId, DeleteRequest RequestData) : IRequest<Result<string>>;

public class Question_DeleteCommandHandler : IRequestHandler<Question_DeleteCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventLogService _eventLogService;

    public Question_DeleteCommandHandler(IApplicationDbContext context, IEventLogService eventLogService)
    {
        _context = context;
        _eventLogService = eventLogService;
    }

    public async Task<Result<string>> Handle(Question_DeleteCommand request, CancellationToken cancellationToken)
    {
        string result = string.Empty;

        if (request.RequestData.Ids == null)
        {
            throw new ApplicationException("Không tìm thấy câu hỏi để xoá");
        }

        List<Guid> ids = request.RequestData.Ids.Select(m => Guid.Parse(m)).ToList();

        var query = await _context.Questions.Where(x => x.DeleteFlag != true && ids.Contains(x.Id))
                                           .ToListAsync();

        if (query == null)
        {
            throw new ApplicationException($"Không tìm thấy trong dữ liệu có Id: {string.Join(";", request.RequestData.Ids)}");
        }

        foreach (var item in query)
        {
            item.DeleteFlag = true;
            item.LastModifiedDate = DateTime.Now;
            item.LastModifiedApplicationUserId = request.RequestData.ApplicationUserId;

            await _eventLogService.Create("QuestionFeature", "QuestionFeature", "Question_DeleteCommand", request.RequestData.ApplicationUserId);
        }

        _context.Questions.UpdateRange(query);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(result);
    }
}
