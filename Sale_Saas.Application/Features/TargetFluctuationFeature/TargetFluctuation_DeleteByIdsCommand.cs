using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.TargetFluctuationFeature;

public record TargetFluctuation_DeleteByIdsCommand(Guid userId, DeleteRequest RequestData) : IRequest<Result<string>>;

public class TargetFluctuation_DeleteByIdsCommandHandler : IRequestHandler<TargetFluctuation_DeleteByIdsCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventLogService _eventLogService;

    public TargetFluctuation_DeleteByIdsCommandHandler(IApplicationDbContext context, IEventLogService eventLogService)
    {
        _context = context;
        _eventLogService = eventLogService;
    }

    public async Task<Result<string>> Handle(TargetFluctuation_DeleteByIdsCommand request, CancellationToken cancellationToken)
    {
        string result = string.Empty;

        if (request.RequestData.Ids == null)
        {
            throw new ApplicationException("Không tìm thấy câu mục tiêu để xoá khỏi biến động");
        }

        List<Guid> ids = request.RequestData.Ids.Select(m => Guid.Parse(m)).ToList();

        var query = await _context.TargetFluctuations.Where(x => x.DeleteFlag != true && ids.Contains(x.Id))
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

            await _eventLogService.Create("TargetFluctuationFeature", "TargetFluctuationFeature", "TargetFluctuation_DeleteByIdsCommand", request.RequestData.ApplicationUserId);
        }

        _context.TargetFluctuations.UpdateRange(query);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(result);
    }
}
