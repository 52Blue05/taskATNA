using Sale_Saas.Application.Features.ProjectStatusFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;

namespace Sale_Saas.Application.Features.ProjectStatusFeature.Commands
{
    public record ProjectStatus_AddOrUpdateCommand(List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<ProjectStatusDto>>>;

    public class ProjectStatus_AddOrUpdateCommandHandler : IRequestHandler<ProjectStatus_AddOrUpdateCommand, Result<List<ProjectStatusDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;

        public ProjectStatus_AddOrUpdateCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;

        }

        public async Task<Result<List<ProjectStatusDto>>> Handle(ProjectStatus_AddOrUpdateCommand request, CancellationToken cancellationToken)
        {
            ProjectStatus? obj = null;
            List<ProjectStatusDto> updatedSuccess = new List<ProjectStatusDto>();

            foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
            {
                if (addOrUpdateRequest.Data == null)
                {
                    throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
                }

                if (addOrUpdateRequest.Id == null)
                {
                    obj = new ProjectStatus()
                    {
                        CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId
                    };
                }
                else
                {
                    obj = await _context.ProjectStatuses.FindAsync(addOrUpdateRequest.Id.Value);

                    if (obj == null)
                        throw new ApplicationException($"Không tìm thấy Chức vụ có id: {addOrUpdateRequest.Id.Value}");

                }

                obj = (ProjectStatus)_internalService.MapValueToObject(new ProjectStatus(), addOrUpdateRequest.Data, obj);
                obj.LastModifiedDate = DateTime.Now;
                obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;

                if (addOrUpdateRequest.Id == null)
                {
                    _context.ProjectStatuses.Add(obj);
                }
                else
                {
                    _context.ProjectStatuses.Update(obj);
                }

                updatedSuccess.Add(new ProjectStatusDto() { Id = obj.Id });
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result<List<ProjectStatusDto>>.Success(updatedSuccess);
        }
    }
}
