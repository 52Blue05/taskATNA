using Sale_Saas.Application.Common.Mappings;
using Sale_Saas.Application.Features.CriteriaFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.CriteriaFeature.Queries;

public record Criteria_GetListWithPaginationQuery(Guid UserId, GetListWithPaginationQueryRequest RequestData) : IRequest<Result<PaginatedList<CriteriaDto>>>;

public class Criteria_GetListWithPaginationQueryHandler : IRequestHandler<Criteria_GetListWithPaginationQuery, Result<PaginatedList<CriteriaDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventLogService _eventLogService;
    private readonly IMapper _mapper;

    public Criteria_GetListWithPaginationQueryHandler(IApplicationDbContext context, IEventLogService eventLogService, IMapper mapper)
    {
        _context = context;
        _eventLogService = eventLogService;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedList<CriteriaDto>>> Handle(Criteria_GetListWithPaginationQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy người dùng");
        }

        var query = from criteria in _context.Criterias
                    where criteria.DeleteFlag != true
                    select new { criteria };

        // text search
        if (request.RequestData.TextSearch != null)
        {
            query = query.Where(x => x.criteria.Name != null && x.criteria.Name.ToLower().Trim().Contains(request.RequestData.TextSearch.ToLower().Trim()));
        }

        // default sort asc by id
        query = query.OrderByDescending(x => x.criteria.CreatedDate);

        // result
        var result = await query.AsNoTracking()
                                .Select(x => new CriteriaDto()
                                {
                                    Id = x.criteria.Id,
                                    Code = x.criteria.Code,
                                    Name = x.criteria.Name,
                                })
                                .PaginatedListAsync(request.RequestData.PageIndex, request.RequestData.PageSize);

        var eventLog = await _eventLogService.Create("CriteriaFeature", "CriteriaFeature", "Criteria_GetListWithPaginationQuery", Guid.Empty);

        return Result<PaginatedList<CriteriaDto>>.Success(result);
    }
}
