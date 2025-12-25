using Sale_Saas.Application.Features.UnitFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Sale_Saas.Application.Features.UnitFeature.Commands
{
    public record Unit_UpdateCommand(Guid userId, UpdateUnit data) : IRequest<Result<UnitDto>>;

    public class Unit_UpdateCommandHandler : IRequestHandler<Unit_UpdateCommand, Result<UnitDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEventLogService _eventLogService;
        private readonly IFileStorageService _fileStorageService;

        public Unit_UpdateCommandHandler(IApplicationDbContext context, IEventLogService eventLogService, IFileStorageService fileStorageService)
        {
            _context = context;
            _eventLogService = eventLogService;
            _fileStorageService = fileStorageService;
        }

        public async Task<Result<UnitDto>> Handle(Unit_UpdateCommand request, CancellationToken cancellationToken)
        {
            if (request.data == null)
                throw new Exception("Dữ liệu gửi đến máy chủ rỗng");

            if (string.IsNullOrEmpty(request.data.Name) && request.data.Name.Length < 0)
                throw new Exception($"Độ dài {request.data.Name} không hợp lệ");

            if (string.IsNullOrEmpty(request.data.Description) && request.data.Description.Length < 0)
                throw new Exception($"Độ dài mô tả khóa học không hợp lệ");

            var unit = await _context.Units.Where(u => u.Id == request.data.Id).AsNoTracking().FirstOrDefaultAsync();

            if (unit == null)
                throw new Exception("Không tìm thấy khóa học");

            if(request.data.Name != null)
            {
                unit.Name = request.data.Name;
            }
           
            if (request.data.Description != null)
            {
                unit.Description = request.data.Description;
            }

            unit.LastModifiedDate = DateTime.Now;
            unit.LastModifiedApplicationUserId = request.userId;

            string thumbnail = "";
            if (request.data.Thumbnail != null)
            {
                try
                {
                    var response = await _fileStorageService.UploadFileAsync(new List<IFormFile>() { request.data.Thumbnail });
                    if (response.Any())
                    {
                        thumbnail = response[0].ServerPath;
                        unit.ThumbnailPath = thumbnail;
                    }
                }
                catch (Exception ex) { }
            }
          
            _context.Units.Update(unit);
            await _context.SaveChangesAsync(cancellationToken);

            var eventLog = await _eventLogService.Create("UnitsFeature", "UnitsFeature",
                                       "Units_UpdateCommand", request.userId);

            UnitDto unitDto = new UnitDto
            {
                Id = unit.Id,
                Name = unit.Name,
                EndTime = unit.EndTime,
                ThumbnailPath = unit.ThumbnailPath,
                Description = unit.Description
            };

            return Result<UnitDto>.Success(unitDto);
        }
    }
}
