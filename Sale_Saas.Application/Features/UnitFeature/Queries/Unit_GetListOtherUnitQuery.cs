using Sale_Saas.Application.Features.UnitFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.UnitFeature.Queries
{
    public record Unit_GetListOtherUnitQuery(Guid userId) : IRequest<Result<List<UnitDto>>>;

    public class Unit_GetListOtherUnitQueryHandler : IRequestHandler<Unit_GetListOtherUnitQuery, Result<List<UnitDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEventLogService _eventLogService;

        public Unit_GetListOtherUnitQueryHandler(IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _eventLogService = eventLogService;
        }

        public async Task<Result<List<UnitDto>>> Handle(Unit_GetListOtherUnitQuery request, CancellationToken cancellationToken)
        {
            var listMain = await _context.Units.Select(u => u.Id).ToListAsync();

            var listUnit = await _context.Syllabus.Include(s => s.SyllabusUnits).ThenInclude(su => su.Unit)
                                                  .Include(s => s.ApplicationUserSyllabus).ThenInclude(us => us.ApplicationUser)
                                                       .Where(s => !s.DeleteFlag
                                                                && s.SyllabusUnits.Select(su => su.Unit.DeleteFlag != true).Count() > 0
                                                                && s.ApplicationUserSyllabus.Any(asy => asy.ApplicationUserId == request.userId && asy.DeleteFlag != true))
                                                  .SelectMany(s => s.SyllabusUnits.Select(su => su.Unit.Id))
                                                  .Distinct().ToListAsync();

            if (listUnit != null)
            {
                var listNoExistSyllabus = listMain.Except(listUnit);

                var existedLoveUnit = await _context.LoveUnits.Where(lu => lu.ApplicationUserId == request.userId && lu.DeleteFlag != true)
                                                              .Select(lu => lu.UnitId).ToListAsync();

                if (listNoExistSyllabus.Any() && existedLoveUnit.Any())
                {
                    var listFinal = listNoExistSyllabus.Except(existedLoveUnit);
                    return Result<List<UnitDto>>.Success(ConvertListUnitToListUnitDto(listFinal));
                }

                return Result<List<UnitDto>>.Success(ConvertListUnitToListUnitDto(listNoExistSyllabus));
            }

            return Result<List<UnitDto>>.Success(ConvertListUnitToListUnitDto(listMain));
        }

        public List<UnitDto> ConvertListUnitToListUnitDto(IEnumerable<Guid> listUnitId)
        {
            if (listUnitId == null)
            {
                return null;
            }

            var listUnits = _context.Units.Where(u => listUnitId.Contains(u.Id)).ToList();

            List<UnitDto> listUnitDtos = new List<UnitDto>();

            foreach (var unit in listUnits)
            {
                UnitDto unitDto = new UnitDto
                {
                    Id = unit.Id,
                    Name = unit.Name,
                    ThumbnailPath = unit.ThumbnailPath
                };

                listUnitDtos.Add(unitDto);
            }

            return listUnitDtos;
        }
    }
}
