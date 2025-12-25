using Sale_Saas.Application.Features.GoalFeature.Services;
using Sale_Saas.Application.Features.TargetFluctuationFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;

namespace Sale_Saas.Application.Features.TargetFluctuationFeature.Commads;

public record TargetFluctuation_AddOrUpdateWithBenefitCommand(Guid BenefitId, List<AddOrUpdateRequest> RequestData) : IRequest<Result<List<TargetFluctuationDto>>>;

public class TargetFluctuation_AddOrUpdateWithBenefitCommandHandler : IRequestHandler<TargetFluctuation_AddOrUpdateWithBenefitCommand, Result<List<TargetFluctuationDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IInternalService _internalService;
    public TargetFluctuation_AddOrUpdateWithBenefitCommandHandler(IMapper mapper,
                                            IApplicationDbContext context,
                                            IInternalService internalService)
    {
        _context = context;
        _mapper = mapper;
        _internalService = internalService;
    }
    public async Task<Result<List<TargetFluctuationDto>>> Handle(TargetFluctuation_AddOrUpdateWithBenefitCommand request, CancellationToken cancellationToken)
    {
        TargetFluctuation? obj = null;
        List<TargetFluctuationDto> updatedSuccess = new List<TargetFluctuationDto>();

        ICollection<TargetFluctuation> listTargetFluctuationRequest = new List<TargetFluctuation>();
        foreach (AddOrUpdateRequest item in request.RequestData)
        {
            TargetFluctuation tempObj = new TargetFluctuation();
            tempObj = (TargetFluctuation)_internalService.MapValueToObject(new TargetFluctuation(), item.Data, tempObj);
            listTargetFluctuationRequest.Add(tempObj);
        }

        CheckDuplicateGoal(listTargetFluctuationRequest);

        var requestIds = request.RequestData.Select(x => x.Id).ToList();

        var listTargetFluctuation = await _context.TargetFluctuations.Where(x => x.DeleteFlag != true
                                                                              && x.BenefitId == request.BenefitId
                                                                              && !requestIds.Contains(x.Id))
                                                                     .ToListAsync();

        foreach (var item in listTargetFluctuation)
        {
            _context.TargetFluctuations.Remove(item);
        }

        await _context.SaveChangesAsync(cancellationToken);

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
            if (obj.BenefitId == null) obj.BenefitId = request.BenefitId;

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

    private static void CheckDuplicateGoal(ICollection<TargetFluctuation>? targetFluctuations)
    {
        if (targetFluctuations == null || targetFluctuations.Count <= 0)
        {
            return;
        }

        // Check for duplicates
        var duplicateGoalIds = targetFluctuations.GroupBy(g => g.GoalId)
                                                 .Where(g => g.Count() > 1)
                                                 .Select(g => g.Key)
                                                 .ToList();

        if (duplicateGoalIds.Any())
        {
            var duplicates = string.Join(", ", duplicateGoalIds);
            throw new ApplicationException($"Các mục tiêu bị trùng lặp");
        }
    }
}
