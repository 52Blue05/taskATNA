
using Sale_Saas.Application.Features.UnitFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.UnitFeature.Queries
{
    public record Unit_GetUnitById(Guid userId, Guid unitId) : IRequest<Result<UnitDetailDto>>;

    public class Unit_GetUnitByIdHandler : IRequestHandler<Unit_GetUnitById, Result<UnitDetailDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEventLogService _eventLogService;

        public Unit_GetUnitByIdHandler(IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _eventLogService = eventLogService;
        }

        public async Task<Result<UnitDetailDto>> Handle(Unit_GetUnitById request, CancellationToken cancellationToken)
        {
            if (request.unitId == null)
                throw new Exception("Dữ liệu gửi đến máy chủ rỗng");

            var unitDetailDto = await _context.Units.Where(u => u.Id == request.unitId && u.DeleteFlag != true)
                                           .Select(u => new UnitDetailDto
                                           {
                                               Id = u.Id,
                                               Name = u.Name,
                                               EndTime = u.EndTime,
                                               ThumbnailPath = u.ThumbnailPath,
                                               Description = u.Description
                                           }).AsNoTracking().FirstOrDefaultAsync();

            var eventLog = await _eventLogService.Create("UnitsFeature", "UnitsFeature",
                           "Unit_GetUnitById", request.userId);
            if(unitDetailDto!=null)
             unitDetailDto.IsLove = await _context.LoveUnits.Where(lu => lu.UnitId == request.unitId && lu.ApplicationUserId == request.userId && lu.DeleteFlag != true)
                                                            .AsNoTracking().FirstOrDefaultAsync() == null ? false : true;

            return Result<UnitDetailDto>.Success(unitDetailDto);
        }
    }
}
