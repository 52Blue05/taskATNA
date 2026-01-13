using Sale_Saas.Application.Features.ProjectFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Models.Project;
using Sale_Saas.Domain.Constants.API;

namespace Sale_Saas.Application.Features.ProjectFeature.Commands
{
    public record Project_UpdateResultCommand(List<ProjectUpdateResultRequest> RequestData) : IRequest<Result<List<ProjectDto>>>;

    public class Project_UpdateResultCommandHandler : IRequestHandler<Project_UpdateResultCommand, Result<List<ProjectDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;

        public Project_UpdateResultCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;

        }

        public async Task<Result<List<ProjectDto>>> Handle(Project_UpdateResultCommand request, CancellationToken cancellationToken)
        {
            List<ProjectDto> updatedSuccess = new List<ProjectDto>();
            foreach (var addOrUpdateRequest in request.RequestData)
            {
                if (addOrUpdateRequest.Data == null)
                {
                    throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ");
                }

				StringHelper.IsValidLength(StringInfoConstant.DesLimit, addOrUpdateRequest.Data.Result, true);
				StringHelper.IsValidLength(StringInfoConstant.DesLimit, addOrUpdateRequest.Data.Note, true);

				var obj = await _context.Projects.Include(s => s.ApplicationUser).Include(s => s.ProjectStatus)
                                .FirstOrDefaultAsync(s => s.Id == addOrUpdateRequest.Id && !s.DeleteFlag);
                if (obj == null)
                    throw new ApplicationException($"Không tìm thấy Dự án với mã: {addOrUpdateRequest.Id}");

                obj.Result = addOrUpdateRequest.Data.Result ?? obj.Result;
                obj.Point = addOrUpdateRequest.Data.Point ?? obj.Point;
                obj.Note = addOrUpdateRequest.Data.Note ?? obj.Note;
                obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId ?? obj.LastModifiedApplicationUserId;

                if (addOrUpdateRequest.Data.ProjectStatusId != null)
                {
                    var status = await _context.ProjectStatuses.FirstOrDefaultAsync(s => s.Id == addOrUpdateRequest.Data.ProjectStatusId);
                    if (status == null) throw new ApplicationException($"Không tìm thấy Trạng thái với mã: {addOrUpdateRequest.Data.ProjectStatusId}");
                    obj.ProjectStatusId = status.Id;
                    obj.ProjectStatus = status;
                }

                updatedSuccess.Add(_mapper.Map<ProjectDto>(obj));
            }

            await _context.SaveChangesAsync(cancellationToken);

            foreach (var item in updatedSuccess)
            {
                var serviveName = await _context.Services.Where(s => s.Id == Guid.Parse(item.Service) && s.DeleteFlag != true)
                                                         .Select(s => s.Name).AsNoTracking().FirstOrDefaultAsync();

                item.Service = serviveName ?? "";
            }

            return Result<List<ProjectDto>>.Success(updatedSuccess);
        }
    }
}
