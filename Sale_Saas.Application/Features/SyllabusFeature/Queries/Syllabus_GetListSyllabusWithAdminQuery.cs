using Sale_Saas.Application.Features.SyllabusFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.SyllabusFeature.Queries
{
    public record Syllabus_GetListSyllabusWithAdminQuery(Guid userId, GetAllQueryRequest data) : IRequest<Result<List<SyllabusAdminDto>>>;

    public class Syllabus_GetListSyllabusByAdminQueryHandler : IRequestHandler<Syllabus_GetListSyllabusWithAdminQuery, Result<List<SyllabusAdminDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEventLogService _eventLogService;

        public Syllabus_GetListSyllabusByAdminQueryHandler(IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _eventLogService = eventLogService;
        }

        public async Task<Result<List<SyllabusAdminDto>>> Handle(Syllabus_GetListSyllabusWithAdminQuery request, CancellationToken cancellationToken)
        {
            if (request.userId == Guid.Empty)
            {
                throw new ApplicationException("Không tìm thấy người dùng");
            }

            var syllabusQuery = _context.Syllabus.Include(s => s.SyllabusUnits).ThenInclude(su => su.Unit)
                                                 .Include(s => s.ApplicationUserSyllabus).ThenInclude(us => us.ApplicationUser)
                                                 .Where(s => !s.DeleteFlag)
                                                 .Select(s => new SyllabusAdminDto
                                                 {
                                                     Id = s.Id,
                                                     Name = s.Name,
                                                     ToTalUnit = s.SyllabusUnits.Where(su => su.Unit.DeleteFlag != true
                                                                                          && su.DeleteFlag != true)
                                                                                .Select(su => su.Unit)
                                                                                .Distinct()
                                                                                .Count(),
                                                     ToTalUser = s.ApplicationUserSyllabus.Where(asy => asy.DeleteFlag != true
                                                                                                     && asy.ApplicationUser.DeleteFlag != true)
                                                                                          .Select(asy => asy.ApplicationUser)
                                                                                          .Distinct()
                                                                                          .Count(),
                                                     EndTime = s.EndTime
                                                 });

            if (!string.IsNullOrEmpty(request.data.TextSearch))
            {
                var textSearchLower = request.data.TextSearch.ToLower();
                syllabusQuery = syllabusQuery.Where(sy => sy.Name.ToLower().Contains(textSearchLower));
            }

            var syllabusAdminDtos = await syllabusQuery.AsNoTracking().ToListAsync();

            var eventLog = await _eventLogService.Create("SyllabusFeature", "SyllabusFeature",
                                       "Syllabus_GetListSyllabusByAdminQuery", request.userId);

            return Result<List<SyllabusAdminDto>>.Success(syllabusAdminDtos);
        }
    }
}
