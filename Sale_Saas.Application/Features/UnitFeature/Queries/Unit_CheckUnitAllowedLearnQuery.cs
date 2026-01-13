using Sale_Saas.Application.Features.SyllabusFeature.Requests;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.UnitFeature.Queries;

public record Unit_CheckUnitAllowedLearnQuery(Guid UserId, SyllabusUnitRequest RequestData) : IRequest<Result<bool>>;

public class Unit_CheckUnitAllowedLearnQueryHandler : IRequestHandler<Unit_CheckUnitAllowedLearnQuery, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventLogService _eventLogService;

    public Unit_CheckUnitAllowedLearnQueryHandler(IApplicationDbContext context, IEventLogService eventLogService)
    {
        _context = context;
        _eventLogService = eventLogService;
    }

    public async Task<Result<bool>> Handle(Unit_CheckUnitAllowedLearnQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy user");
        }

        if (request.RequestData.UnitId == Guid.Empty)
        {
            throw new ApplicationException("Id của khoá học không được phép null");
        }

        var syllabusUnit = await _context.SyllabusUnits.Where(x => x.DeleteFlag != true
                                                                && x.SyllabusId == request.RequestData.SyllabusId
                                                                && x.UnitId == request.RequestData.UnitId)
                                                       .AsNoTracking()
                                                       .FirstOrDefaultAsync();

        if (syllabusUnit == null)
        {
            throw new ApplicationException("Không tìm thấy khoá học");
        }

        if (syllabusUnit.SortOrder==null ||  syllabusUnit.SortOrder == 1)
        {
            return Result<bool>.Success(true);
        }

        var prevSyllabusUnitSortOrder = syllabusUnit.SortOrder - 1;

        var prevSyllabusUnit = await _context.SyllabusUnits.Where(x => x.DeleteFlag != true
                                                                    && x.SyllabusId == request.RequestData.SyllabusId
                                                                    && x.SortOrder < syllabusUnit.SortOrder)
                                                           .AsNoTracking()
                                                           .OrderByDescending(x=>x.SortOrder)
                                                           .FirstOrDefaultAsync();

        if (prevSyllabusUnit == null)
        {
            throw new ApplicationException("Không tìm thấy khoá học trước");
        }

        var isExistLessonInPrevSyllabusUnit = await _context.Lessions.Where(x => x.UnitId == prevSyllabusUnit.UnitId && x.DeleteFlag != true)
                                                                     .AsNoTracking()
                                                                     .ToListAsync();

        if (!isExistLessonInPrevSyllabusUnit.Any())
        {
            return Result<bool>.Success(false);
        }

        // check done prev syllabus unit
        var query = from le in _context.Lessions
                    join ulp in _context.UserLessonProgresses.Where(x=>x.ApplicationUserId==request.UserId)                 
                    on le.Id equals ulp.LessonId into le_ulp
                    from ulp in le_ulp.DefaultIfEmpty()
                    where le.DeleteFlag != true
                       && le.UnitId == prevSyllabusUnit.UnitId
                       && (ulp == null || (ulp.IsDone == false
                                        && ulp.ApplicationUserId == request.UserId))
                    select le;

        var isNotAllowedLearn = await query.AnyAsync();

        if (isNotAllowedLearn == true)
        {
            return Result<bool>.Success(false);
        }

        return Result<bool>.Success(true);
    }
}
