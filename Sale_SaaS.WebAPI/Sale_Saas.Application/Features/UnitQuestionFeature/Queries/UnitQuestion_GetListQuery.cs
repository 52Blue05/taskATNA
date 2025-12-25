using Sale_Saas.Application.Features.UnitFeature.Dto;
using Sale_Saas.Application.Features.UnitQuestionFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.UnitQuestionFeature.Queries;

public record UnitQuestion_GetListQuery : IRequest<Result<IEnumerable<UnitQuestionDto>>>;

public class UnitQuestion_GetListQueryHandler : IRequestHandler<UnitQuestion_GetListQuery, Result<IEnumerable<UnitQuestionDto>>>
{
    private readonly IApplicationDbContext _context;
    private IEventLogService _eventLogService;

    public UnitQuestion_GetListQueryHandler(IApplicationDbContext context, IEventLogService eventLogService)
    {
        _context = context;
        _eventLogService = eventLogService;
    }

    public async Task<Result<IEnumerable<UnitQuestionDto>>> Handle(UnitQuestion_GetListQuery request, CancellationToken cancellationToken)
    {
        var query = from unitQuestion in _context.UnitQuestions
                    join unit in _context.Units
                    on unitQuestion.UnitId equals unit.Id into unitQuestion_unit
                    from unit in unitQuestion_unit.DefaultIfEmpty()
                    where unitQuestion.DeleteFlag != true
                    select new { unitQuestion, unit };

        // default sort asc by name
        query = query.OrderBy(x => x.unitQuestion.Id);


        var result = await query.AsNoTracking()
                                .Select(x => new UnitQuestionDto()
                                {
                                    Id = x.unitQuestion.Id,
                                    MinScore = x.unitQuestion.MinScore,
                                    MaxScore = x.unitQuestion.MaxScore,
                                    MinNumberQuestion = x.unitQuestion.MinNumberQuestion,
                                    MaxNumberQuestion = x.unitQuestion.MaxNumberQuestion,
                                    Time = x.unitQuestion.Time,
                                    UnitId = x.unitQuestion.UnitId,
                                    Unit = x.unit != null ? new UnitDto()
                                    {
                                        Id = x.unit.Id,
                                        Name = x.unit.Name,
                                        EndTime = x.unit.EndTime,
                                        ThumbnailPath = x.unit.ThumbnailPath,
                                    } : null
                                })
                                .ToListAsync();

        var eventLog = await _eventLogService.Create("UnitQuestionFeature", "UnitQuestionFeature", "UnitQuestion_GetListQuery", Guid.Empty);

        return Result<IEnumerable<UnitQuestionDto>>.Success(result.AsReadOnly());
    }
}
