using Sale_Saas.Application.Features.ProjectFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Models.Project;
using Sale_Saas.Domain.Constants.API;

namespace Sale_Saas.Application.Features.ProjectFeature.Commands
{
    public record Project_AddOrUpdateCommand(List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<ProjectDto>>>;

    public class Project_AddOrUpdateCommandHandler : IRequestHandler<Project_AddOrUpdateCommand, Result<List<ProjectDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;

        public Project_AddOrUpdateCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;

        }

        public async Task<Result<List<ProjectDto>>> Handle(Project_AddOrUpdateCommand request, CancellationToken cancellationToken)
        {
            Project? obj = null;
            List<ProjectDto> updatedSuccess = new List<ProjectDto>();

            foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
            {
                var tmp = new Project();
                if (addOrUpdateRequest.Data == null)
                {
                    throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
                }
				if (string.IsNullOrEmpty(StringHelper.DictGetValue(addOrUpdateRequest.Data, "Code")))
				{
					throw new ApplicationException($"Mã dữ liệu không thể để trống");
				}

				StringHelper.IsValidLength(StringInfoConstant.NameLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "Code"), true);
				StringHelper.IsValidLength(StringInfoConstant.NameLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "Name"), true);
				StringHelper.IsValidLength(StringInfoConstant.DesLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "Result"), true);
				StringHelper.IsValidLength(StringInfoConstant.DesLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "Type"), true);
				StringHelper.IsValidLength(StringInfoConstant.DesLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "Note"), true);
				StringHelper.IsValidLength(StringInfoConstant.DesLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "Service"), true);

				if (addOrUpdateRequest.Id == null)
                {
                    obj = new Project()
                    {
                        CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId
                    };
                }
                else
                {
                    obj = await _context.Projects.Include(s => s.ProjectStatus)
                                        .Include(s => s.ApplicationUser).Include(s => s.Contract)
                                        .FirstOrDefaultAsync(s => s.Id == addOrUpdateRequest.Id.Value);

                    if (obj == null)
                        throw new ApplicationException($"Không tìm thấy Chức vụ có id: {addOrUpdateRequest.Id.Value}");

                }

                obj = (Project)_internalService.MapValueToObject(new Project(), addOrUpdateRequest.Data, obj);
                obj.LastModifiedDate = DateTime.Now;
                obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;
                /*obj.ProjectStatusId = tmp.ProjectStatusId ?? obj.ProjectStatusId;
                obj.ApplicationUserId = tmp.ApplicationUserId ?? obj.ApplicationUserId;
                obj.ContractId = tmp.ContractId ?? obj.ContractId;*/

                if (obj.ProjectStatusId != null)
                {
                    var exist = await _context.ProjectStatuses.FindAsync(obj.ProjectStatusId);
                    if (exist == null) throw new ApplicationException($"Không tìm thấy Trạng thái có id: {obj.ProjectStatusId}");
                    obj.ProjectStatus = exist;
                    obj.ProjectStatusId = exist.Id;
                }
                if (obj.ApplicationUserId != null)
                {
                    var exist = await _context.ApplicationUsers.FindAsync(obj.ApplicationUserId);
                    if (exist == null) throw new ApplicationException($"Không tìm thấy Nhân sự có id: {obj.ApplicationUserId}");
                    obj.ApplicationUser = exist;
                    obj.ApplicationUserId = exist.Id;
                }
                if (obj.ContractId != null)
                {
                    var exist = await _context.Contracts.FindAsync(obj.ContractId);
                    if (exist == null) throw new ApplicationException($"Không tìm thấy Hợp đồng có id: {obj.ContractId}");
                    obj.Contract = exist;
                    obj.ContractId = exist.Id;
                }

				var query = _context.Projects.Where(s => s.DeleteFlag != true && s.Code == obj.Code).AsNoTracking();
				if (addOrUpdateRequest.Id == null)
                {
                    _context.Projects.Add(obj);
                }
                else
                {
					query = query.Where(s => s.Id != obj.Id);
					_context.Projects.Update(obj);
                }
				var duplicate = await query.CountAsync();
				if (duplicate > 0) throw new ApplicationException($"Mã dữ liệu đã được sử dụng {obj.Code}");

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
