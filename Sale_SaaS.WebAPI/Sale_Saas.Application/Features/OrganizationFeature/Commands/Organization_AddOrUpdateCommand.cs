using Sale_Saas.Application.Features.OrganizationFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;

namespace Sale_Saas.Application.Features.OrganizationFeature.Commands
{
    public record Organization_AddOrUpdateCommand(List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<OrganizationDto>>>;

    public class Organization_AddOrUpdateCommandHandler : IRequestHandler<Organization_AddOrUpdateCommand, Result<List<OrganizationDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;

        public Organization_AddOrUpdateCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;

        }

        public async Task<Result<List<OrganizationDto>>> Handle(Organization_AddOrUpdateCommand request, CancellationToken cancellationToken)
        {
            Organization? obj = null;
            List<OrganizationDto> updatedSuccess = new List<OrganizationDto>();

            foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
            {
                if (addOrUpdateRequest.Data == null)
                {
                    throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
                }

                if (addOrUpdateRequest.Id == null)
                {
                    obj = new Organization()
                    {
                        CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId
                    };
                }
                else
                {
                    obj = await _context.Organizations.FindAsync(addOrUpdateRequest.Id.Value);

                    if (obj == null)
                        throw new ApplicationException($"Không tìm thấy Chức vụ có id: {addOrUpdateRequest.Id.Value}");

                }

                obj = (Organization)_internalService.MapValueToObject(new Organization(), addOrUpdateRequest.Data, obj);
                obj.LastModifiedDate = DateTime.Now;
                obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;

                if (addOrUpdateRequest.Id == null)
                {
                    _context.Organizations.Add(obj);
                }
                else
                {
                    _context.Organizations.Update(obj);
                }

                updatedSuccess.Add(_mapper.Map<OrganizationDto>(obj));
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result<List<OrganizationDto>>.Success(updatedSuccess);
        }
    }
}
