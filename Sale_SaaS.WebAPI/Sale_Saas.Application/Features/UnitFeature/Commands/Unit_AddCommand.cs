
using Sale_Saas.Application.Features.SyllabusFeature.Dto;
using Sale_Saas.Application.Features.UnitFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.UnitFeature.Commands
{
    public record Unit_AddCommand(Guid userId, AddUnit data) : IRequest<Result<UnitDto>>;

    public class Unit_AddCommandHandler : IRequestHandler<Unit_AddCommand, Result<UnitDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEventLogService _eventLogService;
        private readonly IFileStorageService _fileStorageService;

        public Unit_AddCommandHandler(IApplicationDbContext context, IEventLogService eventLogService, IFileStorageService fileStorageService)
        {
            _context = context;
            _eventLogService = eventLogService;
            _fileStorageService = fileStorageService;
        }

        public async Task<Result<UnitDto>> Handle(Unit_AddCommand request, CancellationToken cancellationToken)
        {
            if (request.data == null)
                throw new Exception("Dữ liệu gửi đến máy chủ rỗng");

            if (string.IsNullOrEmpty(request.data.Name) && request.data.Name.Length < 0)
                throw new Exception($"Độ dài {request.data.Name} không hợp lệ");

            if (string.IsNullOrEmpty(request.data.Description) && request.data.Description.Length < 0)
                throw new Exception($"Độ dài mô tả khóa học không hợp lệ");

            string thumbnail = "";
            if (request.data.Thumbnail != null)
            {
                try
                {
                    var response = await _fileStorageService.UploadFileAsync(new List<IFormFile>() { request.data.Thumbnail });
                    if (response.Any())
                    {
                        thumbnail = response[0].ServerPath;
                    }
                }
                catch (Exception ex) { }
            }

            Units unit = new Units
            {
                Id = Guid.NewGuid(),
                Name = request.data.Name,
                Description = request.data.Description,
                ThumbnailPath = thumbnail,
                DeleteFlag = false,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
                CreatedApplicationUserId = request.userId,
                LastModifiedApplicationUserId = request.userId,
            };

            _context.Units.Add(unit);
            await _context.SaveChangesAsync(cancellationToken);

            var eventLog = await _eventLogService.Create("UnitsFeature", "UnitsFeature",
                                       "Units_AddCommand", request.userId);

            UnitDto unitDto = new UnitDto
            {
                Id= unit.Id,
                Name = unit.Name,
                ThumbnailPath = unit.ThumbnailPath
            };

            return Result<UnitDto>.Success(unitDto);
        }
    }
}
