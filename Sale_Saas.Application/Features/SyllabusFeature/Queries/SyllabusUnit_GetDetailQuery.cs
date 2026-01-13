using Sale_Saas.Application.Features.SyllabusFeature.Dto;
using Sale_Saas.Application.Features.SyllabusFeature.Requests;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.SyllabusFeature.Queries;

public record SyllabusUnit_GetDetailQuery(Guid UserId, SyllabusUnitRequest RequestData) : IRequest<Result<SyllabusUnitDto>>;

public class SyllabusUnit_GetDetailQueryHandler : IRequestHandler<SyllabusUnit_GetDetailQuery, Result<SyllabusUnitDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventLogService _eventLogService;

    public SyllabusUnit_GetDetailQueryHandler(IApplicationDbContext context, IEventLogService eventLogService)
    {
        _context = context;
        _eventLogService = eventLogService;
    }

    public async Task<Result<SyllabusUnitDto>> Handle(SyllabusUnit_GetDetailQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy user");
        }

        if (request.RequestData.UnitId == Guid.Empty)
        {
            throw new ApplicationException("Id của khoá học không được phép null");
        }

        var syllabusUnit = from su in _context.SyllabusUnits
                           join unit in _context.Units
                           on su.UnitId equals unit.Id into su_unit
                           from unit in su_unit.DefaultIfEmpty()
                           where su.DeleteFlag != true
                              && su.SyllabusId == request.RequestData.SyllabusId
                              && su.UnitId == request.RequestData.UnitId
                              && unit != null && unit.DeleteFlag != true
                           select new { su, unit };

        var result = await syllabusUnit.AsNoTracking()
                                       .Select(x => new SyllabusUnitDto()
                                       {
                                           Id = x.su.Id,
                                           UnitId = x.unit.Id,
                                           UnitName = x.unit.Name ?? "",
                                           SortOrder = x.su.SortOrder ?? 0
                                       })
                                       .FirstOrDefaultAsync();

        return Result<SyllabusUnitDto>.Success(result);
    }
}
