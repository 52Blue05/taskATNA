using Sale_Saas.Application.Features.BenefitFeature.Dto;
using Sale_Saas.Application.Features.BenefitFeature.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.Benefit;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.BenefitFeature.Commands;

public record BenefitMobile_UpdateStatusCommand(Guid userId, UpdateStatusBenefitRequest RequestData) : IRequest<Result<BenefitDto>>;
public class BenefitMobile_UpdateStatusCommandHandler : IRequestHandler<BenefitMobile_UpdateStatusCommand, Result<BenefitDto>>
{

    private readonly IApplicationDbContext _context;
    private readonly IApplicationUserService _userService;
    private readonly IMapper _mapper;
    private readonly IEventLogService _eventLogService;

    public BenefitMobile_UpdateStatusCommandHandler(IMapper mapper, IApplicationDbContext context,
                                                        IApplicationUserService userService, IEventLogService eventLogService)
    {
        _context = context;
        _mapper = mapper;
        _userService = userService;
        _eventLogService = eventLogService;
    }

    public async Task<Result<BenefitDto>> Handle(BenefitMobile_UpdateStatusCommand request, CancellationToken cancellationToken)
    {
        var data = await BenefitService.GetBenefit(request.RequestData.Id, _context);
        var status = await BenefitService.MobileGetStatus(request.RequestData.Status ?? "", _context);

        Validate(request);

        switch (Enum.Parse<BenefitStatusEnum>(request.RequestData.Status ?? ""))
        {
            case BenefitStatusEnum.REJECT:
                data.SuggestMonthlySalary = null;
                data.SuggestTargetSalary = null;
                data.SuggestTotalSalary = null;

                status = await BenefitService.GetStatus(BenefitStatusEnum.CONFIRMED.ToString(), _context);

                break;

            case BenefitStatusEnum.CONFIRMED:
                if (data.SuggestMonthlySalary > 0)
                {
                    data.MonthlySalary = data.SuggestMonthlySalary;
                    data.SuggestMonthlySalary = null;
                }

                if (data.SuggestTargetSalary > 0)
                {
                    data.TargetSalary = data.SuggestTargetSalary;
                    data.SuggestTargetSalary = null;
                }

                if (data.SuggestTotalSalary > 0)
                {
                    data.TotalSalary = data.SuggestTotalSalary;
                    data.SuggestTotalSalary = null;
                }

                break;

            case BenefitStatusEnum.REQUEST:
                if (request.RequestData.SuggestMonthlySalary > 0) data.SuggestMonthlySalary = request.RequestData.SuggestMonthlySalary ?? data.SuggestMonthlySalary;
                if (request.RequestData.SuggestTargetSalary > 0) data.SuggestTargetSalary = request.RequestData.SuggestTargetSalary ?? data.SuggestTargetSalary;
                if (request.RequestData.SuggestTotalSalary > 0) data.SuggestTotalSalary = request.RequestData.SuggestTotalSalary ?? data.SuggestTotalSalary;

                break;

            case BenefitStatusEnum.UPDATED:
                if (request.RequestData.SuggestTotalSalary > 0)
                {
                    data.TotalSalary = request.RequestData.SuggestTotalSalary ?? data.TotalSalary;
                    data.SuggestMonthlySalary = null;
                }
                if (request.RequestData.SuggestTargetSalary > 0)
                {
                    data.TargetSalary = request.RequestData.SuggestTargetSalary ?? data.TargetSalary;
                    data.SuggestTargetSalary = null;
                }
                if (request.RequestData.SuggestMonthlySalary > 0)
                {
                    data.MonthlySalary = request.RequestData.SuggestMonthlySalary ?? data.MonthlySalary;
                    data.SuggestTotalSalary = null;
                }
                if (data.BenefitStatus!.Code == BenefitStatusEnum.REQUEST.ToString())
                {
                    status = await BenefitService.GetStatus(BenefitStatusEnum.CONFIRMED.ToString(), _context);
                }

                break;

            default:
                break;
        }

        await BenefitService.AddHistory(data.Id, data.BenefitStatusId, status.Id, request.userId, _context);
        data.BenefitStatus = status;
        data.LastModifiedDate = DateTime.Now;
        data.LastModifiedApplicationUserId = request.RequestData.ApplicationUserId;

        await _eventLogService.Create("BenefitFeature", "BenefitFeature", "BenefitMobile_UpdateStatusCommand", request.userId);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<BenefitDto>.Success(_mapper.Map<BenefitDto>(data));
    }

    private void Validate(BenefitMobile_UpdateStatusCommand request)
    {
        if (request.RequestData.SuggestMonthlySalary > 0 && request.RequestData.SuggestMonthlySalary > decimal.MaxValue)
            throw new ApplicationException($"Độ dài kí tự không hợp lệ, tối đa chỉ {decimal.MaxValue} kí tự");
        if (request.RequestData.SuggestTargetSalary > 0 && request.RequestData.SuggestTargetSalary > decimal.MaxValue)
            throw new ApplicationException($"Độ dài kí tự không hợp lệ, tối đa chỉ {decimal.MaxValue} kí tự");
        if (request.RequestData.SuggestTotalSalary > 0 && request.RequestData.SuggestTotalSalary > decimal.MaxValue)
            throw new ApplicationException($"Độ dài kí tự không hợp lệ, tối đa chỉ {decimal.MaxValue} kí tự");
    }
}
