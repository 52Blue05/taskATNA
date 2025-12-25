using Sale_Saas.Application.Features.LessionFeature.Dto;
using Sale_Saas.Application.Features.UnitFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using System;

namespace Sale_Saas.Application.Features.LessionFeature.Queries;

public record Lession_GetListByUnitIdQuery(Guid UnitId, Guid userId) : IRequest<Result<IEnumerable<LessionDto>>>;

public class Lession_GetListByUnitIdQueryHandler : IRequestHandler<Lession_GetListByUnitIdQuery, Result<IEnumerable<LessionDto>>>
{
    private readonly IApplicationDbContext _context;
    private IEventLogService _eventLogService;

    public Lession_GetListByUnitIdQueryHandler(IApplicationDbContext context, IEventLogService eventLogService)
    {
        _context = context;
        _eventLogService = eventLogService;
    }

    public async Task<Result<IEnumerable<LessionDto>>> Handle(Lession_GetListByUnitIdQuery request, CancellationToken cancellationToken)
    {
        var query = from lession in _context.Lessions
                    join unit in _context.Units
                    on lession.UnitId equals unit.Id into lession_unit
                    from unit in lession_unit.DefaultIfEmpty()
                    join userProgress in _context.UserLessonProgresses.Where(x=> x.ApplicationUserId == request.userId)
                    on lession.Id equals userProgress.LessonId  into lesson_progress 
                    from progress in lesson_progress.DefaultIfEmpty()
                    where lession.DeleteFlag != true && lession.UnitId == request.UnitId
                    select new { lession, unit, progress };

        // default sort asc by name
        query = query.OrderBy(x => x.lession.SortOrder);

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
                                    IsDone = x.progress != null &&   x.progress.ApplicationUserId == request.userId ? x.progress.IsDone : false,
                                    Unit = x.unit != null ? new UnitDto()
                                    {
                                        Id = x.unit.Id,
                                        Name = x.unit.Name,
                                        EndTime = x.unit.EndTime,
                                        ThumbnailPath = x.unit.ThumbnailPath,
                                    } : null
                                })
                                .ToListAsync();

        var eventLog = await _eventLogService.Create("LessionFeature", "LessionFeature", "Lession_GetListByUnitIdQuery", Guid.Empty);

        return Result<IEnumerable<LessionDto>>.Success(result.AsReadOnly());
    }
}