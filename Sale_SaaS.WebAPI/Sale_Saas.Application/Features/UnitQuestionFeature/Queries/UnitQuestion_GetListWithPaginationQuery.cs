using Sale_Saas.Application.Common.Mappings;
using Sale_Saas.Application.Features.UnitFeature.Dto;
using Sale_Saas.Application.Features.UnitQuestionFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.UnitQuestionFeature.Queries;

public record UnitQuestion_GetListWithPaginationQuery(GetListWithPaginationQueryRequest RequestData) : IRequest<Result<PaginatedList<UnitQuestionDto>>>;

public class UnitQuestion_GetListWithPaginationQueryHandler : IRequestHandler<UnitQuestion_GetListWithPaginationQuery, Result<PaginatedList<UnitQuestionDto>>>
{
    private readonly IApplicationDbContext _context;
    private IEventLogService _eventLogService;

    public UnitQuestion_GetListWithPaginationQueryHandler(IApplicationDbContext context, IEventLogService eventLogService)
    {
        _context = context;
        _eventLogService = eventLogService;
    }

    public async Task<Result<PaginatedList<UnitQuestionDto>>> Handle(UnitQuestion_GetListWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var query = from unitQuestion in _context.UnitQuestions
                    join unit in _context.Units
                    on unitQuestion.UnitId equals unit.Id into lession_unit
                    from unit in lession_unit.DefaultIfEmpty()
                    where unitQuestion.DeleteFlag != true
                    select new { unitQuestion, unit };

        // text search
        //if (request.RequestData.TextSearch != null)
        //{
        //    query = query.Where(x => x.unitQuestion.Name.ToLower().Trim().Contains(request.RequestData.TextSearch.ToLower().Trim()));
        //}

        // filter


        // default sort asc by name
        query = query.OrderBy(x => x.unitQuestion.Id);

        // result
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
                                .PaginatedListAsync(request.RequestData.PageIndex, request.RequestData.PageSize);

        var eventLog = await _eventLogService.Create("UnitQuestionFeature", "UnitQuestionFeature", "UnitQuestion_GetListWithPaginationQuery", Guid.Empty);

        return Result<PaginatedList<UnitQuestionDto>>.Success(result);
    }
}