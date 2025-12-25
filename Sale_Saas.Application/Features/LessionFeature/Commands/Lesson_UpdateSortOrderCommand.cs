using Sale_Saas.Application.Features.LessionFeature.Requests;

namespace Sale_Saas.Application.Features.LessionFeature.Commands;

public record Lesson_UpdateSortOrderCommand(Guid UserId, LessonUpdateSortOrderRequest RequestData) : IRequest<Result<bool>>;

public class Lesson_UpdateSortOrderCommandHandler : IRequestHandler<Lesson_UpdateSortOrderCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public Lesson_UpdateSortOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(Lesson_UpdateSortOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy người dùng");
        }
        var lesson = await _context.Lessions.Where(x => x.Id == request.RequestData.LessonId && x.DeleteFlag != true)
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync();

        if (lesson == null)
        {
            throw new ApplicationException("Không tìm thấy bài học");
        }

        var count = await _context.Lessions.Where(x => x.DeleteFlag != true && x.UnitId == request.RequestData.UnitId)
                                           .AsNoTracking()
                                           .CountAsync();

        await HandleSortOrderWithCaseUpdate(lesson, count, request.RequestData.SortOrder);

        lesson.SortOrder = request.RequestData.SortOrder;

        _context.Lessions.Update(lesson);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    public async Task HandleSortOrderWithCaseUpdate(Lessions obj, int count, int sortOrder)
    {
        if (obj.SortOrder == sortOrder)
        {
            return;
        }

        if (sortOrder <= 0 || sortOrder >= count)
        {
            sortOrder = count;
        }

        // 1 <= count <= n
        List<Lessions> listLessons;

        // origin > target
        if (obj.SortOrder > sortOrder)
        {
            listLessons = await _context.Lessions.Where(s => s.DeleteFlag != true && s.UnitId == obj.UnitId
                                                          && s.SortOrder >= sortOrder
                                                          && s.SortOrder < obj.SortOrder)
                                                 .OrderBy(s => s.SortOrder)
                                                 .ToListAsync();

            var index = sortOrder + 1;

            foreach (var item in listLessons)
            {
                item.SortOrder = index;
                index = index + 1;
            }
        }
        else
        {
            listLessons = await _context.Lessions.Where(s => s.DeleteFlag != true && s.UnitId == obj.UnitId
                                                          && s.SortOrder <= sortOrder
                                                          && s.SortOrder > obj.SortOrder)
                                                 .OrderBy(s => s.SortOrder)
                                                 .ToListAsync();

            var index = obj.SortOrder;
            foreach (var item in listLessons)
            {
                item.SortOrder = index;
                index = index + 1;
            }
        }

        _context.Lessions.UpdateRange(listLessons);
    }
}
