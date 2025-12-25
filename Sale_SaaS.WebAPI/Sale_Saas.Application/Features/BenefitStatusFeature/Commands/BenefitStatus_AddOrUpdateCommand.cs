using Sale_Saas.Application.Features.BenefitStatusFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.BenefitStatusFeature.Commands;

public record BenefitStatus_AddOrUpdateCommand(Guid userId, List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<BenefitStatusDto>>>;

public class BenefitStatus_AddOrUpdateCommandHandler : IRequestHandler<BenefitStatus_AddOrUpdateCommand, Result<List<BenefitStatusDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IInternalService _internalService;
    private readonly IEventLogService _eventLogService;

    public BenefitStatus_AddOrUpdateCommandHandler(IMapper mapper,
                                            IApplicationDbContext context,
                                            IInternalService internalService,
                                            IEventLogService eventLogService)
    {
        _context = context;
        _mapper = mapper;
        _internalService = internalService;
        _eventLogService = eventLogService;
    }

    public async Task<Result<List<BenefitStatusDto>>> Handle(BenefitStatus_AddOrUpdateCommand request, CancellationToken cancellationToken)
    {
        BenefitStatus? obj = null;
        List<BenefitStatusDto> updatedSuccess = new List<BenefitStatusDto>();

        foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
        {
            if (addOrUpdateRequest.Data == null)
            {
                throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
            }

            if (addOrUpdateRequest.Id == null)
            {
                obj = new BenefitStatus()
                {
                    CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId
                };
            }
            else
            {
                obj = await _context.BenefitStatuses.FindAsync(addOrUpdateRequest.Id.Value);

                if (obj == null)
                    throw new ApplicationException($"Không tìm thấy Chức vụ có id: {addOrUpdateRequest.Id.Value}");

            }

            obj = (BenefitStatus)_internalService.MapValueToObject(new BenefitStatus(), addOrUpdateRequest.Data, obj);
            obj.LastModifiedDate = DateTime.Now;
            obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;

            if (addOrUpdateRequest.Id == null)
            {
                _context.BenefitStatuses.Add(obj);
            }
            else
            {
                _context.BenefitStatuses.Update(obj);
            }

            updatedSuccess.Add(new BenefitStatusDto() { Id = obj.Id });
        }

        var eventLog = await _eventLogService.Create("BenefitStatusFeature", "BenefitStatusFeature",
                                                    "BenefitStatus_AddOrUpdateCommand", request.userId);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<List<BenefitStatusDto>>.Success(updatedSuccess);
    }
}
