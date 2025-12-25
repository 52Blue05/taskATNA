using Sale_Saas.Application.Features.UnitFeature.Dto;
using Sale_Saas.Application.Features.UnitQuestionFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.UnitQuestionFeature.Queries;

public record UnitQuestion_GetByIdQuery(Guid Id) : IRequest<Result<UnitQuestionDto>>;

public class UnitQuestion_GetByIdQueryHandler : IRequestHandler<UnitQuestion_GetByIdQuery, Result<UnitQuestionDto>>
{
    private readonly IApplicationDbContext _context;
    private IEventLogService _eventLogService;

    public UnitQuestion_GetByIdQueryHandler(IApplicationDbContext context, IEventLogService eventLogService)
    {
        _context = context;
        _eventLogService = eventLogService;
    }

    public async Task<Result<UnitQuestionDto>> Handle(UnitQuestion_GetByIdQuery request, CancellationToken cancellationToken)
    {
        var query = from unitQuestion in _context.UnitQuestions
                    join unit in _context.Units
                    on unitQuestion.UnitId equals unit.Id into unitQuestion_unit
                    from unit in unitQuestion_unit.DefaultIfEmpty()
                    where unitQuestion.DeleteFlag != true && unitQuestion.Id == request.Id
                    select new { unitQuestion, unit };

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
                                .FirstOrDefaultAsync();

        var eventLog = await _eventLogService.Create("UnitQuestionFeature", "UnitQuestionFeature", "UnitQuestion_GetByIdQuery", request.Id);

        return Result<UnitQuestionDto>.Success(result);
    }
}