using Sale_Saas.Application.Features.UnitFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.UnitFeature.Queries
{
    public record Unit_GetListUnitWithAdminQuery(Guid userId, GetAllQueryRequest data) : IRequest<Result<List<UnitDto>>>;

    public class Unit_GetListUnitWithAdminQueryHandler : IRequestHandler<Unit_GetListUnitWithAdminQuery, Result<List<UnitDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEventLogService _eventLogService;

        public Unit_GetListUnitWithAdminQueryHandler(IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _eventLogService = eventLogService;
        }

        public async Task<Result<List<UnitDto>>> Handle(Unit_GetListUnitWithAdminQuery request, CancellationToken cancellationToken)
        {
            var unitsQuery = _context.Units.Where(u => u.DeleteFlag != true)
                                                .Select(u => new UnitDto
                                                {
                                                    Id = u.Id,
                                                    Name = u.Name,
                                                    EndTime = u.EndTime,
                                                    ThumbnailPath = u.ThumbnailPath
                                                });

            if (!string.IsNullOrEmpty(request.data.TextSearch))
            {
                var textSearchLower = request.data.TextSearch.ToLower();
                unitsQuery = unitsQuery.Where(sy => sy.Name.ToLower().Contains(textSearchLower));
            }

            var listUnitsQuery = await unitsQuery.AsNoTracking().ToListAsync();

            var eventLog = await _eventLogService.Create("UnitFeature", "UnitFeature",
                                       "Unit_GetListUnitWithAdminQuery", request.userId);

            return Result<List<UnitDto>>.Success(listUnitsQuery);
        }
    }
}