using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.OpportunityFeature.Queries;

public record OpportunityHistoryMobile_DeleteCommand(Guid? userId, DeleteRequest RequestData) : IRequest<Result<string>>;

public class OpportunityHistoryMobile_DeleteCommandHandler : IRequestHandler<OpportunityHistoryMobile_DeleteCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventLogService _eventLogService;

    public OpportunityHistoryMobile_DeleteCommandHandler(IApplicationDbContext context, IEventLogService eventLogService)
    {
        _context = context;
        _eventLogService = eventLogService;
    }

    public async Task<Result<string>> Handle(OpportunityHistoryMobile_DeleteCommand request, CancellationToken cancellationToken)
    {
        string result = string.Empty;

        if (request.RequestData.Ids == null)
        {
            throw new ApplicationException("Không tìm thấy câu trả lời để xoá");
        }

        List<Guid> ids = request.RequestData.Ids.Select(m => Guid.Parse(m)).ToList();

        var query = await _context.OpportunityHistories.Where(x => x.DeleteFlag != true && ids.Contains(x.Id))
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

            await _eventLogService.Create("AnswerFeature", "AnswerFeature", "OpportunityHistoryMobile_DeleteCommand", request.RequestData.ApplicationUserId);
        }

        _context.OpportunityHistories.UpdateRange(query);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(result);
    }
}
