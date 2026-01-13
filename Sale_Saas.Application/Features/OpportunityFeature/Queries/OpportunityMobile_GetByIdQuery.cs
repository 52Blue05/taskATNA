using Sale_Saas.Application.Features.OpportunityFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;

namespace Sale_Saas.Application.Features.OpportunityFeature.Queries;

public record OpportunityMobile_GetByIdQuery(Guid UserId, Guid OpportunityId) : IRequest<Result<GetOpportunityDetailMobileResponse>>;
public class OpportunityMobile_GetByIdQueryHandler : IRequestHandler<OpportunityMobile_GetByIdQuery, Result<GetOpportunityDetailMobileResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IInternalService _internalService;
    private readonly IFeaturePermissionService _permissionService;

    public OpportunityMobile_GetByIdQueryHandler(IApplicationDbContext context, IMapper mapper, IInternalService internalService, IFeaturePermissionService permissionService)
    {
        _context = context;
        _mapper = mapper;
        _internalService = internalService;
        _permissionService = permissionService;
    }

    public async Task<Result<GetOpportunityDetailMobileResponse>> Handle(OpportunityMobile_GetByIdQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy user");
        }

        var opportunity = await _context.Opportunities.Where(x => x.DeleteFlag != true && x.Id == request.OpportunityId)
                                                      .Include(x => x.OpportunityOpponents)
                                                      .Include(x => x.OpportunityStatus)
                                                      .Include(x => x.Customer)
                                                      .Include(x => x.OpportunityHistories)
                                                      .Include(x => x.ApplicationUser)
                                                      .FirstOrDefaultAsync();

        if (opportunity == null)
        {
            throw new ApplicationException("Không có cơ hội");
        }

        GetOpportunityDetailMobileResponse result = new GetOpportunityDetailMobileResponse()
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
            ChienLuoc = opportunity.Strategy,
            QuyDoi = opportunity.CurrencyConversion.ToString(),
            LyDo = opportunity.Reason,
            VonDuKien = opportunity.EstimatedMoney.ToString(),
            TienHoaHong = opportunity.CommissionMoney.ToString(),
            TrangThai = new TrangThaiResponse()
            {
                Id = opportunity.OpportunityStatus.Id,
                Code = opportunity.OpportunityStatus.Code,
                Name = opportunity.OpportunityStatus.Name
            }
        };

        if (opportunity.OpportunityOpponents != null && opportunity.OpportunityOpponents.Count > 0)
        {
            result.DanhSachDoiThu = opportunity.OpportunityOpponents.Where(x => x.DeleteFlag != true)
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
            result.DanhSachLichSuTuongTac = opportunity.OpportunityHistories.Where(x => x.DeleteFlag != true)
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

        return Result<GetOpportunityDetailMobileResponse>.Success(result);
    }
}
