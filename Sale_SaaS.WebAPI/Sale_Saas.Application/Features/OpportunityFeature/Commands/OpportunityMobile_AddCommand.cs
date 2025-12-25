using Sale_Saas.Application.Features.OpportunityFeature.Dto;
using Sale_Saas.Application.Features.OpportunityFeature.Services;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.OpportunityFeature.Commands;

public record OpportunityMobile_AddCommand(Guid UserId, CreateOrUpdateOpportunityRequest RequestData) : IRequest<Result<GetOpportunityDetailMobileResponse>>;

public class OpportunityMobile_AddCommandHandler : IRequestHandler<OpportunityMobile_AddCommand, Result<GetOpportunityDetailMobileResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IInternalService _internalService;
    private readonly IFeaturePermissionService _permissionService;
    private readonly IApplicationRoleService _roleService;

    public OpportunityMobile_AddCommandHandler(IApplicationDbContext context, IMapper mapper, IInternalService internalService, IFeaturePermissionService permissionService, IApplicationRoleService roleService)
    {
        _context = context;
        _mapper = mapper;
        _internalService = internalService;
        _permissionService = permissionService;
        _roleService = roleService;
    }

    public async Task<Result<GetOpportunityDetailMobileResponse>> Handle(OpportunityMobile_AddCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy user");
        }

        // check permission
        await _permissionService.HasPermission(MenuType.Sale_CH, FeatureType.CREATE, request.UserId, true);

        // get current role
        string currentRole = await _roleService.GetCurrentRoleOfUser(request.UserId);

        // it don't have avatar
        Opportunity opportunity = new Opportunity()
        {
            ApplicationUserId = request.RequestData.IdNhanVien ?? request.UserId,
            CustomerId = request.RequestData.IdKhachHang,
            Need = request.RequestData.NhuCau,
            Accountable = request.RequestData.NguoiQuyetDinh,
            Beneficiary = request.RequestData.NguoiThuHuong,
            TechnicalLead = request.RequestData.NguoiPhuTrachKiThuat,
            OpportunityStartDate = request.RequestData.NgayBatDau,
            OpportunityEndDate = request.RequestData.NgayKetThuc,
            TotalMoney = decimal.Parse(request.RequestData.TongTien ?? "0"),
            WinningOppotunity = request.RequestData.KhaNangThang,
            TypeMoney = request.RequestData.LoaiTien,
            Strategy = request.RequestData.ChienLuoc,
            CurrencyConversion = decimal.Parse(request.RequestData.QuyDoi ?? "0"),
            EstimatedMoney = decimal.Parse(request.RequestData.VonDuKien ?? "0"),
            CommissionMoney = decimal.Parse(request.RequestData.TienHoaHong ?? "0"),

            DeleteFlag = false,
            CreatedApplicationUserId = currentRole == RolePositionEnum.MANAGER.ToString() ? request.UserId : null,
            LastModifiedApplicationUserId = request.UserId,
            CreatedDate = DateTime.Now,
            LastModifiedDate = DateTime.Now
        };

        var statusString = OpportunityStatusEnum.PENDING.ToString();

        if (request.RequestData.IdNhanVien != null && request.RequestData.IdNhanVien.HasValue && request.RequestData.IdNhanVien.Value != Guid.Empty)
        {
            statusString = OpportunityStatusEnum.ACTIVE.ToString();
        }

        if (currentRole == RolePositionEnum.MANAGER.ToString())
        {
            statusString = OpportunityStatusEnum.ACTIVE.ToString();
        }

        //var statusString = request.RequestData.IdNhanVien != null ? OpportunityStatusEnum.ACTIVE.ToString() : OpportunityStatusEnum.PENDING.ToString();

        var status = await OpportunityService.GetStatus(statusString, _context);
        opportunity.OpportunityStatusId = status.Id;
        opportunity.OpportunityStatus = status;
        opportunity.Customer = await OpportunityService.GetCustomer(opportunity.CustomerId ?? Guid.Empty, _context);

        var applicationUser = await OpportunityService.GetApplicationUser(opportunity.ApplicationUserId ?? Guid.Empty, _context);
        opportunity.ApplicationUser = applicationUser;

        _context.Opportunities.Add(opportunity);

        await _context.SaveChangesAsync(cancellationToken);

        // auto add status

        List<GetOpportunityOpponentDetailResponse> listOpponentMap = null;
        List<GetOpportunityHistoryDetailResponse> listOpportunityHistoryMap = null;

        // add opponent
        if (request.RequestData.DanhSachDoiThu != null && request.RequestData.DanhSachDoiThu.Count > 0)
        {
            List<OpportunityOpponent> listOpponent = request.RequestData.DanhSachDoiThu
                                                                .Select(x => new OpportunityOpponent()
                                                                {
                                                                    Name = x.TenDoiThu,
                                                                    Strength = x.DiemManh,
                                                                    Weakness = x.DiemYeu,
                                                                    OpportunityId = opportunity.Id,

                                                                    DeleteFlag = false,
                                                                    CreatedApplicationUserId = request.UserId,
                                                                    LastModifiedApplicationUserId = request.UserId,
                                                                    CreatedDate = DateTime.Now,
                                                                    LastModifiedDate = DateTime.Now
                                                                })
                                                                .ToList();

            _context.OpportunityOpponents.AddRange(listOpponent);

            await _context.SaveChangesAsync(cancellationToken);

            listOpponentMap = listOpponent
                                    .Select(x => new GetOpportunityOpponentDetailResponse()
                                    {
                                        Id = x.Id,
                                        TenDoiThu = x.Name,
                                        DiemManh = x.Strength,
                                        DiemYeu = x.Weakness,
                                    })
                                    .ToList();
        }

        // add opportunity history
        if (request.RequestData.DanhSachLichSuTuongTac != null && request.RequestData.DanhSachLichSuTuongTac.Count > 0)
        {
            List<OpportunityHistory> listOpportunityHistory = request.RequestData.DanhSachLichSuTuongTac
                                                                                     .Select(x => new OpportunityHistory()
                                                                                     {
                                                                                         Goal = x.MucTieu,
                                                                                         Activity = x.HoatDong,
                                                                                         Time = x.ThoiDiem,
                                                                                         Result = x.KetQua,
                                                                                         OpportunityId = opportunity.Id,

                                                                                         DeleteFlag = false,
                                                                                         CreatedApplicationUserId = request.UserId,
                                                                                         LastModifiedApplicationUserId = request.UserId,
                                                                                         CreatedDate = DateTime.Now,
                                                                                         LastModifiedDate = DateTime.Now
                                                                                     })
                                                                                     .ToList();

            _context.OpportunityHistories.AddRange(listOpportunityHistory);

            await _context.SaveChangesAsync(cancellationToken);

            listOpportunityHistoryMap = listOpportunityHistory
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

        GetOpportunityDetailMobileResponse result = new GetOpportunityDetailMobileResponse()
        {
            IdCoHoi = opportunity.Id,
            IdKhachHang = opportunity.Customer.Id,
            MaKhachHang = opportunity.Customer.Code,
            TenKhachHang = opportunity.Customer.Fullname,
            IdNhanVien = opportunity.ApplicationUser.Id,
            TenNhanVien = opportunity.ApplicationUser.FullName,
            //AnhDaiDien = "",
            NhuCau = opportunity.Need,
            NguoiQuyetDinh = opportunity.Accountable,
            NguoiPhuTrachKiThuat = opportunity.TechnicalLead,
            NguoiThuHuong = opportunity.Beneficiary,
            NgayBatDau = opportunity.OpportunityStartDate,
            NgayKetThuc = opportunity.OpportunityEndDate,
            TongTien = opportunity.TotalMoney.ToString(),
            KhaNangThang = opportunity.WinningOppotunity,
            LoaiTien = opportunity.TypeMoney,
            QuyDoi = opportunity.CurrencyConversion.ToString(),
            DanhSachDoiThu = listOpponentMap,
            ChienLuoc = opportunity.Strategy,
            LyDo = opportunity.Reason,
            DanhSachLichSuTuongTac = listOpportunityHistoryMap,
            VonDuKien = opportunity.EstimatedMoney.ToString(),
            TienHoaHong = opportunity.CommissionMoney.ToString(),
            TrangThai = new TrangThaiResponse()
            {
                Id = opportunity.OpportunityStatus.Id,
                Code = opportunity.OpportunityStatus.Code,
                Name = opportunity.OpportunityStatus.Name
            }
        };

        return Result<GetOpportunityDetailMobileResponse>.Success(result);
    }
}
