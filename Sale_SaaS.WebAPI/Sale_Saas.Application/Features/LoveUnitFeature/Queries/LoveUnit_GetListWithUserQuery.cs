using Sale_Saas.Application.Features.UnitFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.LoveUnitFeature.Queries
{
    public record LoveUnit_GetListWithUserQuery(Guid userId) : IRequest<Result<List<UnitDto>>>;

    public class LoveUnit_GetListWithUserQueryHandler : IRequestHandler<LoveUnit_GetListWithUserQuery, Result<List<UnitDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEventLogService _eventLogService;

        public LoveUnit_GetListWithUserQueryHandler(IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _eventLogService = eventLogService;
        }

        public async Task<Result<List<UnitDto>>> Handle(LoveUnit_GetListWithUserQuery request, CancellationToken cancellationToken)
        {
            var loveUnits = await (from user in _context.ApplicationUsers
                                   join loveUnit in _context.LoveUnits on user.Id equals loveUnit.ApplicationUserId
                                   join unit in _context.Units on loveUnit.UnitId equals unit.Id
                                   where user.DeleteFlag != true && unit.DeleteFlag != true && loveUnit.DeleteFlag != true && loveUnit.ApplicationUserId == request.userId
                                   select new UnitDto
                                   {
                                       Id = unit.Id,
                                       Name = unit.Name,
                                       ThumbnailPath = unit.ThumbnailPath,
                                   }).AsNoTracking().ToListAsync();

            var eventLog = await _eventLogService.Create("UnitFeature", "UnitFeature",
                                       "Unit_GetListUnitWithAdminQuery", request.userId);

            return Result<List<UnitDto>>.Success(loveUnits);
        }
    }
}
