using Sale_Saas.Application.Features.BenefitFeature.Dto;
using Sale_Saas.Application.Features.BenefitFeature.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.Benefit;
using Sale_Saas.Domain.Enums;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Sale_Saas.Application.Features.BenefitFeature.Commands
{
    public record Benefit_UpdateStatusByIdCommand(Guid userId, UpdateStatusBenefitRequest RequestData) : IRequest<Result<BenefitDto>>;
    public class Benefit_UpdateStatusByIdCommandHandler : IRequestHandler<Benefit_UpdateStatusByIdCommand, Result<BenefitDto>>
    {

        private readonly IApplicationDbContext _context;
        private readonly IApplicationUserService _userService;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;

        public Benefit_UpdateStatusByIdCommandHandler(IMapper mapper, IApplicationDbContext context,
                                                            IApplicationUserService userService, IEventLogService eventLogService)
        {
            _context = context;
            _mapper = mapper;
            _userService = userService;
            _eventLogService = eventLogService;
        }

        public async Task<Result<BenefitDto>> Handle(Benefit_UpdateStatusByIdCommand request, CancellationToken cancellationToken)
        {
            var data = await BenefitService.GetBenefit(request.RequestData.Id, _context);
            var status = await BenefitService.GetStatus(request.RequestData.Status, _context);

            Validate(request);
            if(Enum.Parse<BenefitStatusEnum>(request.RequestData.Status ?? "") == BenefitStatusEnum.REQUEST )
            {
                var ids = await _userService.GetUserIdByRolePosition(RolePositionEnum.ADMINISTRATOR.ToString());
                if(ids.Contains(request.userId))
                {
                    request.RequestData.Status = BenefitStatusEnum.UPDATED.ToString();
                }
            }

            switch (Enum.Parse<BenefitStatusEnum>(request.RequestData.Status ?? ""))
            {
                case BenefitStatusEnum.CONFIRMED:
                    data.SuggestMonthlySalary = null;
                    data.SuggestTargetSalary = null;
                    data.SuggestTotalSalary = null;

                    if (data.BenefitStatus!.Code == BenefitStatusEnum.REQUEST.ToString())
                    {
                        status = await BenefitService.GetStatus(BenefitStatusEnum.CONFIRMED.ToString(), _context);
                    }
                    if (data.BenefitStatus!.Code == BenefitStatusEnum.PENDING.ToString())
                    {
                        status = await BenefitService.GetStatus(BenefitStatusEnum.CONFIRMED.ToString(), _context);
                    }
                    break;
                case BenefitStatusEnum.REQUEST:
                    if (request.RequestData.SuggestMonthlySalary > 0) data.SuggestMonthlySalary = request.RequestData.SuggestMonthlySalary ?? data.SuggestMonthlySalary;
                    if (request.RequestData.SuggestTargetSalary > 0) data.SuggestTargetSalary = request.RequestData.SuggestTargetSalary ?? data.SuggestTargetSalary;
                    if (request.RequestData.SuggestTotalSalary > 0) data.SuggestTotalSalary = request.RequestData.SuggestTotalSalary ?? data.SuggestTotalSalary;

                    status = await BenefitService.GetStatus(BenefitStatusEnum.REQUEST.ToString(), _context);
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
                    if (data.BenefitStatus!.Code == BenefitStatusEnum.PENDING.ToString())
                    {
                        status = await BenefitService.GetStatus(BenefitStatusEnum.UPDATED.ToString(), _context);
                    }

                    break;
                default:
                    break;
            }
            await BenefitService.AddHistory(data.Id, data.BenefitStatusId, status.Id, request.userId, _context);
            data.BenefitStatus = status;
            data.LastModifiedDate = DateTime.Now;
            data.LastModifiedApplicationUserId = request.RequestData.ApplicationUserId;

            await _eventLogService.Create("BenefitFeature", "BenefitFeature", "Benefit_UpdateStatusByIdCommand", request.userId);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<BenefitDto>.Success(_mapper.Map<BenefitDto>(data));
        }

        private void Validate(Benefit_UpdateStatusByIdCommand request)
        {
            if (request.RequestData.SuggestMonthlySalary > 0 && request.RequestData.SuggestMonthlySalary > decimal.MaxValue)
                throw new ApplicationException($"Độ dài kí tự không hợp lệ, tối đa chỉ {decimal.MaxValue} kí tự");
            if (request.RequestData.SuggestTargetSalary > 0 && request.RequestData.SuggestTargetSalary > decimal.MaxValue)
                throw new ApplicationException($"Độ dài kí tự không hợp lệ, tối đa chỉ {decimal.MaxValue} kí tự");
            if (request.RequestData.SuggestTotalSalary > 0 && request.RequestData.SuggestTotalSalary > decimal.MaxValue)
                throw new ApplicationException($"Độ dài kí tự không hợp lệ, tối đa chỉ {decimal.MaxValue} kí tự");
        }
    }
}
