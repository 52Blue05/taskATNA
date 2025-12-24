using Sale_Saas.Application.Features.BenefitHistoryFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.BenefitHistoryFeature.Commands;

public record BenefitHistory_AddOrUpdateCommand(Guid userId, List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<BenefitHistoryDto>>>;

public class BenefitHistory_AddOrUpdateCommandHandler : IRequestHandler<BenefitHistory_AddOrUpdateCommand, Result<List<BenefitHistoryDto>>>
{
	private readonly IApplicationDbContext _context;
	private readonly IMapper _mapper;
	private readonly IInternalService _internalService;
    private readonly IEventLogService _eventLogService;

    public BenefitHistory_AddOrUpdateCommandHandler(IMapper mapper,
											IApplicationDbContext context,
											IInternalService internalService, 
											IEventLogService eventLogService)
	{
		_context = context;
		_mapper = mapper;
		_internalService = internalService;
		_eventLogService = eventLogService;
	}

	public async Task<Result<List<BenefitHistoryDto>>> Handle(BenefitHistory_AddOrUpdateCommand request, CancellationToken cancellationToken)
	{
		BenefitHistory? obj = null;
		List<BenefitHistoryDto> updatedSuccess = new List<BenefitHistoryDto>();

		foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
		{
			if (addOrUpdateRequest.Data == null)
			{
				throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
			}

			if (addOrUpdateRequest.Id == null)
			{
				obj = new BenefitHistory()
				{
					CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId
				};
			}
			else
			{
				obj = await _context.BenefitHistories.FindAsync(addOrUpdateRequest.Id.Value);

				if (obj == null)
					throw new ApplicationException($"Không tìm thấy Lịch sử có id: {addOrUpdateRequest.Id.Value}");

			}

			obj = (BenefitHistory)_internalService.MapValueToObject(new BenefitHistory(), addOrUpdateRequest.Data, obj);
			obj.LastModifiedDate = DateTime.Now;
			obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;

			if (addOrUpdateRequest.Id == null)
			{
				_context.BenefitHistories.Add(obj);
			}
			else
			{
				_context.BenefitHistories.Update(obj);
			}

			updatedSuccess.Add(new BenefitHistoryDto() { Id = obj.Id });
		}

        var eventLog = await _eventLogService.Create("BenefitHistoryFeature", "BenefitHistoryFeature",
                                                            "BenefitHistory_AddOrUpdateCommand", request.userId);

        await _context.SaveChangesAsync(cancellationToken);

		return Result<List<BenefitHistoryDto>>.Success(updatedSuccess);
	}
}
