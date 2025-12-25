using Sale_Saas.Application.Features.EmployeeSalaryDetailFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.EmployeeSalaryDetailFeature.Commands
{
    public record EmployeeSalaryDetail_AddOrUpdateCommand(Guid userId, List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<EmployeeSalaryDetailDto>>>;

    public class EmployeeSalaryDetail_AddOrUpdateCommandHandler : IRequestHandler<EmployeeSalaryDetail_AddOrUpdateCommand, Result<List<EmployeeSalaryDetailDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;
        private readonly IEventLogService _eventLogService;

        public EmployeeSalaryDetail_AddOrUpdateCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;
            _eventLogService = eventLogService;
        }

        public async Task<Result<List<EmployeeSalaryDetailDto>>> Handle(EmployeeSalaryDetail_AddOrUpdateCommand request, CancellationToken cancellationToken)
        {
            EmployeeSalaryDetail? obj = null;
            List<EmployeeSalaryDetailDto> updatedSuccess = new List<EmployeeSalaryDetailDto>();

            foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
            {
                if (addOrUpdateRequest.Data == null)
                {
                    throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
                }

                if (addOrUpdateRequest.Id == null)
                {
                    obj = new EmployeeSalaryDetail()
                    {
                        CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId
                    };
                }
                else
                {
                    obj = await _context.EmployeeSalaryDetails.FindAsync(addOrUpdateRequest.Id.Value);

                    if (obj == null)
                        throw new ApplicationException($"Không tìm thấy Chức vụ có id: {addOrUpdateRequest.Id.Value}");

                }

                obj = (EmployeeSalaryDetail)_internalService.MapValueToObject(new EmployeeSalaryDetail(), addOrUpdateRequest.Data, obj);
                obj.LastModifiedDate = DateTime.Now;
                obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;

                if (addOrUpdateRequest.Id == null)
                {
                    _context.EmployeeSalaryDetails.Add(obj);
                }
                else
                {
                    _context.EmployeeSalaryDetails.Update(obj);
                }

                updatedSuccess.Add(new EmployeeSalaryDetailDto() { Id = obj.Id });
            }

            var eventLog = await _eventLogService.Create("EmployeeSalaryDetailFeature", "EmployeeSalaryDetailFeature",
                           "EmployeeSalaryDetail_AddOrUpdateCommand", request.userId);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<List<EmployeeSalaryDetailDto>>.Success(updatedSuccess);
        }
    }
}
