namespace Sale_Saas.Application.Features.UnitQuestionFeature.Queries;

public record UnitQuestion_CheckUserLearnAllLessonOfUnitQuery(Guid UserId, Guid UnitId) : IRequest<Result<bool>>;

public class UnitQuestion_CheckUserLearnAllLessonOfUnitQueryHandler : IRequestHandler<UnitQuestion_CheckUserLearnAllLessonOfUnitQuery, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public UnitQuestion_CheckUserLearnAllLessonOfUnitQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(UnitQuestion_CheckUserLearnAllLessonOfUnitQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy user");
        }

        if (request.UnitId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy khoá học");
        }

        var query = from le in _context.Lessions
                    join ulp in _context.UserLessonProgresses.Where(x=>x.ApplicationUserId== request.UserId)
                    on le.Id equals ulp.LessonId into le_ulp
                    from ulp in le_ulp.DefaultIfEmpty()
                    where le.DeleteFlag != true
                       && le.UnitId == request.UnitId
                       && (ulp == null || (ulp.IsDone == false
                                        && ulp.ApplicationUserId == request.UserId))
                    select le;
        var result= await query.AnyAsync();
        if (result)
        {
            return Result<bool>.Success(false);
        }
        return Result<bool>.Success(true);
    }
}
