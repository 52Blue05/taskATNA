using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.UnitQuestionFeature.Commands;

public record UnitQuestion_DeleteCommand(Guid? userId, DeleteRequest RequestData) : IRequest<Result<string>>;

public class UnitQuestion_DeleteCommandHandler : IRequestHandler<UnitQuestion_DeleteCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventLogService _eventLogService;

    public UnitQuestion_DeleteCommandHandler(IApplicationDbContext context, IEventLogService eventLogService)
    {
        _context = context;
        _eventLogService = eventLogService;
    }

    public async Task<Result<string>> Handle(UnitQuestion_DeleteCommand request, CancellationToken cancellationToken)
    {
        string result = string.Empty;

        if (request.RequestData.Ids == null)
        {
            throw new ApplicationException("Không tìm thấy unit question để xoá");
        }

        List<Guid> ids = request.RequestData.Ids.Select(m => Guid.Parse(m)).ToList();

        var query = await _context.UnitQuestions.Where(x => x.DeleteFlag != true && ids.Contains(x.Id))
                                                .ToListAsync();

        if (query == null)
        {
            throw new ApplicationException($"Không tìm thấy trong dữ liệu có Id: {string.Join(";", request.RequestData.Ids)}");
        }

        foreach (var item in query)
        {
            item.DeleteFlag = true;
            item.LastModifiedApplicationUserId = request.RequestData.ApplicationUserId;
            item.LastModifiedDate = DateTime.Now;

            await _eventLogService.Create("UnitQuestionFeature", "UnitQuestionFeature", "UnitQuestion_DeleteCommand", request.RequestData.ApplicationUserId);
        }

        _context.UnitQuestions.UpdateRange(query);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(result);
    }
}