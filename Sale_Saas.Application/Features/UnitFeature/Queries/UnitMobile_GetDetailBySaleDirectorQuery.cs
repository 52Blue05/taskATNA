using Sale_Saas.Application.Features.UnitFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.UnitFeature.Queries;

public record UnitMobile_GetDetailBySaleDirectorQuery(Guid UserId, Guid UnitId) : IRequest<Result<List<MobileUnitResultDto>>>;

public class UnitMobile_GetDetailBySaleDirectorQueryHandler : IRequestHandler<UnitMobile_GetDetailBySaleDirectorQuery, Result<List<MobileUnitResultDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventLogService _eventLogService;

    public UnitMobile_GetDetailBySaleDirectorQueryHandler(IApplicationDbContext context, IEventLogService eventLogService)
    {
        _context = context;
        _eventLogService = eventLogService;
    }

    public async Task<Result<List<MobileUnitResultDto>>> Handle(UnitMobile_GetDetailBySaleDirectorQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy người dùng");
        }

        if (request.UnitId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy khoá học");
        }

        var query = from res in _context.Results
                    join unit in _context.Units
                    on res.UnitId equals unit.Id into res_unit
                    from unit in res_unit.DefaultIfEmpty()
                    where res.DeleteFlag != true
                       && unit.DeleteFlag != true
                       && res.UnitId == request.UnitId
                    orderby res.LastModifiedDate ascending
                    select new { res, unit };

        var result = await query.AsNoTracking()
                                .Select(x => new MobileUnitResultDto()
                                {
                                    Id = x.res.Id,
                                    CompletedAt = x.res.LastModifiedDate,
                                    TimeCompletion = x.res.TimeCompletion,
                                    CorrectQuestion = x.res.CorrectQuestion,
                                    TotalQuestion = x.res.ToTalQuestion,
                                    isOldResult = x.res.isOldResult,
                                    isQualified = x.res.isQualified,
                                })
                                .ToListAsync();

        result = result.Select((item, index) =>
                       {
                           item.Order = index + 1;
                           return item;
                       })
                       .OrderByDescending(x => x.CompletedAt)
                       .ToList();

        return Result<List<MobileUnitResultDto>>.Success(result);
    }
}
