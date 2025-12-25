
using Sale_Saas.Application.Features.UnitFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services;

namespace Sale_Saas.Application.Features.UnitFeature.Commands
{
    public record Unit_DeleteCommand(Guid userId, Guid unitId) : IRequest<Result<UnitDto>>;

    public class Unit_DeleteCommandHandler : IRequestHandler<Unit_DeleteCommand, Result<UnitDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEventLogService _eventLogService;

        public Unit_DeleteCommandHandler(IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _eventLogService = eventLogService;
        }

        public async Task<Result<UnitDto>> Handle(Unit_DeleteCommand request, CancellationToken cancellationToken)
        {
            if (request.unitId == null)
                throw new Exception("Dữ liệu gửi đến máy chủ rỗng");

            var unit = await _context.Units.Where(u => u.Id == request.unitId).AsNoTracking().FirstOrDefaultAsync();

            if (unit == null)
                throw new Exception("Không tìm thấy khóa học");

            unit.DeleteFlag = true;

            _context.Units.Update(unit);
            await _context.SaveChangesAsync(cancellationToken);

            var eventLog = await _eventLogService.Create("UnitsFeature", "UnitsFeature",
                                       "Unit_DeleteCommand", request.userId);

            //UnitDto unitDto = new UnitDto
            //{
            //    Id = unit.Id,
            //    Name = unit.Name,
            //    EndTime = unit.EndTime,
            //    ThumbnailPath = unit.ThumbnailPath
            //};

            return Result<UnitDto>.Success(null);
        }
    }
}
