using Sale_Saas.Application.Features.LessionFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.LessionFeature.Commands;

public record Lesson_UpdateStatusLearningCommand(Guid UserId, UpdateStatusLessonRequest RequestData) : IRequest<Result<bool>>;

public class Lesson_UpdateStatusLearningCommandHandler : IRequestHandler<Lesson_UpdateStatusLearningCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventLogService _eventLogService;

    public Lesson_UpdateStatusLearningCommandHandler(IApplicationDbContext context, IEventLogService eventLogService)
    {
        _context = context;
        _eventLogService = eventLogService;
    }

    public async Task<Result<bool>> Handle(Lesson_UpdateStatusLearningCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy user");
        }

        if (request.RequestData.LessonId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy bài học");
        }

        // check lesson allowed or not
        var lesson = await _context.Lessions.Where(x => x.DeleteFlag != true && x.Id == request.RequestData.LessonId)
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync();

        if (lesson == null)
        {
            throw new ApplicationException("Không tìm thấy bài học");
        }

        // check sort order => the highest sort order done
        if (lesson.SortOrder != 1)
        {
            var prevSortOrder = lesson.SortOrder - 1;

            var prevLesson = await _context.Lessions.Where(x => x.DeleteFlag != true && x.UnitId == lesson.UnitId &&  x.SortOrder < lesson.SortOrder)
                                                    .AsNoTracking()
                                                     .OrderByDescending(x => x.SortOrder)
                                                    .FirstOrDefaultAsync();

            if (prevLesson == null)
            {
                throw new ApplicationException("Không tìm thấy bài học trước đó");
            }

            var isAllowedLearn = await _context.UserLessonProgresses.Where(x => x.ApplicationUserId == request.UserId && x.LessonId == prevLesson.Id && x.IsDone == true)
                                                                    .AsNoTracking()
                                                                    .FirstOrDefaultAsync();

            if (isAllowedLearn == null)
            {
                return Result<bool>.Success(false);
            }
        }


        var userLessonProgress = await _context.UserLessonProgresses.Where(s => s.DeleteFlag != true && s.LessonId == request.RequestData.LessonId && s.ApplicationUserId==request.UserId)
                                                                    .FirstOrDefaultAsync();

        if (userLessonProgress == null)
        {
            var newUserLessonProgress = new UserLessonProgress()
            {
                LessonId = request.RequestData.LessonId,
                IsDone = request.RequestData.IsDone == null ? false : request.RequestData.IsDone,
                ApplicationUserId = request.UserId
            };

            _context.UserLessonProgresses.Add(newUserLessonProgress);
        }
        else
        {
            userLessonProgress.LessonId = request.RequestData.LessonId;
            userLessonProgress.IsDone = request.RequestData.IsDone == null ? false : request.RequestData.IsDone;

            userLessonProgress.LastModifiedApplicationUserId = request.UserId;
            userLessonProgress.LastModifiedDate = DateTime.Now;

            _context.UserLessonProgresses.Update(userLessonProgress);
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _eventLogService.Create("LessonFeature", "LessonFeature", "Lesson_UpdateStatusLearningCommand", request.UserId);

        return Result<bool>.Success(true);
    }
}
