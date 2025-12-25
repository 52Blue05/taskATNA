using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.CriteriaFeature.Commands;

public record Criteria_DeleteCommand(Guid UserId, DeleteRequest RequestData) : IRequest<Result<bool>>;

public class Criteria_DeleteCommandHandler : IRequestHandler<Criteria_DeleteCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventLogService _eventLogService;

    public Criteria_DeleteCommandHandler(IApplicationDbContext context, IEventLogService eventLogService)
    {
        _context = context;
        _eventLogService = eventLogService;
    }

    public async Task<Result<bool>> Handle(Criteria_DeleteCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy người dùng");
        }

        if (request.RequestData.Ids == null)
        {
            throw new ApplicationException("Không tìm thấy tiêu chí để xoá");
        }

        List<Guid> ids = request.RequestData.Ids.Select(m => Guid.Parse(m)).ToList();

        var query = await _context.Criterias.Where(x => x.DeleteFlag != true && ids.Contains(x.Id))
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

            await _eventLogService.Create("CriteriaFeature", "CriteriaFeature", "Criteria_DeleteCommand", request.RequestData.ApplicationUserId);
        }

        _context.Criterias.UpdateRange(query);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
