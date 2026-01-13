using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.GainsFamilyFeature.Commands;

public record GainsFamily_DeleteByIdsCommand(Guid userId, DeleteRequest RequestData) : IRequest<Result<bool>>;

public class GainsFamily_DeleteByIdsCommandHandler : IRequestHandler<GainsFamily_DeleteByIdsCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventLogService _eventLogService;

    public GainsFamily_DeleteByIdsCommandHandler(IApplicationDbContext context, IEventLogService eventLogService)
    {
        _context = context;
        _eventLogService = eventLogService;
    }

    public async Task<Result<bool>> Handle(GainsFamily_DeleteByIdsCommand request, CancellationToken cancellationToken)
    {
        if (request.RequestData.Ids == null)
        {
            throw new ApplicationException("Không tìm thấy gia đình để xoá");
        }

        List<Guid> ids = request.RequestData.Ids.Select(m => Guid.Parse(m)).ToList();

        var query = await _context.GainsFamilies.Where(x => x.DeleteFlag != true && ids.Contains(x.Id))
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

            await _eventLogService.Create("GainsFamilyFeature", "GainsFamilyFeature", "GainsFamily_DeleteByIdsCommand", request.RequestData.ApplicationUserId);
        }

        _context.GainsFamilies.UpdateRange(query);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
