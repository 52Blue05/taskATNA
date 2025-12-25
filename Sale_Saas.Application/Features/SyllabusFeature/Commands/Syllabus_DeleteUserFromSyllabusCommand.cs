using Sale_Saas.Application.Features.SyllabusFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Application.Features.SyllabusFeature.Commands
{
    public record Syllabus_DeleteUserFromSyllabusCommand(Guid UserId, Guid Id, Guid ApplicationUserId) : IRequest<Result<SyllabusAdminDto>>;

    public class Syllabus_DeleteUserFromSyllabusCommandHandler : IRequestHandler<Syllabus_DeleteUserFromSyllabusCommand, Result<SyllabusAdminDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEventLogService _eventLogService;

        public Syllabus_DeleteUserFromSyllabusCommandHandler(IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _eventLogService = eventLogService;
        }

        public async Task<Result<SyllabusAdminDto>> Handle(Syllabus_DeleteUserFromSyllabusCommand request, CancellationToken cancellationToken)
        {
            var applicationUserSyllabus = await _context.ApplicationUserSyllabus
                                                    .Where(sy => sy.SyllabusId == request.Id
                                                            && sy.ApplicationUserId == request.ApplicationUserId
                                                            && sy.DeleteFlag != true)
                                                    .FirstOrDefaultAsync();

            if (applicationUserSyllabus == null)
                throw new Exception($"Không thể tìm thấy dữ liệu");

            applicationUserSyllabus.DeleteFlag = true;
            applicationUserSyllabus.LastModifiedDate = DateTime.Now;
            applicationUserSyllabus.LastModifiedApplicationUserId = request.UserId;

            await _context.SaveChangesAsync(cancellationToken);

            var eventLog = await _eventLogService.Create("SyllabusFeature", "SyllabusFeature",
                                                   "Syllabus_DeleteUserFromSyllabusCommand", request.UserId);

            return Result<SyllabusAdminDto>.Success(null);
        }
    }
}
