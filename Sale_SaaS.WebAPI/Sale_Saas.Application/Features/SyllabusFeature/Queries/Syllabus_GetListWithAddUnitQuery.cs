using Sale_Saas.Application.Features.SyllabusFeature.Dto;
using Sale_Saas.Application.Features.UnitFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Application.Features.SyllabusFeature.Queries
{
    public record Syllabus_GetListWithAddUnitQuery(Guid userId, Guid syllabusId) : IRequest<Result<List<ListUnitToSyllabusDto>>>;

    public class Syllabus_GetListWithAddUnitQueryHandler : IRequestHandler<Syllabus_GetListWithAddUnitQuery, Result<List<ListUnitToSyllabusDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEventLogService _eventLogService;

        public Syllabus_GetListWithAddUnitQueryHandler(IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _eventLogService = eventLogService;
        }

        public async Task<Result<List<ListUnitToSyllabusDto>>> Handle(Syllabus_GetListWithAddUnitQuery request, CancellationToken cancellationToken)
        {
            if (request.syllabusId == Guid.Empty)
                throw new Exception("Dữ liệu gửi đến máy chủ rỗng");

            var listResult = await (from unit in _context.Units
                                    where unit.DeleteFlag != true && !(
                                        from syllabusUnit in _context.SyllabusUnits
                                        where syllabusUnit.SyllabusId == request.syllabusId && syllabusUnit.DeleteFlag != true
                                        select syllabusUnit.UnitId
                                    ).Contains(unit.Id)
                                    select new
                                    {
                                        unit.Id,
                                        unit.Name
                                    }).AsNoTracking().ToListAsync();

            var listUnitToSyllabusDtos = listResult.Select(item => new ListUnitToSyllabusDto
            {
                UnitId = item.Id,
                Name = item.Name
            }).ToList();


            var eventLog = await _eventLogService.Create("SyllabusFeature", "SyllabusFeature",
                                       "Syllabus_GetListWithAddUnitQuery", request.userId);

            return Result<List<ListUnitToSyllabusDto>>.Success(listUnitToSyllabusDtos);
        }
    }
}
