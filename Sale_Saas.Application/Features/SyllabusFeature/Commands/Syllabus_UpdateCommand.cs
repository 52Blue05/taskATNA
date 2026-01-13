
using Sale_Saas.Application.Features.SyllabusFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.SyllabusFeature.Commands
{
    public record Syllabus_UpdateCommand(Guid userId, UpdateSyllabus data) : IRequest<Result<SyllabusAdminDto>>;

    public class Syllabus_UpdateCommandHandler : IRequestHandler<Syllabus_UpdateCommand, Result<SyllabusAdminDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEventLogService _eventLogService;

        public Syllabus_UpdateCommandHandler(IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _eventLogService = eventLogService;
        }

        public async Task<Result<SyllabusAdminDto>> Handle(Syllabus_UpdateCommand request, CancellationToken cancellationToken)
        {
            if (request.data == null)
                throw new Exception("Dữ liệu gửi đến máy chủ rỗng");

            if (string.IsNullOrEmpty(request.data.Name) && request.data.Name.Length < 0)
                throw new Exception($"Độ dài {request.data.Name} không hợp lệ");

            if (request.data.EndTime != null && request.data.EndTime <= DateTime.Now)
                throw new Exception("Ngày kết thúc không thể nhỏ hơn hoặc bằng ngày hiện tại");

            Syllabus syllabus = await _context.Syllabus.Where(sy => sy.Id == request.data.Id).FirstOrDefaultAsync();

            if (syllabus == null)
                throw new Exception($"Không thể tìm thấy chương trình học {request.data.Name}");

            if(syllabus.Name != request.data.Name)
            {
                var checkSyllabus = await _context.Syllabus.Where(s => s.Name.ToLower() == request.data.Name.ToLower()).AsNoTracking().FirstOrDefaultAsync();

                if (checkSyllabus != null)
                    throw new Exception("Tên chương trình học đã được sử dụng");
            }

            syllabus.EndTime = request.data.EndTime;
            syllabus.Name = request.data.Name;
            syllabus.LastModifiedApplicationUserId = request.userId;
            syllabus.LastModifiedDate = DateTime.Now;

            _context.Syllabus.Update(syllabus);

            //var getListUnit = await (from sy in _context.Syllabus 
            //                         join syUnit in _context.SyllabusUnits on sy.Id equals syUnit.SyllabusId 
            //                         join unit in _context.Units on syUnit.UnitId equals unit.Id 
            //                         select new {
            //                           unit = unit 
            //                         }).AsNoTracking().ToListAsync();

            //if (getListUnit.Any())
            //{
            //    foreach (var item in getListUnit)
            //    {
            //        item.unit.EndTime = syllabus.EndTime;
            //        _context.Units.Update(item.unit);
            //    }              
            //}

            await _context.SaveChangesAsync(cancellationToken);

            var eventLog = await _eventLogService.Create("SyllabusFeature", "SyllabusFeature",
                                                   "Syllabus_UpdateCommand", request.userId);

            var syllabusAdminDto = await _context.Syllabus.Include(s => s.SyllabusUnits).ThenInclude(su => su.Unit)
                                                          .Include(s => s.ApplicationUserSyllabus).ThenInclude(us => us.ApplicationUser)
                                                          .Where(s => !s.DeleteFlag && s.Id == request.data.Id)
                                                          .Select(s => new SyllabusAdminDto
                                                          {
                                                            Id = s.Id,
                                                            Name = s.Name,
                                                            ToTalUnit = s.SyllabusUnits.Select(su => su.Unit).Distinct().Count(),
                                                            ToTalUser = s.ApplicationUserSyllabus.Select(us => us.ApplicationUser).Distinct().Count()
                                                         }).AsNoTracking().FirstOrDefaultAsync();

            return Result<SyllabusAdminDto>.Success(syllabusAdminDto);
        }
    }
}
