using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.LessionFeature.Commands;

public record Lession_DeleteCommand(Guid UserId, DeleteRequest RequestData) : IRequest<Result<string>>;

public class Lession_DeleteCommandHandler : IRequestHandler<Lession_DeleteCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventLogService _eventLogService;
    private readonly IFileStorageService _storageService;

    public Lession_DeleteCommandHandler(IApplicationDbContext context, IEventLogService eventLogService, IFileStorageService storageService)
    {
        _context = context;
        _eventLogService = eventLogService;
        _storageService = storageService;
    }

    public async Task<Result<string>> Handle(Lession_DeleteCommand request, CancellationToken cancellationToken)
    {
        string result = string.Empty;

        if (request.RequestData.Ids == null)
        {
            throw new ApplicationException("Không tìm thấy bài học để xoá");
        }

        List<Guid> ids = request.RequestData.Ids.Select(m => Guid.Parse(m)).ToList();

        var query = await _context.Lessions.Where(x => x.DeleteFlag != true && ids.Contains(x.Id))
                                           .ToListAsync();

        if (query == null || query.Count == 0)
        {
            throw new ApplicationException($"Không tìm thấy trong dữ liệu có Id: {string.Join(";", request.RequestData.Ids)}");
        }

        Guid unitId = Guid.Empty;

        foreach (var item in query)
        {
            item.DeleteFlag = true;
            item.LastModifiedDate = DateTime.Now;
            item.LastModifiedApplicationUserId = request.RequestData.ApplicationUserId;
            unitId = item.UnitId;


            if (!string.IsNullOrEmpty(item.FileName))
            {
                await _storageService.DeleteMediaAsync(item.FileName ?? string.Empty);
            }
        }

        _context.Lessions.UpdateRange(query);

        await _context.SaveChangesAsync(cancellationToken);

        // handle re-sort
        var listLessons = await _context.Lessions.Where(x => x.DeleteFlag != true && x.UnitId == unitId)
                                                           .OrderBy(x => x.SortOrder)
                                                           .ToListAsync();

        const int INITIAL_SORT_ORDER = 1;
        var index = INITIAL_SORT_ORDER;

        foreach (var item in listLessons)
        {
            item.SortOrder = index;
            index = index + 1;
        }

        _context.Lessions.UpdateRange(listLessons);

        await _eventLogService.Create("LessionFeature", "LessionFeature", "Lession_DeleteCommand", request.UserId);

        return Result<string>.Success(result);
    }
}


