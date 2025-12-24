using Sale_Saas.Application.Features.GoalFeature.Services;
using Sale_Saas.Application.Features.TargetFluctuationFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;

namespace Sale_Saas.Application.Features.TargetFluctuationFeature.Commads
{
    public record TargetFluctuation_AddOrUpdateCommand(List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<TargetFluctuationDto>>>;

    public class TargetFluctuation_AddOrUpdateCommandHandler : IRequestHandler<TargetFluctuation_AddOrUpdateCommand, Result<List<TargetFluctuationDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IInternalService _internalService;
        public TargetFluctuation_AddOrUpdateCommandHandler(IMapper mapper,
                                                IApplicationDbContext context,
                                                IInternalService internalService)
        {
            _context = context;
            _mapper = mapper;
            _internalService = internalService;
        }
        public async Task<Result<List<TargetFluctuationDto>>> Handle(TargetFluctuation_AddOrUpdateCommand request, CancellationToken cancellationToken)
        {
            TargetFluctuation? obj = null;
            List<TargetFluctuationDto> updatedSuccess = new List<TargetFluctuationDto>();

            foreach (AddOrUpdateRequest addOrUpdateRequest in request.RequestData)
            {
                if (addOrUpdateRequest.Data == null)
                {
                    throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
                }

                if (addOrUpdateRequest.Id == null)
                {
                    obj = new TargetFluctuation()
                    {
                        CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId
                    };
                }
                else
                {
                    obj = await _context.TargetFluctuations.FindAsync(addOrUpdateRequest.Id.Value);

                    if (obj == null)
                        throw new ApplicationException($"Không tìm thấy năm học có id: {addOrUpdateRequest.Id.Value}");

                }

                obj = (TargetFluctuation)_internalService.MapValueToObject(new TargetFluctuation(), addOrUpdateRequest.Data, obj);
                obj.LastModifiedDate = DateTime.Now;
                obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;


                if (addOrUpdateRequest.Id == null)
                {
                    _context.TargetFluctuations.Add(obj);
                }
                else
                {
                    _context.TargetFluctuations.Update(obj);
                }

                updatedSuccess.Add(_mapper.Map<TargetFluctuationDto>(obj));

                updatedSuccess[updatedSuccess.Count - 1].CriteriaName = await GoalService.GetCriteriaName(obj.GoalId ?? Guid.Empty, _context);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result<List<TargetFluctuationDto>>.Success(updatedSuccess);
        }
    }
}
