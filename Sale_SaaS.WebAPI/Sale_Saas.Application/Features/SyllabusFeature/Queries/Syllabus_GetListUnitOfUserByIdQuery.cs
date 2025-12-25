using Sale_Saas.Application.Features.SyllabusFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.SyllabusFeature.Queries;

public record Syllabus_GetListUnitOfUserByIdQuery(Guid UserId, Guid SyllabusId, bool? IsQualified) : IRequest<Result<SyllabusUnitOfUserDto>>;

public class Syllabus_GetListUnitOfUserByIdQueryHandler : IRequestHandler<Syllabus_GetListUnitOfUserByIdQuery, Result<SyllabusUnitOfUserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventLogService _eventLogService;

    public Syllabus_GetListUnitOfUserByIdQueryHandler(IApplicationDbContext context, IEventLogService eventLogService)
    {
        _context = context;
        _eventLogService = eventLogService;
    }

    public async Task<Result<SyllabusUnitOfUserDto>> Handle(Syllabus_GetListUnitOfUserByIdQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy user");
        }

        var query = from user in _context.ApplicationUsers
                    join userSyllabus in _context.ApplicationUserSyllabus
                    on user.Id equals userSyllabus.ApplicationUserId into user_userSyllabus
                    from userSyllabus in user_userSyllabus.DefaultIfEmpty()
                    join syllabus in _context.Syllabus
                    on userSyllabus.SyllabusId equals syllabus.Id into user_syllabus
                    from syllabus in user_syllabus.DefaultIfEmpty()
                    join syllabusUnit in _context.SyllabusUnits
                    on syllabus.Id equals syllabusUnit.SyllabusId into user_syllabusUnit
                    from syllabusUnit in user_syllabusUnit.DefaultIfEmpty()
                    join unit in _context.Units
                    on syllabusUnit.UnitId equals unit.Id into user_unit
                    from unit in user_unit.DefaultIfEmpty()
                    join result in _context.Results on new { ApplicationUserId = user.Id, UnitId = unit.Id } equals new { result.ApplicationUserId, result.UnitId } into result_custom
                    from result in result_custom.DefaultIfEmpty()
                    where !(user.DeleteFlag ?? false)
                        && user.Id == request.UserId
                        && (userSyllabus == null || !userSyllabus.DeleteFlag)
                        && (syllabus == null || !syllabus.DeleteFlag)
                        && (syllabusUnit == null || !syllabusUnit.DeleteFlag)
                        && (unit == null || !unit.DeleteFlag)
                        && (syllabus != null && syllabus.Id == request.SyllabusId)
                        && (result == null || !(result.isOldResult ?? false))
                    //&& result.isQualified == request.IsQualified
                    select new
                    {
                        user,
                        unit,
                        SyllabusId = syllabus.Id,
                        SyllabusName = syllabus.Name,
                        SyllabusEndTime = syllabus.EndTime,
                        CompletionDate = result.LastModifiedDate,
                        CorrectQuestion = result != null ? result.CorrectQuestion : 0,
                        TotalQuestion = result != null ? result.ToTalQuestion : 0,
                        isQualified = result != null ? result.isQualified : false
                    };

        var groupedSearchResults = await query
                .AsQueryable()
                .GroupBy(x => new { x.user.Id, x.user.FullName, x.SyllabusId, x.SyllabusName })
                .Select(g => new SyllabusUnitOfUserDto()
                {
                    Id = g.Key.SyllabusId,
                    FullName = g.Key.FullName ?? "",
                    SyllabusName = g.Key.SyllabusName ?? "",
                    Units = g.Where(u => u.unit != null && ((request.IsQualified!=null && u.isQualified == request.IsQualified) || (request.IsQualified == null && u.isQualified!=null)))
                             .Select(u => new UnitOfUserDto()
                             {
                                 Id = u.unit.Id,
                                 UnitName = u.unit.Name ?? "",
                                 EndTime = u.SyllabusEndTime,
                                 ThumbnailPath = u.unit.ThumbnailPath ?? "",
                                 Description = u.unit.Description ?? "",
                                 isQualified = u.isQualified,
                                 CorrectQuestion = u.CorrectQuestion,
                                 TotalQuestion = u.TotalQuestion,
                                 CompletionDate = u.CompletionDate
                             })
                             .ToList()
                })
                .FirstOrDefaultAsync();

        return Result<SyllabusUnitOfUserDto>.Success(groupedSearchResults);
    }
}
