using Sale_Saas.Application.Features.BenefitFeature.Dto;
using Sale_Saas.Application.Features.BenefitFeature.Requests;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.BenefitFeature.Commands;


public record BenefitMobile_UpdateEstimatedBenefitCommand(Guid UserId, BenefitMobileUpdateEstimateBenefitRequest RequestData) : IRequest<Result<BenefitDto>>;

public class BenefitMobile_UpdateEstimatedBenefitCommandHandler : IRequestHandler<BenefitMobile_UpdateEstimatedBenefitCommand, Result<BenefitDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventLogService _eventLogService;
    private readonly IMapper _mapper;

    public BenefitMobile_UpdateEstimatedBenefitCommandHandler(IApplicationDbContext context, IEventLogService eventLogService, IMapper mapper)
    {
        _context = context;
        _eventLogService = eventLogService;
        _mapper = mapper;
    }

    public async Task<Result<BenefitDto>> Handle(BenefitMobile_UpdateEstimatedBenefitCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy user");
        }

        if (request.RequestData.IdQuyenLoi == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy quyền lợi hiện tại");
        }

        var item = await _context.Benefits.Where(x => x.DeleteFlag != true && x.Id == request.RequestData.IdQuyenLoi).FirstOrDefaultAsync();

        if (item == null)
        {
            throw new ApplicationException("Không tìm thấy quyền lợi hiện tại");
        }

        item.EstimateBenefit = request.RequestData.QuyenLoiTamTinh;

        var result = _mapper.Map<BenefitDto>(item);

        return Result<BenefitDto>.Success(result);
    }
}
