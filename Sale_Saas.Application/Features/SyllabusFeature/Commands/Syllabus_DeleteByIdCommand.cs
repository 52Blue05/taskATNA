using Sale_Saas.Application.Features.SyllabusFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Application.Features.SyllabusFeature.Commands
{
    public record Syllabus_DeleteByIdCommand(Guid userId, Guid syllabusId) : IRequest<Result<SyllabusAdminDto>>;

    public class Syllabus_DeleteByIdCommandHandler : IRequestHandler<Syllabus_DeleteByIdCommand, Result<SyllabusAdminDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEventLogService _eventLogService;

        public Syllabus_DeleteByIdCommandHandler(IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _eventLogService = eventLogService;
        }

        public async Task<Result<SyllabusAdminDto>> Handle(Syllabus_DeleteByIdCommand request, CancellationToken cancellationToken)
        {
            if (request == null)
                throw new Exception("Dữ liệu gửi đến máy chủ rỗng");

            var userSyllabus = await _context.ApplicationUserSyllabus.Where(us => us.SyllabusId == request.syllabusId && us.DeleteFlag == false)
                                                                     .AsNoTracking().FirstOrDefaultAsync();

            if(userSyllabus != null)
                throw new Exception("Không thể xóa vì đã có nhân viên trong chương trình");

            Syllabus syllabus = await _context.Syllabus.Where(sy => sy.Id == request.syllabusId && sy.DeleteFlag != true).FirstOrDefaultAsync();

            if (syllabus == null)
                throw new Exception("Không thể tìm thấy chương trình học");

            syllabus.DeleteFlag = true;

            _context.Syllabus.Update(syllabus);

            await _context.SaveChangesAsync(cancellationToken);

            var eventLog = await _eventLogService.Create("SyllabusFeature", "SyllabusFeature",
                                                   "Syllabus_DeleteByIdCommand", request.userId);

            //var syllabusAdminDto = await _context.Syllabus.Include(s => s.SyllabusUnits).ThenInclude(su => su.Unit)
            //                                         .Include(s => s.ApplicationUserSyllabus).ThenInclude(us => us.ApplicationUser)
            //                                         .Where(s => s.Id == request.syllabusId)
            //                                         .Select(s => new SyllabusAdminDto
            //                                         {
            //                                             Id = s.Id,
            //                                             Name = s.Name,
            //                                             ToTalUnit = s.SyllabusUnits.Select(su => su.Unit).Distinct().Count(),
            //                                             ToTalUser = s.ApplicationUserSyllabus.Select(us => us.ApplicationUser).Distinct().Count()
            //                                         }).AsNoTracking().FirstOrDefaultAsync();

            return Result<SyllabusAdminDto>.Success(null);
        }
    }
}
