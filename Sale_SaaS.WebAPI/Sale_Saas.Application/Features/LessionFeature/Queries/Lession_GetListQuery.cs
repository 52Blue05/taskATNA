using Sale_Saas.Application.Features.LessionFeature.Dto;
using Sale_Saas.Application.Features.UnitFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.LessionFeature.Queries;

public record Lession_GetListQuery : IRequest<Result<IEnumerable<LessionDto>>>;

public class Lession_GetListQueryHandler : IRequestHandler<Lession_GetListQuery, Result<IEnumerable<LessionDto>>>
{
    private readonly IApplicationDbContext _context;
    private IEventLogService _eventLogService;

    public Lession_GetListQueryHandler(IApplicationDbContext context, IEventLogService eventLogService)
    {
        _context = context;
        _eventLogService = eventLogService;
    }

    public async Task<Result<IEnumerable<LessionDto>>> Handle(Lession_GetListQuery request, CancellationToken cancellationToken)
    {
        var query = from lession in _context.Lessions
                    join unit in _context.Units
                    on lession.UnitId equals unit.Id into lession_unit
                    from unit in lession_unit.DefaultIfEmpty()
                    where lession.DeleteFlag != true
                    select new { lession, unit };

        // default sort asc by name
        query = query.OrderBy(x => x.lession.Name);


        var result = await query.AsNoTracking()
                                .Select(x => new LessionDto()
                                {
                                    Id = x.lession.Id,
                                    Name = x.lession.Name ?? null,
                                    Description = x.lession.Description ?? null,
                                    FolderName = x.lession.FolderName ?? null,
                                    OriginalFileName = x.lession.OriginalFileName ?? null,
                                    FileName = x.lession.FileName ?? null,
                                    ContentType = x.lession.ContentType ?? null,
                                    FileSize = x.lession.FileSize ?? null,
                                    FilePath = x.lession.FilePath ?? null,
                                    ServerPath = x.lession.ServerPath ?? null,
                                    Extension = x.lession.Extension ?? null,
                                    Type = x.lession.Type ?? null,
                                    IsFile = x.lession.IsFile ?? null,
                                    Link = x.lession.Link ?? null,
                                    SortOrder = x.lession.SortOrder ?? null,
                                    Unit = x.unit != null ? new UnitDto()
                                    {
                                        Id = x.unit.Id,
                                        Name = x.unit.Name,
                                        EndTime = x.unit.EndTime,
                                        ThumbnailPath = x.unit.ThumbnailPath,
                                    } : null
                                })
                                .ToListAsync();

        var eventLog = await _eventLogService.Create("LessionFeature", "LessionFeature", "Lession_GetListQuery", Guid.Empty);

        return Result<IEnumerable<LessionDto>>.Success(result.AsReadOnly());
    }
}