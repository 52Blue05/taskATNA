using Sale_Saas.Application.Features.SyllabusFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Application.Features.SyllabusFeature.Queries
{
    public record Syllabus_GetSyllabusByIdQuery(Guid userId, Guid syllabusId) : IRequest<Result<SyllabusAdminDto>>;

    public class Syllabus_GetSyllabusByIdQueryHandler : IRequestHandler<Syllabus_GetSyllabusByIdQuery, Result<SyllabusAdminDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEventLogService _eventLogService;

        public Syllabus_GetSyllabusByIdQueryHandler(IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _eventLogService = eventLogService;
        }

        public async Task<Result<SyllabusAdminDto>> Handle(Syllabus_GetSyllabusByIdQuery request, CancellationToken cancellationToken)
        {
            if(request.syllabusId == null)
                throw new Exception("Dữ liệu gửi đến máy chủ rỗng");

            var getSyllabus = await _context.Syllabus.Include(s => s.SyllabusUnits).ThenInclude(su => su.Unit)
                                                     .Include(s => s.ApplicationUserSyllabus).ThenInclude(us => us.ApplicationUser)
                                                     .Where(s => !s.DeleteFlag && s.Id == request.syllabusId)
                                                     .Select(s => new SyllabusAdminDto
                                                     {
                                                         Id = s.Id,
                                                         Name = s.Name,
                                                         EndTime = s.EndTime,
                                                         ToTalUnit = s.SyllabusUnits.Where(su => su.Unit.DeleteFlag != true).Select(su => su.Unit).Distinct().Count(),
                                                         ToTalUser = s.ApplicationUserSyllabus.Where(asy => asy.DeleteFlag != true && asy.ApplicationUser.DeleteFlag != true)
                                                                                              .Select(asy => asy.ApplicationUser).Distinct().Count()
                                                     }).AsNoTracking().FirstOrDefaultAsync();

            if(getSyllabus == null)
                throw new Exception($"Không thể tìm thấy chương trình học");

            var eventLog = await _eventLogService.Create("SyllabusFeature", "SyllabusFeature",
                                       "Syllabus_GetSyllabusByIdQuery", request.userId);

            return Result<SyllabusAdminDto>.Success(getSyllabus);
        }
    }
}
