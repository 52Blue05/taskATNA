using Sale_Saas.Application.Features.BenefitFeature.Dto;
using Sale_Saas.Application.Features.BenefitFeature.Services;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.BenefitFeature.Commands
{
    public record Benefit_UpdateTotalBenefitCommand(Guid userId, List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<BenefitDto>>>;

    public class Benefit_UpdateTotalBenefitCommandHandler : IRequestHandler<Benefit_UpdateTotalBenefitCommand, Result<List<BenefitDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;
        private readonly IEventLogService _eventLogService;

        public Benefit_UpdateTotalBenefitCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService,
                                                IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;
            _eventLogService = eventLogService;
        }

        public async Task<Result<List<BenefitDto>>> Handle(Benefit_UpdateTotalBenefitCommand request, CancellationToken cancellationToken)
        {
            Benefit? obj = null;
            List<BenefitDto> updatedSuccess = new List<BenefitDto>();
            foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
            {
                if (addOrUpdateRequest.Data == null || addOrUpdateRequest.Id == null)
                {
                    throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
                }

				StringHelper.IsNumberOutOfRange(StringHelper.DictGetValue(addOrUpdateRequest.Data, "TotalBenefit"), true);

                obj = await BenefitService.GetBenefit(addOrUpdateRequest.Id.Value, _context);

                BenefitUpdateTotalBenefit dto = new BenefitUpdateTotalBenefit();
                var tmp = (BenefitUpdateTotalBenefit)_internalService.MapValueToObject(new BenefitUpdateTotalBenefit(), addOrUpdateRequest.Data, dto);
                obj.TotalBenefit = tmp.TotalBenefit;
                obj.LastModifiedDate = DateTime.Now;
                obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;

               
                _context.Benefits.Update(obj);

                updatedSuccess.Add(_mapper.Map<BenefitDto>(obj));
            }

            var eventLog = await _eventLogService.Create("BenefitFeature", "BenefitFeature",
                                                            "Benefit_UpdateTotalBenefitCommand", request.userId);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<List<BenefitDto>>.Success(updatedSuccess);
        }
    }
}
