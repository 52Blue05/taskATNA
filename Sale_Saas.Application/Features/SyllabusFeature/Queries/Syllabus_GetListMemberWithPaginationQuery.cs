using Sale_Saas.Application.Common.Mappings;
using Sale_Saas.Application.Features.SyllabusFeature.Dto;
using Sale_Saas.Application.Features.UnitFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.SyllabusFeature.Queries;

public record Syllabus_GetListMemberWithPaginationQuery(Guid userId, Guid SyllabusId, GetListWithPaginationQueryRequest RequestData) : IRequest<Result<PaginatedList<SyllabusMemberDto>>>;

public class Syllabus_GetListMemberWithPaginationQueryHandler : IRequestHandler<Syllabus_GetListMemberWithPaginationQuery, Result<PaginatedList<SyllabusMemberDto>>>
{
    private readonly IApplicationDbContext _context;
    private IEventLogService _eventLogService;

    public Syllabus_GetListMemberWithPaginationQueryHandler(IApplicationDbContext context, IEventLogService eventLogService)
    {
        _context = context;
        _eventLogService = eventLogService;
    }

    public async Task<Result<PaginatedList<SyllabusMemberDto>>> Handle(Syllabus_GetListMemberWithPaginationQuery request, CancellationToken cancellationToken)
    {
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
                        && (userSyllabus == null || !userSyllabus.DeleteFlag)
                        && (syllabus == null || !syllabus.DeleteFlag)
                        && (syllabusUnit == null || !syllabusUnit.DeleteFlag)
                        && (unit == null || !unit.DeleteFlag)
                        && (syllabus != null && syllabus.Id == request.SyllabusId)
                        && (result == null || !(result.isOldResult ?? false))
                    select new
                    {
                        user,
                        unit,
                        SyllabusId = syllabus.Id,
                        CorrectQuestion = result != null ? result.CorrectQuestion : 0,
                        TotalQuestion = result != null ? result.ToTalQuestion : 0,
                        isQualified = result != null ? result.isQualified : false
                    };

        if (request.RequestData.TextSearch != null)
        {
            var groupedSearchResults = await query
                    .AsQueryable()
                    .GroupBy(x => new { x.user.Id, x.user.FirstName, x.user.LastName, x.user.FullName, x.user.Email, x.SyllabusId })
                    .Where(s => s.Key.FullName.ToLower().Contains(request.RequestData.TextSearch.ToLower()) || s.Key.Email.ToLower().Contains(request.RequestData.TextSearch.ToLower()))
                    .Select(g => new SyllabusMemberDto()
                    {
                        Id = g.Key.SyllabusId,
                        UserId = g.Key.Id,
                        FirstName = g.Key.FirstName ?? "",
                        LastName = g.Key.LastName ?? "",
                        FullName = g.Key.FullName,
                        Email = g.Key.Email ?? "",
                        UnitResults = g.Where(u => u.unit != null)
                                       .Select(u => new UnitResultDto()
                                       {
                                           Id = u.unit.Id,
                                           Name = u.unit.Name ?? "",
                                           CorrectQuestion = u.CorrectQuestion,
                                           TotalQuestion = u.TotalQuestion,
                                           isQualified = u.isQualified
                                       }).ToList()
                    }).PaginatedListAsync(request.RequestData.PageIndex, request.RequestData.PageSize);

            return Result<PaginatedList<SyllabusMemberDto>>.Success(groupedSearchResults);
        }

        var groupedResults = await query.AsQueryable()
                                        .GroupBy(x => new { x.user.Id, x.user.FirstName, x.user.LastName, x.user.FullName, x.user.Email, x.SyllabusId })
                                        .Select(g => new SyllabusMemberDto()
                                        {
                                            Id = g.Key.SyllabusId,
                                            UserId = g.Key.Id,
                                            FirstName = g.Key.FirstName ?? "",
                                            LastName = g.Key.LastName ?? "",
                                            FullName = g.Key.FullName,
                                            Email = g.Key.Email ?? "",
                                            UnitResults = g.Where(u => u.unit != null)
                                                           .Select(u => new UnitResultDto()
                                                           {
                                                               Id = u.unit.Id,
                                                               Name = u.unit.Name ?? "",
                                                               CorrectQuestion = u.CorrectQuestion,
                                                               TotalQuestion = u.TotalQuestion,
                                                               isQualified = u.isQualified
                                                           }).ToList()
                                        }).PaginatedListAsync(request.RequestData.PageIndex, request.RequestData.PageSize);
        return Result<PaginatedList<SyllabusMemberDto>>.Success(groupedResults);
    }
}
