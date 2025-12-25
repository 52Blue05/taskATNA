using Sale_Saas.Application.Features.UnitFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Models.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Application.Features.SyllabusFeature.Queries
{
    public record Syllabus_GetListWithAddUserQuery(Guid userId, Guid syllabusId) : IRequest<Result<List<string>>>;

    public class Syllabus_GetListUserForSyllabusQueryHandler : IRequestHandler<Syllabus_GetListWithAddUserQuery, Result<List<string>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEventLogService _eventLogService;

        public Syllabus_GetListUserForSyllabusQueryHandler(IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _eventLogService = eventLogService;
        }

        public async Task<Result<List<string>>> Handle(Syllabus_GetListWithAddUserQuery request, CancellationToken cancellationToken)
        {
            if (request.syllabusId == null)
                throw new Exception("Dữ liệu gửi đến máy chủ rỗng");

            var listResult = await (from user in _context.ApplicationUsers
                                    where user.DeleteFlag != true && !(
                                        from applicationUserSyllabus in _context.ApplicationUserSyllabus
                                        where applicationUserSyllabus.SyllabusId == request.syllabusId && applicationUserSyllabus.DeleteFlag != true
                                        select applicationUserSyllabus.ApplicationUserId
                                    ).Contains(user.Id)
                                    select new
                                    {
                                        user.Email
                                    }).AsNoTracking().ToListAsync();

            var listGetListUserForSyllabusDtos = listResult.Select(item => item.Email).ToList();


            var eventLog = await _eventLogService.Create("SyllabusFeature", "SyllabusFeature",
                                       "Syllabus_GetListUserForSyllabusQuery", request.userId);

            return Result<List<string>>.Success(listGetListUserForSyllabusDtos);
        }
    }
}
