using Sale_Saas.Application.Features.UnitFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.UnitFeature.Queries
{
    public record Unit_GetListBySyllabusIdQuery(Guid UserId, Guid SyllabusId) : IRequest<Result<List<UnitDto>>>;

    public class Unit_GetListBySyllabusIdQueryHandler : IRequestHandler<Unit_GetListBySyllabusIdQuery, Result<List<UnitDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEventLogService _eventLogService;

        public Unit_GetListBySyllabusIdQueryHandler(IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _eventLogService = eventLogService;
        }

        public async Task<Result<List<UnitDto>>> Handle(Unit_GetListBySyllabusIdQuery request, CancellationToken cancellationToken)
        {
            var syllabus = await _context.Syllabus.Where(s => s.Id == request.SyllabusId).AsNoTracking().FirstOrDefaultAsync();
            var query = from unit in _context.Units
                        join su in _context.SyllabusUnits
                        on unit.Id equals su.UnitId into unit_su
                        from su in unit_su.DefaultIfEmpty()
                        where unit.DeleteFlag != true && su.DeleteFlag != true
                           && su.SyllabusId == request.SyllabusId
                        select new { unit, su };

            query = query.OrderBy(x => x.su.SortOrder);

            var results = await query.AsNoTracking()
                                     .Select(x => new UnitDto()
                                     {
                                         Id = x.unit.Id,
                                         Name = x.unit.Name,
                                         EndTime = syllabus.EndTime,
                                         ThumbnailPath = x.unit.ThumbnailPath,
                                         Description = x.unit.Description,
                                         SortOrder = x.su.SortOrder
                                     })
                                     .ToListAsync();

            return Result<List<UnitDto>>.Success(results);
        }
    }
}
