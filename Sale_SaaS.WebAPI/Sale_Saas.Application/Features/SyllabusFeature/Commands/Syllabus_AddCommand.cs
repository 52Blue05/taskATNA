using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.SyllabusFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Domain.Constants.API;

namespace Sale_Saas.Application.Features.SyllabusFeature.Commands
{
    public record Syllabus_AddCommand(Guid userId, AddSyllabus data) : IRequest<Result<SyllabusAdminDto>>;

    public class Syllabus_AddCommandHandler : IRequestHandler<Syllabus_AddCommand, Result<SyllabusAdminDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEventLogService _eventLogService;

        public Syllabus_AddCommandHandler(IApplicationDbContext context, IEventLogService eventLogService)
        {
            _context = context;
            _eventLogService = eventLogService;
        }

        public async Task<Result<SyllabusAdminDto>> Handle(Syllabus_AddCommand request, CancellationToken cancellationToken)
        {
            if (request.data == null)
                throw new Exception("Dữ liệu gửi đến máy chủ rỗng");

            if(string.IsNullOrEmpty(request.data.Name) && request.data.Name.Length < 0)
                throw new Exception($"Độ dài {request.data.Name} không hợp lệ");

            if(request.data.EndTime != null && request.data.EndTime <= DateTime.Now)
                throw new Exception("Ngày kết thúc không thể nhỏ hơn hoặc bằng ngày hiện tại");

            var checkSyllabus = await _context.Syllabus.Where(s => s.Name.ToLower() == request.data.Name.ToLower()).AsNoTracking().FirstOrDefaultAsync();

            if (checkSyllabus != null)
                throw new Exception("Tên chương trình học đã được sử dụng");

            Syllabus syllabus = new Syllabus
            {
                Id = Guid.NewGuid(),
                Name = request.data.Name,
                EndTime = request.data.EndTime,
                DeleteFlag = false,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
                CreatedApplicationUserId = request.userId,
                LastModifiedApplicationUserId = request.userId,
            };

           _context.Syllabus.Add(syllabus);

            await _context.SaveChangesAsync(cancellationToken);

            var eventLog = await _eventLogService.Create("SyllabusFeature", "SyllabusFeature",
                                                   "Syllabus_AddCommand", request.userId);

            SyllabusAdminDto syllabusAdminDto = new SyllabusAdminDto
            {
                Id = syllabus.Id,
                Name = syllabus.Name,
                ToTalUnit = 0,
                ToTalUser = 0
            };

            return Result<SyllabusAdminDto>.Success(syllabusAdminDto);
        }
    }
}
