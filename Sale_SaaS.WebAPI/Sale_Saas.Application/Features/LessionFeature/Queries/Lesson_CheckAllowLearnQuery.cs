using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.LessionFeature.Queries;

public record Lesson_CheckAllowLearnQuery(Guid UserId, Guid LessonId) : IRequest<Result<bool>>;

public class Lesson_CheckAllowLearnQueryHandler : IRequestHandler<Lesson_CheckAllowLearnQuery, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventLogService _eventLogService;

    public Lesson_CheckAllowLearnQueryHandler(IApplicationDbContext context, IEventLogService eventLogService)
    {
        _context = context;
        _eventLogService = eventLogService;
    }

    public async Task<Result<bool>> Handle(Lesson_CheckAllowLearnQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy user");
        }

        if (request.LessonId == Guid.Empty)
        {
            throw new ApplicationException("Id của bài học không được phép null");
        }

        var lesson = await _context.Lessions.Where(x => x.DeleteFlag != true && x.Id == request.LessonId)
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync();

        if (lesson == null)
        {
            throw new ApplicationException("Không tìm thấy bài học");
        }

        // check sort order => the highest sort order done
        if (lesson.SortOrder==null || lesson.SortOrder == 1)
        {
            return Result<bool>.Success(true);
        }

        //var prevSortOrder = lesson.SortOrder - 1;

        var prevLesson = await _context.Lessions.Where(x => x.DeleteFlag != true && x.UnitId == lesson.UnitId && x.SortOrder< lesson.SortOrder)
                                                .AsNoTracking()
                                                .OrderByDescending(x => x.SortOrder)
                                                .FirstOrDefaultAsync();

        if (prevLesson == null)
        {
            return Result<bool>.Success(true);
            //throw new ApplicationException("Không tìm thấy bài học trước đó");
        }

        var isAllowedLearn = await _context.UserLessonProgresses.Where(x => x.ApplicationUserId == request.UserId && x.LessonId == prevLesson.Id && x.IsDone == true)
                                                                .AsNoTracking()
                                                                .FirstOrDefaultAsync();

        if (isAllowedLearn == null)
        {
            return Result<bool>.Success(false);
        }

        return Result<bool>.Success(true);
    }
}
