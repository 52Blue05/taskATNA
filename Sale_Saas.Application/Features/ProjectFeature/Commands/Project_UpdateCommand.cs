using Sale_Saas.Application.Features.ProjectFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Models.Project;
using Sale_Saas.Domain.Constants.API;

namespace Sale_Saas.Application.Features.ProjectFeature.Commands
{
    public record Project_UpdateCommand(List<ProjectUpdateRequest> RequestData) : IRequest<Result<List<ProjectDto>>>;

    public class Project_UpdateCommandHandler : IRequestHandler<Project_UpdateCommand, Result<List<ProjectDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;

        public Project_UpdateCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;

        }

        public async Task<Result<List<ProjectDto>>> Handle(Project_UpdateCommand request, CancellationToken cancellationToken)
        {
            List<ProjectDto> updatedSuccess = new List<ProjectDto>();
            foreach (var addOrUpdateRequest in request.RequestData)
            {
                if(addOrUpdateRequest.Data == null)
                {
                    throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ");
                }

				StringHelper.IsValidLength(StringInfoConstant.NameLimit, addOrUpdateRequest.Data.Name, true);
				StringHelper.IsValidLength(StringInfoConstant.DesLimit, addOrUpdateRequest.Data.Service, true);
				StringHelper.IsValidLength(StringInfoConstant.DesLimit, addOrUpdateRequest.Data.Type, true);

				var obj = await _context.Projects.Include(s => s.ApplicationUser).Include(s => s.ProjectStatus)
                                .FirstOrDefaultAsync(s => s.Id == addOrUpdateRequest.Id && !s.DeleteFlag);
                if (obj == null)
                    throw new ApplicationException($"Không tìm thấy Dự án với mã: {addOrUpdateRequest.Id}");

                obj.Code = addOrUpdateRequest.Data.Code ?? obj.Code;
                obj.Name = addOrUpdateRequest.Data.Name ?? obj.Name;
                obj.Service = addOrUpdateRequest.Data.Service ?? obj.Service;
                obj.Type = addOrUpdateRequest.Data.Type ?? obj.Type;
                obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId ?? obj.LastModifiedApplicationUserId;

                if(addOrUpdateRequest.Data.ApplicationUserId != null)
                {
                    var user = await _context.ApplicationUsers.FirstOrDefaultAsync(s => s.Id == addOrUpdateRequest.Data.ApplicationUserId);
                    if(user == null) throw new ApplicationException($"Không tìm thấy Tài khoản với mã: {addOrUpdateRequest.Data.ApplicationUserId}");
                    obj.ApplicationUserId = user.Id;
                    obj.ApplicationUser = user;
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
