using Sale_Saas.Application.Features.UnitFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Application.Features.LoveUnitFeature.Commands
{
    public record LoveUnit_DeleteCommand(Guid userId, Guid unitId) : IRequest<Result<UnitDetailDto>>;

    public class LoveUnit_DeleteCommandHandler : IRequestHandler<LoveUnit_DeleteCommand, Result<UnitDetailDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEventLogService _eventLogService;

        public LoveUnit_DeleteCommandHandler(IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _eventLogService = eventLogService;
        }

        public async Task<Result<UnitDetailDto>> Handle(LoveUnit_DeleteCommand request, CancellationToken cancellationToken)
        {
            if (request.unitId == null)
                throw new Exception("Dữ liệu gửi đến máy chủ rỗng");

            var unit = await _context.Units.Where(u => u.Id == request.unitId).AsNoTracking().FirstOrDefaultAsync();

            if (unit == null)
                throw new Exception("Không thể tìm thấy khóa học");

            var loveUnit = await _context.LoveUnits.Where(lu => lu.UnitId == request.unitId && lu.ApplicationUserId == request.userId && lu.DeleteFlag != true)
                                                   .AsNoTracking().FirstOrDefaultAsync();

            loveUnit.DeleteFlag = true;

            _context.LoveUnits.Update(loveUnit);

            await _context.SaveChangesAsync(cancellationToken);

            var eventLog = await _eventLogService.Create("LoveUnitFeature", "LoveUnitFeature",
                                                   "LoveUnit_DeleteCommandHandler", request.userId);

            UnitDetailDto syllabusDetailDto = new UnitDetailDto
            {
                Id = loveUnit.Id,
                Name = unit.Name,
                EndTime = unit.EndTime,
                ThumbnailPath = unit.ThumbnailPath,
                Description = unit.Description,
                IsLove = false
            };

            return Result<UnitDetailDto>.Success(syllabusDetailDto);
        }
    }
}
