using Sale_Saas.Application.Features.BenefitFeature.Dto;
using Sale_Saas.Application.Features.BenefitFeature.Requests;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.BenefitFeature.Commands;

public record Benefit_UpdateEstimateBenefitWithFluctuationCommand(Guid UserId, BenefitUpdateEstimateBenefitWithFluctuationRequest RequestData) : IRequest<Result<BenefitDto>>;

public class Benefit_UpdateEstimateBenefitWithFluctuationCommandHandler : IRequestHandler<Benefit_UpdateEstimateBenefitWithFluctuationCommand, Result<BenefitDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventLogService _eventLogService;
    private readonly IMapper _mapper;

    public Benefit_UpdateEstimateBenefitWithFluctuationCommandHandler(IApplicationDbContext context, IEventLogService eventLogService, IMapper mapper)
    {
        _context = context;
        _eventLogService = eventLogService;
        _mapper = mapper;
    }

    public async Task<Result<BenefitDto>> Handle(Benefit_UpdateEstimateBenefitWithFluctuationCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy user");
        }

        if (request.RequestData.Id == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy quyền lợi hiện tại");
        }

        var item = await _context.Benefits.Where(x => x.DeleteFlag != true && x.Id == request.RequestData.Id).FirstOrDefaultAsync();

        if (item == null)
        {
            throw new ApplicationException("Không tìm thấy quyền lợi hiện tại");
        }

        item.EstimateBenefit = request.RequestData.EstimateBenefit;

        var result = _mapper.Map<BenefitDto>(item);

        return Result<BenefitDto>.Success(result);
    }
}
