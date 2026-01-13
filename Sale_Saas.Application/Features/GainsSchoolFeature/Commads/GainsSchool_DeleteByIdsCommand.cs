using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.GainsSchoolFeature.Commads;

public record GainsSchool_DeleteByIdsCommand(Guid? userId, DeleteRequest RequestData) : IRequest<Result<bool>>;

public class GainsSchool_DeleteByIdsCommandHandler : IRequestHandler<GainsSchool_DeleteByIdsCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventLogService _eventLogService;

    public GainsSchool_DeleteByIdsCommandHandler(IApplicationDbContext context, IEventLogService eventLogService)
    {
        _context = context;
        _eventLogService = eventLogService;
    }

    public async Task<Result<bool>> Handle(GainsSchool_DeleteByIdsCommand request, CancellationToken cancellationToken)
    {
        if (request.RequestData.Ids == null)
        {
            throw new ApplicationException("Không tìm thấy trường học để xoá");
        }

        List<Guid> ids = request.RequestData.Ids.Select(m => Guid.Parse(m)).ToList();

        var query = await _context.GainsSchools.Where(x => x.DeleteFlag != true && ids.Contains(x.Id))
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

            await _eventLogService.Create("GainsSchoolFeature", "GainsSchoolFeature", "GainsSchool_DeleteByIdsCommand", request.RequestData.ApplicationUserId);
        }

        _context.GainsSchools.UpdateRange(query);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
