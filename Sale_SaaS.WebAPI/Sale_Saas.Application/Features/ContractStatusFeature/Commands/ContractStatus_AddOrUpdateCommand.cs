using Sale_Saas.Application.Features.ContractStatusFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.ContractStatusFeature.Commands
{
    public record ContractStatus_AddOrUpdateCommand(Guid userId, List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<ContractStatusDto>>>;

    public class ContractStatus_AddOrUpdateCommandHandler : IRequestHandler<ContractStatus_AddOrUpdateCommand, Result<List<ContractStatusDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;
        private readonly IEventLogService _eventLogService;

        public ContractStatus_AddOrUpdateCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;
            _eventLogService = eventLogService;
        }

        public async Task<Result<List<ContractStatusDto>>> Handle(ContractStatus_AddOrUpdateCommand request, CancellationToken cancellationToken)
        {
            ContractStatus? obj = null;
            List<ContractStatusDto> updatedSuccess = new List<ContractStatusDto>();

            foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
            {
                if (addOrUpdateRequest.Data == null)
                {
                    throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
                }

                if (addOrUpdateRequest.Id == null)
                {
                    obj = new ContractStatus()
                    {
                        CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId
                    };
                }
                else
                {
                    obj = await _context.ContractStatuses.FindAsync(addOrUpdateRequest.Id.Value);

                    if (obj == null)
                        throw new ApplicationException($"Không tìm thấy Chức vụ có id: {addOrUpdateRequest.Id.Value}");

                }

                obj = (ContractStatus)_internalService.MapValueToObject(new ContractStatus(), addOrUpdateRequest.Data, obj);
                obj.LastModifiedDate = DateTime.Now;
                obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;

                if (addOrUpdateRequest.Id == null)
                {
                    _context.ContractStatuses.Add(obj);
                }
                else
                {
                    _context.ContractStatuses.Update(obj);
                }

                updatedSuccess.Add(new ContractStatusDto() { Id = obj.Id });
            }

            var eventLog = await _eventLogService.Create("ContractStatusFeature", "ContractStatusFeature",
                                                   "ContractStatus_AddOrUpdateCommand", request.userId);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<List<ContractStatusDto>>.Success(updatedSuccess);
        }
    }
}
