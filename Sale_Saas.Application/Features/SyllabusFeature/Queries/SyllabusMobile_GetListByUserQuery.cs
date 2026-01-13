using Sale_Saas.Application.Common.Mappings;
using Sale_Saas.Application.Features.SyllabusFeature.Dto;
using Sale_Saas.Application.Features.UnitFeature.Dto;

namespace Sale_Saas.Application.Features.SyllabusFeature.Queries;

public record SyllabusMobile_GetListByUserQuery(Guid UserId, GetListWithPaginationQueryRequest RequestData) : IRequest<Result<PaginatedList<SyllabusOfUserDto>>>;

public class SyllabusMobile_GetListByUserQueryHandler : IRequestHandler<SyllabusMobile_GetListByUserQuery, Result<PaginatedList<SyllabusOfUserDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SyllabusMobile_GetListByUserQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedList<SyllabusOfUserDto>>> Handle(SyllabusMobile_GetListByUserQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty && request.RequestData.UserId != null && request.RequestData.UserId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy người dùng");
        }

        var userId = request.RequestData.UserId ?? Guid.Empty;

        var query = from userSyllabus in _context.ApplicationUserSyllabus
                    join syllabus in _context.Syllabus
                    on userSyllabus.SyllabusId equals syllabus.Id
                    join syllabusUnit in _context.SyllabusUnits
                    on syllabus.Id equals syllabusUnit.SyllabusId
                    join unit in _context.Units
                    on syllabusUnit.UnitId equals unit.Id
                    join result in _context.Results on new { ApplicationUserId = userId, UnitId = unit.Id } equals new { result.ApplicationUserId, result.UnitId } into result_custom
                    from result in result_custom.DefaultIfEmpty()
                    where userSyllabus.ApplicationUserId == request.RequestData.UserId
                          && !(userSyllabus.DeleteFlag)
                          && !(syllabus.DeleteFlag)
                          && !(syllabusUnit.DeleteFlag)
                          && !(unit.DeleteFlag)
                          && (result == null || !(result.isOldResult ?? false))
                    select new
                    {
                        syllabus,
                        unit,
                        SortOrder = syllabusUnit.SortOrder,
                        CorrectQuestion = result != null ? result.CorrectQuestion : 0,
                        TotalQuestion = result != null ? result.ToTalQuestion : 0,
                        isQualified = result != null ? result.isQualified : false,
                        result
                    };

        var groupedResults = await query.AsQueryable()
                                        .GroupBy(x => new { x.syllabus.Id, x.syllabus.Name })
                                        .Select(g => new SyllabusOfUserDto()
                                        {
                                            Id = g.Key.Id,
                                            SyllabusName = g.Key.Name ?? "",
                                            UnitResults = g.Where(u => u.unit != null)
                                                           .OrderBy(u => u.SortOrder)
                                                           .Select(u => new UnitResultDto()
                                                           {
                                                               Id = u.unit.Id,
                                                               Name = u.unit.Name ?? "",
                                                               CorrectQuestion = u.CorrectQuestion,
                                                               TotalQuestion = u.TotalQuestion,
                                                               isQualified = u.isQualified
                                                           }).ToList()
                                        })
                                        .PaginatedListAsync(request.RequestData.PageIndex, request.RequestData.PageSize);

        return Result<PaginatedList<SyllabusOfUserDto>>.Success(groupedResults);
    }
}
