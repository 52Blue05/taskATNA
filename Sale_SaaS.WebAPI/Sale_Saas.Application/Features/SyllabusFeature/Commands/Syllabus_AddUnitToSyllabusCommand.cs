
using Sale_Saas.Application.Features.SyllabusFeature.Dto;
using Sale_Saas.Application.Features.UnitFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.SyllabusFeature.Commands
{
    public record Syllabus_AddUnitToSyllabusCommand(Guid userId, AddUnitToSyllabus data) : IRequest<Result<List<UnitDto>>>;

    public class Syllabus_AddUnitToSyllabusCommandHandler : IRequestHandler<Syllabus_AddUnitToSyllabusCommand, Result<List<UnitDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEventLogService _eventLogService;

        public Syllabus_AddUnitToSyllabusCommandHandler(IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _eventLogService = eventLogService;
        }

        public async Task<Result<List<UnitDto>>> Handle(Syllabus_AddUnitToSyllabusCommand request, CancellationToken cancellationToken)
        {
            if (request.data == null || request.data.ListUnitId.Count <= 0)
                throw new Exception("Dữ liệu gửi đến máy chủ rỗng");

            var syllabus = await _context.Syllabus.Where(sy => sy.Id == request.data.SyllabusId && sy.DeleteFlag != true)
                                                  .AsNoTracking()
                                                  .FirstOrDefaultAsync();

            if (syllabus == null)
                throw new Exception($"Không thể tìm thấy chương trình học");

            List<UnitDto> listUnitDto = new List<UnitDto>();

            foreach (var UnitId in request.data.ListUnitId)
            {
                var unit = await _context.Units.Where(u => u.Id == UnitId && u.DeleteFlag != true)
                                               .AsNoTracking()
                                               .FirstOrDefaultAsync();

                if (unit == null)
                    throw new Exception($"Không thể tìm thấy khóa học");

                SyllabusUnits syllabusUnits = new SyllabusUnits
                {
                    Id = Guid.NewGuid(),
                    UnitId = unit.Id,
                    SyllabusId = syllabus.Id,
                    CreatedDate = DateTime.Now,
                    LastModifiedDate = DateTime.Now,
                    CreatedApplicationUserId = request.userId,
                    LastModifiedApplicationUserId = request.userId,
                    DeleteFlag = false
                };

                _context.SyllabusUnits.Add(syllabusUnits);
                await _context.SaveChangesAsync(cancellationToken);

                UnitDto unitDto = new UnitDto
                {
                    Id = unit.Id,
                    EndTime = syllabus.EndTime,
                    Name = unit.Name,
                    ThumbnailPath = unit.ThumbnailPath
                };

                listUnitDto.Add(unitDto);
            }

            return Result<List<UnitDto>>.Success(listUnitDto);
        }
    }
}
