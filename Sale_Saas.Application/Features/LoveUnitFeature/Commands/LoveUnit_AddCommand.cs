
using Sale_Saas.Application.Features.SyllabusFeature.Dto;
using Sale_Saas.Application.Features.UnitFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.LoveUnitFeature.Commands
{
    public record LoveUnit_AddCommand(Guid userId, Guid unitId) : IRequest<Result<UnitDetailDto>>;

    public class LoveUnit_AddCommandHandler : IRequestHandler<LoveUnit_AddCommand, Result<UnitDetailDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEventLogService _eventLogService;

        public LoveUnit_AddCommandHandler(IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _eventLogService = eventLogService;
        }

        public async Task<Result<UnitDetailDto>> Handle(LoveUnit_AddCommand request, CancellationToken cancellationToken)
        {
            if (request.unitId == null)
                throw new Exception("Dữ liệu gửi đến máy chủ rỗng");

            var unit = await _context.Units.Where(u => u.Id == request.unitId).AsNoTracking().FirstOrDefaultAsync();

            if (unit == null)
                throw new Exception("Không thể tìm thấy khóa học");

            LoveUnits loveUnit = new LoveUnits
            {
                Id = Guid.NewGuid(),
                ApplicationUserId = request.userId,
                UnitId = request.unitId,
                DeleteFlag = false,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
                CreatedApplicationUserId = request.userId,
                LastModifiedApplicationUserId = request.userId,
            };

            _context.LoveUnits.Add(loveUnit);

            await _context.SaveChangesAsync(cancellationToken);

            var eventLog = await _eventLogService.Create("LoveUnitFeature", "LoveUnitFeature",
                                                   "LoveUnit_AddCommand", request.userId);

            UnitDetailDto syllabusDetailDto = new UnitDetailDto
            {
                Id = loveUnit.Id,
                Name = unit.Name,
                EndTime = unit.EndTime,
                ThumbnailPath = unit.ThumbnailPath,
                Description = unit.Description,
                IsLove = true
            };

            return Result<UnitDetailDto>.Success(syllabusDetailDto);
        }
    }
}
