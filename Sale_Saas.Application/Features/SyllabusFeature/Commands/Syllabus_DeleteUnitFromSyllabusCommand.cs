using Sale_Saas.Application.Features.SyllabusFeature.Dto;
using Sale_Saas.Application.Features.UnitFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.SyllabusFeature.Commands
{
    public record Syllabus_DeleteUnitFromSyllabusCommand(Guid userId, DeleteUnitFromSyllabus data) : IRequest<Result<UnitDto>>;

    public class Syllabus_DeleteUnitFromSyllabusCommandHandler : IRequestHandler<Syllabus_DeleteUnitFromSyllabusCommand, Result<UnitDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEventLogService _eventLogService;

        public Syllabus_DeleteUnitFromSyllabusCommandHandler(IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _eventLogService = eventLogService;
        }

        public async Task<Result<UnitDto>> Handle(Syllabus_DeleteUnitFromSyllabusCommand request, CancellationToken cancellationToken)
        {
            if (request.data == null)
                throw new Exception("Dữ liệu gửi đến máy chủ rỗng");

            Syllabus syllabus = await _context.Syllabus.Where(sy => sy.Id == request.data.SyllabusId && sy.DeleteFlag != true).AsNoTracking().FirstOrDefaultAsync();

            if (syllabus == null)
                throw new Exception($"Không thể tìm thấy chương trình học");

            Units unit = await _context.Units.Where(u => u.Id == request.data.UnitId && u.DeleteFlag != true).AsNoTracking().FirstOrDefaultAsync();

            if (unit == null)
                throw new Exception($"Không thể tìm thấy khóa học");

            var syllabusUnit = await _context.SyllabusUnits.Where(su => su.SyllabusId == request.data.SyllabusId && su.UnitId == request.data.UnitId && su.DeleteFlag != true)
                                                            .AsNoTracking().FirstOrDefaultAsync();

            if (syllabusUnit == null)
                throw new Exception($"Không thể tìm thấy dữ liệu giữa chương trình học và khóa học");

            syllabusUnit.DeleteFlag = true;

            _context.SyllabusUnits.Update(syllabusUnit);
            await _context.SaveChangesAsync(cancellationToken);

            // handle re-sort
            var listSyllabusUnit = await _context.SyllabusUnits.Where(x => x.DeleteFlag != true && x.SyllabusId == request.data.SyllabusId)
                                                               .OrderBy(x => x.SortOrder)
                                                               .ToListAsync();

            const int INITIAL_SORT_ORDER = 1;
            var index = INITIAL_SORT_ORDER;

            foreach (var item in listSyllabusUnit)
            {
                item.SortOrder = index;
                index = index + 1;
            }

            _context.SyllabusUnits.UpdateRange(listSyllabusUnit);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<UnitDto>.Success(null);
        }
    }
}
