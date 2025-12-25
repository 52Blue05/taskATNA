using Sale_Saas.Application.Common.Mappings;
using Sale_Saas.Application.Features.UnitFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Sale_Saas.Application.Features.UnitFeature.Queries;

public record Unit_GetListWithPaginationByUserQuery(Guid UserId, Guid SyllabusId, GetListWithPaginationQueryRequest RequestData) : IRequest<Result<PaginatedList<UnitOfMemberDto>>>;

public class Unit_GetListWithPaginationByUserQueryHandler : IRequestHandler<Unit_GetListWithPaginationByUserQuery, Result<PaginatedList<UnitOfMemberDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventLogService _eventLogService;

    public Unit_GetListWithPaginationByUserQueryHandler(IApplicationDbContext context, IEventLogService eventLogService)
    {
        _context = context;
        _eventLogService = eventLogService;
    }

    public async Task<Result<PaginatedList<UnitOfMemberDto>>> Handle(Unit_GetListWithPaginationByUserQuery request, CancellationToken cancellationToken)
    {
        var query = from
                    syllabus in _context.Syllabus
                    join
                    syllabusUnit in _context.SyllabusUnits
                    on syllabus.Id equals syllabusUnit.SyllabusId
                    join unit in _context.Units
                    on syllabusUnit.UnitId equals unit.Id into syllabusUnit_unit
                    from unit in syllabusUnit_unit.DefaultIfEmpty()
                    where syllabusUnit.DeleteFlag != true
                       && unit.DeleteFlag != true
                       && syllabusUnit.SyllabusId == request.SyllabusId
                    select new
                    {
                        syllabus,
                        unit,
                        syllabusUnit,
                        isQualified = _context.Results.Any(r => r.UnitId == unit.Id
                                                        && r.ApplicationUserId == request.UserId
                                                        && r.DeleteFlag != true
                                                        && r.isOldResult == false
                                                        && r.isQualified == true)
                    };
    

        var results = await query.AsNoTracking()
                                 .Select(x => new UnitOfMemberDto()
                                 {
                                     Id = x.unit.Id,
                                     Name = x.unit.Name,
                                     Description = x.unit.Description,
                                     EndTime = x.syllabus.EndTime,
                                     ThumbnailPath = x.unit.ThumbnailPath,
                                     isQualified = x.isQualified,
                                     SortOrder = x.syllabusUnit.SortOrder
                                 })
                                 .OrderBy(s => s.SortOrder)
                                 .PaginatedListAsync(request.RequestData.PageIndex, request.RequestData.PageSize);  
        var resResult= Result<PaginatedList<UnitOfMemberDto>>.Success(results);
        var index = 0;
        foreach ( var item in resResult.Data.Items )
        {
            index++;
            if(item.SortOrder==null)
            {
                item.SortOrder = index;
            }
            var isLearnAll=await CheckLearnAll(item.Id.Value, request.UserId);
            if(item.isQualified==true && !isLearnAll)
            {
                item.isQualified = true;
            }
            else
            {
                item.isQualified = false;
            }
        }
        return resResult;
    }

    private async Task<bool> CheckLearnAll(Guid unitId, Guid userId)
    {
        var queryLearn = from le in _context.Lessions
                         join ulp in _context.UserLessonProgresses.Where(x => x.ApplicationUserId == userId)
                         on le.Id equals ulp.LessonId into le_ulp
                         from ulp in le_ulp.DefaultIfEmpty()
                         where le.DeleteFlag != true
                            && le.UnitId == unitId
                            && (ulp == null || (ulp.IsDone == false
                                             && ulp.ApplicationUserId == userId))
                         select le;
        var resultLearn = await queryLearn.AnyAsync();
       return resultLearn;
    }
}
