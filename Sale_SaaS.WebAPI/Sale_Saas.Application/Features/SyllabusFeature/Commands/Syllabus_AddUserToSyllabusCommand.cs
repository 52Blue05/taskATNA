
using Sale_Saas.Application.Features.SyllabusFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Domain.Entities;
using System.Collections.Generic;

namespace Sale_Saas.Application.Features.SyllabusFeature.Commands
{
    public record Syllabus_AddUserToSyllabusCommand(Guid userId, AddUserToSyllabus data) : IRequest<Result<List<SyllabusAdminUserDto>>>;

    public class Syllabus_AddUserToSyllabusCommandHandler : IRequestHandler<Syllabus_AddUserToSyllabusCommand, Result<List<SyllabusAdminUserDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEventLogService _eventLogService;

        public Syllabus_AddUserToSyllabusCommandHandler(IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _eventLogService = eventLogService;
        }

        public async Task<Result<List<SyllabusAdminUserDto>>> Handle(Syllabus_AddUserToSyllabusCommand request, CancellationToken cancellationToken)
        {
            if (request.data == null || request.data.listEmailUser.Count <= 0)
                throw new Exception("Dữ liệu gửi đến máy chủ rỗng");

            Syllabus syllabus = await _context.Syllabus.Where(sy => sy.Id == request.data.syllabusId).FirstOrDefaultAsync();

            if (syllabus == null)
                throw new Exception($"Không thể tìm thấy chương trình học");

            List<SyllabusAdminUserDto> listResult = new List<SyllabusAdminUserDto>();

            foreach (var item in request.data.listEmailUser)
            {
                var user = await _context.ApplicationUsers.Where(user => user.Email == item && user.DeleteFlag != true).AsNoTracking().FirstOrDefaultAsync();

                if(user == null)
                    throw new Exception($"Không thể tìm thấy người dùng với email {item}");

                ApplicationUserSyllabus applicationUserSyllabus = new ApplicationUserSyllabus
                {
                    Id = Guid.NewGuid(),
                    SyllabusId = syllabus.Id,
                    ApplicationUserId = user.Id,
                    DeleteFlag = false,
                    CreatedDate = DateTime.Now,
                    LastModifiedDate = DateTime.Now,
                    CreatedApplicationUserId = request.userId,
                    LastModifiedApplicationUserId = request.userId,
                };

                _context.ApplicationUserSyllabus.Add(applicationUserSyllabus);

                SyllabusAdminUserDto tempSyllabusAdminUserDto = new SyllabusAdminUserDto
                {
                    Id = applicationUserSyllabus.Id,
                    FullName = user.FullName,
                    Email = user.Email
                };

                listResult.Add(tempSyllabusAdminUserDto);
            }

            await _context.SaveChangesAsync(cancellationToken);

            var eventLog = await _eventLogService.Create("SyllabusFeature", "SyllabusFeature",
                                                   "Syllabus_AddUserToSyllabusCommand", request.userId);

            int countUnit = await _context.Syllabus.Where(s => s.Id == syllabus.Id && s.DeleteFlag != true)
                                       .SelectMany(s => s.SyllabusUnits)
                                       .Select(su => su.Unit)
                                       .CountAsync();

            if(listResult.Count > 0)
            {
                SyllabusAdminResult syllabusAdminResult = new SyllabusAdminResult
                {
                    Status = "Chưa hoàn thành"
                };

                List<SyllabusAdminResult> syllabusAdminResults = Enumerable.Repeat(syllabusAdminResult, countUnit).ToList();

                foreach (var item in listResult)
                {
                    item.listSyllabusResults.AddRange(syllabusAdminResults);
                }
            }

            return Result<List<SyllabusAdminUserDto>>.Success(listResult);
        }
    }
}
