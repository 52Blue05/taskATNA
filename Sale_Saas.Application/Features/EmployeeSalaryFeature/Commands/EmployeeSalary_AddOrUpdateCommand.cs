using Sale_Saas.Application.Features.EmployeeSalaryFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.EmployeeSalaryFeature.Commands
{
    public record EmployeeSalary_AddOrUpdateCommand(Guid userId, List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<EmployeeSalaryDto>>>;
    public class EmployeeSalary_AddOrUpdateCommandHandler : IRequestHandler<EmployeeSalary_AddOrUpdateCommand, Result<List<EmployeeSalaryDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;
        private readonly IEventLogService _eventLogService;

        public EmployeeSalary_AddOrUpdateCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;
            _eventLogService = eventLogService;
        }

        public async Task<Result<List<EmployeeSalaryDto>>> Handle(EmployeeSalary_AddOrUpdateCommand request, CancellationToken cancellationToken)
        {
            EmployeeSalary? obj = null;
            List<EmployeeSalaryDto> updatedSuccess = new List<EmployeeSalaryDto>();

            foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
            {
                if (addOrUpdateRequest.Data == null)
                {
                    throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
                }

                if (addOrUpdateRequest.Id == null)
                {
                    obj = new EmployeeSalary()
                    {
                        CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId
                    };
                }
                else
                {
                    obj = await _context.EmployeeSalarys.FindAsync(addOrUpdateRequest.Id.Value);

                    if (obj == null)
                        throw new ApplicationException($"Không tìm thấy Chức vụ có id: {addOrUpdateRequest.Id.Value}");

                }

                obj = (EmployeeSalary)_internalService.MapValueToObject(new EmployeeSalary(), addOrUpdateRequest.Data, obj);
                obj.LastModifiedDate = DateTime.Now;
                obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;

                if (addOrUpdateRequest.Id == null)
                {
                    _context.EmployeeSalarys.Add(obj);
                }
                else
                {
                    _context.EmployeeSalarys.Update(obj);
                }

                updatedSuccess.Add(new EmployeeSalaryDto() { Id = obj.Id });
            }

            var eventLog = await _eventLogService.Create("EmployeeSalaryFeature", "EmployeeSalaryFeature",
               "EmployeeSalary_AddOrUpdateCommand", request.userId);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<List<EmployeeSalaryDto>>.Success(updatedSuccess);
        }

    }
}
