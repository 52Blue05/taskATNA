using Sale_Saas.Application.Features.SyllabusFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.SyllabusFeature.Queries
{
    public record Syllabus_GetListSyllabusWithUserQuery(Guid userId, GetAllQueryRequest data) : IRequest<Result<List<SyllabusUserDto>>>;

    public class Syllabus_GetListSyllabusWithUserQueryHandler : IRequestHandler<Syllabus_GetListSyllabusWithUserQuery, Result<List<SyllabusUserDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEventLogService _eventLogService;

        public Syllabus_GetListSyllabusWithUserQueryHandler(IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _eventLogService = eventLogService;
        }

        public async Task<Result<List<SyllabusUserDto>>> Handle(Syllabus_GetListSyllabusWithUserQuery request, CancellationToken cancellationToken)
        {
            var syllabusQuery = await _context.Syllabus.Include(s => s.SyllabusUnits).ThenInclude(su => su.Unit)
                                                       .Include(s => s.ApplicationUserSyllabus).ThenInclude(us => us.ApplicationUser)
                                                       .Where(s => !s.DeleteFlag
                                                                && s.SyllabusUnits.Any(su => su.Unit.DeleteFlag != true && su.DeleteFlag != true)
                                                                && s.ApplicationUserSyllabus.Any(asy => asy.ApplicationUserId == request.userId && asy.DeleteFlag != true))
                                                       .Select(s => new SyllabusUserDto
                                                       {
                                                           Id = s.Id,
                                                           Name = s.Name,
                                                           EndTime = s.EndTime,
                                                           ToTalUnit = s.SyllabusUnits.Where(su => su.Unit.DeleteFlag != true
                                                                                                && su.DeleteFlag != true)
                                                                                      .Select(su => su.Unit)
                                                                                      .Distinct()
                                                                                      .Count(),
                                                           ToTalLesson = s.SyllabusUnits.Where(su => su.Unit.DeleteFlag != true
                                                                                                  && su.DeleteFlag != true)
                                                                                        .SelectMany(su => su.Unit.Lessions.Where(l => !l.DeleteFlag))
                                                                                        .Count(),
                                                           IsQualified=false
                                                       })
                                                       .AsNoTracking()
                                                       .ToListAsync();

            var eventLog = await _eventLogService.Create("SyllabusFeature", "SyllabusFeature",
                                       "Syllabus_GetListSyllabusByAdminQuery", request.userId);

            return Result<List<SyllabusUserDto>>.Success(syllabusQuery);
        }
    }
}
