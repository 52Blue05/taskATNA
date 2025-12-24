using Sale_Saas.Application.Features.OpportunityFeature.Services;
using Sale_Saas.Application.Features.OpportunityHistoryFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Domain.Constants.API;

namespace Sale_Saas.Application.Features.OpportunityHistoryFeature.Commands
{
    public record OpportunityHistory_AddOrUpdateCommand(List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<OpportunityHistoryDto>>>;

    public class OpportunityHistory_AddOrUpdateCommandHandler : IRequestHandler<OpportunityHistory_AddOrUpdateCommand, Result<List<OpportunityHistoryDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;
		private readonly IApplicationUserService _userService;

		public OpportunityHistory_AddOrUpdateCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService,
												IApplicationUserService userService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;
            _userService = userService;
        }

        public async Task<Result<List<OpportunityHistoryDto>>> Handle(OpportunityHistory_AddOrUpdateCommand request, CancellationToken cancellationToken)
        {
            OpportunityHistory? obj = null;
            List<OpportunityHistoryDto> updatedSuccess = new List<OpportunityHistoryDto>();

            foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
            {
                if (addOrUpdateRequest.Data == null)
                {
                    throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
                }
                Validate(addOrUpdateRequest);

				if (addOrUpdateRequest.Id == null)
                {
                    obj = new OpportunityHistory()
                    {
                        CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId
                    };
                }
                else
                {
                    obj = await OpportunityService.GetHistory(addOrUpdateRequest.Id.Value, _context);
                }

                obj = (OpportunityHistory)_internalService.MapValueToObject(new OpportunityHistory(), addOrUpdateRequest.Data, obj);
                obj.LastModifiedDate = DateTime.Now;
                obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;

				Opportunity opportunity = await OpportunityService.GetOpportunity(obj.OpportunityId,_context);
				ApplicationUser user = await _userService.FindAsync(obj.ApplicationUserId);

                if (addOrUpdateRequest.Id == null)
                {
                    _context.OpportunityHistories.Add(obj);
                }
                else
                {
                    _context.OpportunityHistories.Update(obj);
                }

                updatedSuccess.Add(_mapper.Map<OpportunityHistoryDto>(obj));
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result<List<OpportunityHistoryDto>>.Success(updatedSuccess);
        }

        private void Validate(AddOrUpdateRequest addOrUpdateRequest)
        {
			StringHelper.IsValidLength(StringInfoConstant.DesLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "Goal"), true);
			StringHelper.IsValidLength(StringInfoConstant.DesLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "Activity"), true);
			StringHelper.IsValidLength(StringInfoConstant.DesLimit, StringHelper.DictGetValue(addOrUpdateRequest.Data, "Result"), true);
		}
    }
}
