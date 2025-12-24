using Sale_Saas.Application.Features.OpportunityFeature.Dto;
using Sale_Saas.Application.Features.OpportunityFeature.Services;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.OpportunityFeature.Commands;

public record OpportunityMobile_UpdateToOtherCommand(Guid UserId, UpdateToOtherRequest RequestData) : IRequest<Result<GetOpportunityDetailMobileResponse>>;

public class OpportunityMobile_UpdateToOtherCommandHandler : IRequestHandler<OpportunityMobile_UpdateToOtherCommand, Result<GetOpportunityDetailMobileResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IInternalService _internalService;
    private readonly IFeaturePermissionService _permissionService;
    private readonly IApplicationUserService _applicationUserService;

    public OpportunityMobile_UpdateToOtherCommandHandler(IApplicationDbContext context, IMapper mapper, IInternalService internalService, IFeaturePermissionService permissionService, IApplicationUserService applicationUserService)
    {
        _context = context;
        _mapper = mapper;
        _internalService = internalService;
        _permissionService = permissionService;
        _applicationUserService = applicationUserService;
    }

    public async Task<Result<GetOpportunityDetailMobileResponse>> Handle(OpportunityMobile_UpdateToOtherCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy user");
        }

        // check permission
        await _permissionService.HasPermission(MenuType.Sale_CH, FeatureType.CREATE, request.UserId, true);

        var opportunity = await _context.Opportunities
            .Where(x => x.DeleteFlag != true && x.Id == request.RequestData.IdCoHoi)
            .Include(x => x.OpportunityOpponents)
            .Include(x => x.OpportunityStatus)
            .Include(x => x.Customer)
            .Include(x => x.OpportunityHistories)
            .Include(x => x.ApplicationUser)
            .FirstOrDefaultAsync();

        if (opportunity == null)
        {
            throw new ApplicationException("Không tìm thấy cơ hội");
        }

        if (request.RequestData.IdTrangThai != null && request.RequestData.IdTrangThai != Guid.Empty)
        {
            var status = await OpportunityService.GetStatusById(request.RequestData.IdTrangThai ?? Guid.Empty, _context);
            opportunity.OpportunityStatus = status;
            opportunity.OpportunityStatusId = status.Id;
        }
        else
        {
            if (opportunity.OpportunityStatus.Code == OpportunityStatusEnum.PENDING.ToString())
            {
                var status = await OpportunityService.GetStatus(OpportunityStatusEnum.ACTIVE.ToString(), _context);
                opportunity.OpportunityStatusId = status.Id;
                opportunity.OpportunityStatus = status;
            }
        }

        opportunity.ApplicationUserId = request.RequestData.IdNhanVien ?? opportunity.ApplicationUserId;
        opportunity.ApplicationUser = await _applicationUserService.FindAsync(opportunity.ApplicationUserId);
        opportunity.OpportunityStartDate = request.RequestData.NgayBatDau ?? opportunity.OpportunityStartDate;
        opportunity.OpportunityEndDate = request.RequestData.NgayKetThuc ?? opportunity.OpportunityEndDate;
        opportunity.Reason = request.RequestData.LyDo ?? opportunity.Reason;

        opportunity.LastModifiedApplicationUserId = request.UserId;
        opportunity.LastModifiedDate = DateTime.Now;

        List<GetOpportunityOpponentDetailResponse> listOpponentMap = new List<GetOpportunityOpponentDetailResponse>();
        List<GetOpportunityHistoryDetailResponse> listOpportunityHistoryMap = new List<GetOpportunityHistoryDetailResponse>();

        if (opportunity.OpportunityOpponents != null && opportunity.OpportunityOpponents.Count > 0)
        {
            listOpponentMap = opportunity.OpportunityOpponents.Where(x => x.DeleteFlag != true)
                                                              .Select(x => new GetOpportunityOpponentDetailResponse()
                                                              {
                                                                  Id = x.Id,
                                                                  TenDoiThu = x.Name,
                                                                  DiemManh = x.Strength,
                                                                  DiemYeu = x.Weakness,
                                                              })
                                                              .ToList();
        }

        if (opportunity.OpportunityHistories != null && opportunity.OpportunityHistories.Count > 0)
        {
            listOpportunityHistoryMap = opportunity.OpportunityHistories.Where(x => x.DeleteFlag != true)
                                                                        .Select(x => new GetOpportunityHistoryDetailResponse()
                                                                        {
                                                                            Id = x.Id,
                                                                            MucTieu = x.Goal,
                                                                            HoatDong = x.Activity,
                                                                            ThoiDiem = x.Time,
                                                                            KetQua = x.Result,
                                                                        })
                                                                        .ToList();
        }

        _context.Opportunities.Update(opportunity);

        await _context.SaveChangesAsync(cancellationToken);

        GetOpportunityDetailMobileResponse result = new()
        {
            IdCoHoi = opportunity.Id,
            IdKhachHang = opportunity.Customer.Id,
            MaKhachHang = opportunity.Customer.Code,
            TenKhachHang = opportunity.Customer.Fullname,
            IdNhanVien = opportunity.ApplicationUser.Id,
            TenNhanVien = opportunity.ApplicationUser.FullName,
            NhuCau = opportunity.Need,
            NguoiQuyetDinh = opportunity.Accountable,
            NguoiPhuTrachKiThuat = opportunity.TechnicalLead,
            NguoiThuHuong = opportunity.Beneficiary,
            NgayBatDau = opportunity.OpportunityStartDate,
            NgayKetThuc = opportunity.OpportunityEndDate,
            TongTien = opportunity.TotalMoney.ToString(),
            KhaNangThang = opportunity.WinningOppotunity,
            LoaiTien = opportunity.TypeMoney,
            DanhSachDoiThu = listOpponentMap,
            LyDo = opportunity.Reason,
            ChienLuoc = opportunity.Strategy,
            QuyDoi = opportunity.CurrencyConversion.ToString(),
            VonDuKien = opportunity.EstimatedMoney.ToString(),
            TienHoaHong = opportunity.CommissionMoney.ToString(),
            DanhSachLichSuTuongTac = listOpportunityHistoryMap,
            TrangThai = new TrangThaiResponse
            {
                Id = opportunity.OpportunityStatus.Id,
                Code = opportunity.OpportunityStatus.Code,
                Name = opportunity.OpportunityStatus.Name
            }
        };

        return Result<GetOpportunityDetailMobileResponse>.Success(result);
    }
}

