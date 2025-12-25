using Sale_Saas.Application.Features.OpportunityFeature.Dto;
using Sale_Saas.Application.Features.OpportunityFeature.Services;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.OpportunityFeature.Commands;

public record OpportunityMobile_UpdateCommand(Guid UserId, CreateOrUpdateOpportunityRequest RequestData) : IRequest<Result<GetOpportunityDetailMobileResponse>>;

public class OpportunityMobile_UpdateCommandHandler : IRequestHandler<OpportunityMobile_UpdateCommand, Result<GetOpportunityDetailMobileResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IInternalService _internalService;
    private readonly IFeaturePermissionService _permissionService;

    public OpportunityMobile_UpdateCommandHandler(IApplicationDbContext context, IMapper mapper, IInternalService internalService, IFeaturePermissionService permissionService)
    {
        _context = context;
        _mapper = mapper;
        _internalService = internalService;
        _permissionService = permissionService;
    }

    public async Task<Result<GetOpportunityDetailMobileResponse>> Handle(OpportunityMobile_UpdateCommand request, CancellationToken cancellationToken)
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

        opportunity.ApplicationUserId = opportunity.ApplicationUserId;
        opportunity.CustomerId = request.RequestData.IdKhachHang ?? opportunity.CustomerId;
        opportunity.Need = request.RequestData.NhuCau ?? opportunity.Need;
        opportunity.Accountable = request.RequestData.NguoiQuyetDinh ?? opportunity.Accountable;
        opportunity.OpportunityStartDate = request.RequestData.NgayBatDau ?? opportunity.OpportunityStartDate;
        opportunity.OpportunityEndDate = request.RequestData.NgayKetThuc ?? opportunity.OpportunityEndDate;
        opportunity.TotalMoney = decimal.Parse(request.RequestData.TongTien ?? opportunity.TotalMoney.ToString());
        opportunity.WinningOppotunity = request.RequestData.KhaNangThang ?? opportunity.WinningOppotunity;
        opportunity.TypeMoney = request.RequestData.LoaiTien ?? opportunity.TypeMoney;
        opportunity.Strategy = request.RequestData.ChienLuoc ?? opportunity.Strategy;
        opportunity.Beneficiary = request.RequestData.NguoiThuHuong ?? opportunity.Beneficiary;
        opportunity.TechnicalLead = request.RequestData.NguoiPhuTrachKiThuat ?? opportunity.TechnicalLead;
        opportunity.CurrencyConversion = decimal.Parse(request.RequestData.QuyDoi ?? opportunity.CurrencyConversion.ToString());
        opportunity.EstimatedMoney = decimal.Parse(request.RequestData.VonDuKien ?? opportunity.EstimatedMoney.ToString());
        opportunity.CommissionMoney = decimal.Parse(request.RequestData.TienHoaHong ?? opportunity.CommissionMoney.ToString());
        opportunity.Reason = opportunity.Reason;
        opportunity.LastModifiedApplicationUserId = request.UserId;
        opportunity.LastModifiedDate = DateTime.Now;
        opportunity.Customer = await OpportunityService.GetCustomer(opportunity.CustomerId ?? Guid.Empty, _context);

        List<GetOpportunityOpponentDetailResponse> listOpponentMap = new();
        List<GetOpportunityHistoryDetailResponse> listOpportunityHistoryMap = new();

        // Update opponents
        if (request.RequestData.DanhSachDoiThu != null && request.RequestData.DanhSachDoiThu.Count > 0)
        {
            // remove
            var listOpponentRequestIds = request.RequestData.DanhSachDoiThu.Select(x => x.Id).ToList();
            var listOpponentNeedRemove = opportunity.OpportunityOpponents.Where(x => !listOpponentRequestIds.Contains(x.Id)).ToList();

            foreach (var opponent in listOpponentNeedRemove)
            {
                _context.OpportunityOpponents.Remove(opponent);
            }

            await _context.SaveChangesAsync(cancellationToken);

            // update or add
            foreach (var opponentDto in request.RequestData.DanhSachDoiThu)
            {
                var opponent = opponentDto.Id != null
                            ? opportunity.OpportunityOpponents.FirstOrDefault(x => x.Id == opponentDto.Id)
                            : null;

                if (opponent != null)
                {
                    opponent.Name = opponentDto.TenDoiThu;
                    opponent.Strength = opponentDto.DiemManh;
                    opponent.Weakness = opponentDto.DiemYeu;
                    opponent.LastModifiedApplicationUserId = request.UserId;
                    opponent.LastModifiedDate = DateTime.Now;
                }
                else
                {
                    _context.OpportunityOpponents.Add(new OpportunityOpponent
                    {
                        Id = opponentDto.Id ?? Guid.NewGuid(),
                        Name = opponentDto.TenDoiThu,
                        Strength = opponentDto.DiemManh,
                        Weakness = opponentDto.DiemYeu,
                        OpportunityId = opportunity.Id,
                        DeleteFlag = false,
                        LastModifiedApplicationUserId = request.UserId,
                        LastModifiedDate = DateTime.Now
                    });
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            listOpponentMap = await _context.OpportunityOpponents.Where(x => x.DeleteFlag != true && x.OpportunityId == opportunity.Id)
                                                                 .Select(x => new GetOpportunityOpponentDetailResponse
                                                                 {
                                                                     Id = x.Id,
                                                                     TenDoiThu = x.Name,
                                                                     DiemManh = x.Strength,
                                                                     DiemYeu = x.Weakness,
                                                                 })
                                                                 .ToListAsync();
        }
        else
        {
            // remove
            foreach (var opponent in opportunity.OpportunityOpponents)
            {
                _context.OpportunityOpponents.Remove(opponent);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        // Update opportunity histories
        if (request.RequestData.DanhSachLichSuTuongTac != null && request.RequestData.DanhSachLichSuTuongTac.Count > 0)
        {
            // remove
            var listHistoriesRequestIds = request.RequestData.DanhSachLichSuTuongTac.Select(x => x.Id).ToList();
            var listHistoriesNeedRemove = opportunity.OpportunityHistories.Where(x => !listHistoriesRequestIds.Contains(x.Id)).ToList();

            foreach (var history in listHistoriesNeedRemove)
            {
                _context.OpportunityHistories.Remove(history);
            }

            await _context.SaveChangesAsync(cancellationToken);

            // update or add
            foreach (var historyDto in request.RequestData.DanhSachLichSuTuongTac)
            {
                var history = historyDto.Id != null ?
                    opportunity.OpportunityHistories.FirstOrDefault(x => x.Id == historyDto.Id)
                    : null;

                if (history != null)
                {
                    history.Goal = historyDto.MucTieu;
                    history.Activity = historyDto.HoatDong;
                    history.Time = historyDto.ThoiDiem;
                    history.Result = historyDto.KetQua;
                    history.LastModifiedApplicationUserId = request.UserId;
                    history.LastModifiedDate = DateTime.Now;
                }
                else
                {
                    _context.OpportunityHistories.Add(new OpportunityHistory
                    {
                        Id = historyDto.Id ?? Guid.NewGuid(),
                        Goal = historyDto.MucTieu,
                        Activity = historyDto.HoatDong,
                        Time = historyDto.ThoiDiem,
                        Result = historyDto.KetQua,
                        OpportunityId = opportunity.Id,
                        DeleteFlag = false,
                        LastModifiedApplicationUserId = request.UserId,
                        LastModifiedDate = DateTime.Now
                    });
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            listOpportunityHistoryMap = await _context.OpportunityHistories.Where(x => x.DeleteFlag != true && x.OpportunityId == opportunity.Id)
                                                                           .Select(x => new GetOpportunityHistoryDetailResponse
                                                                           {
                                                                               Id = x.Id,
                                                                               MucTieu = x.Goal,
                                                                               HoatDong = x.Activity,
                                                                               ThoiDiem = x.Time,
                                                                               KetQua = x.Result,
                                                                           })
                                                                           .ToListAsync();
        }
        else
        {
            foreach (var history in opportunity.OpportunityHistories)
            {
                _context.OpportunityHistories.Remove(history);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        GetOpportunityDetailMobileResponse result = new()
        {
            IdCoHoi = opportunity.Id,
            IdKhachHang = opportunity.Customer.Id,
            MaKhachHang = opportunity.Customer.Code,
            TenKhachHang = opportunity.Customer.Fullname,
            IdNhanVien = opportunity.ApplicationUser?.Id,
            TenNhanVien = opportunity.ApplicationUser?.FullName,
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
            ChienLuoc = opportunity.Strategy,
            QuyDoi = opportunity.CurrencyConversion.ToString(),
            LyDo = opportunity.Reason,
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

