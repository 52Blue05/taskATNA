using System.Net;
using Sale_Saas.Application.Features.OpportunityFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.OpportunityFeature.Queries;

public record OpportunityMobile_GetListWithPaginationV2Query(Guid UserId, OpportunityGetListWithPaginationRequest RequestData) : IRequest<Result<GetListOpportunityMobileResponse>>;

public class OpportunityMobile_GetListWithPaginationV2QueryHandler : IRequestHandler<OpportunityMobile_GetListWithPaginationV2Query, Result<GetListOpportunityMobileResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IFeaturePermissionService _permissionService;

    public OpportunityMobile_GetListWithPaginationV2QueryHandler(IMapper mapper, IApplicationDbContext context, IFeaturePermissionService permissionService)
    {
        _context = context;
        _mapper = mapper;
        _permissionService = permissionService;
    }

    public async Task<Result<GetListOpportunityMobileResponse>> Handle(OpportunityMobile_GetListWithPaginationV2Query request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy người dùng");
        }

        var query = _context.Opportunities.Where(m => m.DeleteFlag != true)
                                          .Include(s => s.ApplicationUser)
                                          .Include(s => s.Customer)
                                          .OrderByDescending(x => x.CreatedDate)
                                          .ProjectTo<OpportunityMobileDto>(_mapper.ConfigurationProvider)
                                          .AsNoTracking();

        if (!string.IsNullOrEmpty(request.RequestData.TextSearch))
        {
            query = query.Where(x => x.Customer.Fullname.ToLower().Contains(request.RequestData.TextSearch.ToLower()));
        }

        if (request.RequestData.RoleType == RoleType.MYSELF.ToString())
        {
            await _permissionService.HasPermission(MenuType.Sale_CH, FeatureType.VIEW, request.UserId);

            query = query.Where(x => x.ApplicationUser.Id == request.UserId);
        }
        else if (request.RequestData.RoleType == RoleType.EMPLOYEE.ToString())
        {
            await _permissionService.HasPermission(MenuType.Sale_CH, FeatureType.VIEW, request.UserId);

            query = query.Where(x => x.CreatedApplicationUserId == null || (x.CreatedApplicationUserId == request.UserId && x.ApplicationUser.Id != request.UserId));

            if (request.RequestData.UserId != null)
            {
                query = query.Where(x => x.ApplicationUser.Id == request.RequestData.UserId
                                     && (x.CreatedApplicationUserId == null || x.CreatedApplicationUserId == request.UserId));
            }
        }

        if (request.RequestData.StatusId != null)
        {
            query = query.Where(s => s.OpportunityStatus.Id == request.RequestData.StatusId);
        }

        //if (request.RequestData.Time != null)
        //{
        //    query = query.Where(s => s.CreatedDate.Date.Year == request.RequestData.Time.Value.Year);
        //}
        if (request.RequestData.Year != null)
        {
            query = query.Where(s => s.CreatedDate.Date.Year == request.RequestData.Year);
        }

        if (request.RequestData.RoleId != null)
        {
            query = query.Where(x => x.ApplicationRoleId == request.RequestData.RoleId);
        }

        var listOpportunity = await query.ToListAsync();
        List<OpportunityMobileDto> listResult = new List<OpportunityMobileDto>();

        int totalOpportunityClosed = 0;
        decimal totalMoneyOpportunityClosed = 0;

        if (listOpportunity != null && listOpportunity.Count > 0)
        {
            var listPending = listOpportunity.Where(x => x.OpportunityStatus.Code == OpportunityStatusEnum.PENDING.ToString())
                                             .OrderByDescending(x => int.Parse(x.WinningOppotunity??"0"))
                                             .ToList();

            var listInActive = listOpportunity.Where(x => x.OpportunityStatus.Code == OpportunityStatusEnum.ACTIVE.ToString())
                                              .OrderByDescending(x => int.Parse(x.WinningOppotunity ?? "0"))
                                              .ToList();

            var listOnHold = listOpportunity.Where(x => x.OpportunityStatus.Code == OpportunityStatusEnum.ONHOLD.ToString())
                                            .OrderByDescending(x => int.Parse(x.WinningOppotunity ?? "0"))
                                            .ToList();

            var listCancel = listOpportunity.Where(x => x.OpportunityStatus.Code == OpportunityStatusEnum.CANCEL.ToString())
                                            .OrderByDescending(x => int.Parse(x.WinningOppotunity ?? "0"))
                                            .ToList();

            var listFail = listOpportunity.Where(x => x.OpportunityStatus.Code == OpportunityStatusEnum.FAIL.ToString())
                                          .OrderByDescending(x => int.Parse(x.WinningOppotunity ?? "0"))
                                          .ToList();

            var listClose = listOpportunity.Where(x => x.OpportunityStatus.Code == OpportunityStatusEnum.CLOSE.ToString())
                                           .OrderByDescending(x => int.Parse(x.WinningOppotunity ?? "0"))
                                           .ToList();

            totalOpportunityClosed = listClose.Count;
            totalMoneyOpportunityClosed = listClose.Sum(x => x.TotalMoney);

            listResult.AddRange(listPending);
            listResult.AddRange(listInActive);
            listResult.AddRange(listOnHold);
            listResult.AddRange(listCancel);
            listResult.AddRange(listFail);
            listResult.AddRange(listClose);
        }

        if (listResult.Count <= 0 || listResult == null)
        {
            var response = new GetListOpportunityMobileResponse()
            {
                SoCoHoi = 0,
                TongTienCoHoi = "0",
                LoaiTien = "",
                SoCoHoiDaDong = totalOpportunityClosed,
                TongTienCoHoiDaDong = totalMoneyOpportunityClosed.ToString()
            };

            return Result<GetListOpportunityMobileResponse>.Success(response);
        }

        var listResultVietnamese = listResult
                                        .Select(x => new GetOpportunityMobileDto()
                                        {
                                            IdCoHoi = x.Id,
                                            IdNhanVien = x.ApplicationUser.Id,
                                            TenNhanVien = x.ApplicationUser.FullName,
                                            TrangThai = x.OpportunityStatus.Code,
                                            TenTrangThai = x.OpportunityStatus.Name,
                                            MaKhachHang = x.Customer.Code,
                                            TenKhachHang = x.Customer.Fullname,
                                            NguoiQuyetDinh = x.Accountable,
                                            NguoiPhuTrachKiThuat = x.TechnicalLead,
                                            NguoiThuHuong = x.Beneficiary,
                                            NhuCau = x.Need,
                                            NgayBatDau = x.OpportunityStartDate,
                                            NgayKetThuc = x.OpportunityEndDate,
                                            TongTien = x.TotalMoney,
                                            KhaNangThang = x.WinningOppotunity,
                                            LoaiTien = x.TypeMoney,
                                            QuyDoi = x.CurrencyConversion,
                                            LyDo = x.Reason,
                                            NguoiPhuTrach = x.ApplicationUser
                                        })
                                        .ToList();

        var totalOpportunity = listResultVietnamese.Count;
        var items = listResultVietnamese.Skip((request.RequestData.PageIndex - 1) * request.RequestData.PageSize).Take(request.RequestData.PageSize).ToList();

        var result = new GetListOpportunityMobileResponse()
        {
            SoCoHoi = totalOpportunity,
            TongTienCoHoi = items.Sum(x => x.TongTien).ToString(),
            LoaiTien = items[0].LoaiTien,
            SoCoHoiDaDong = totalOpportunityClosed,
            TongTienCoHoiDaDong = totalMoneyOpportunityClosed.ToString(),

            DanhSachCoHoi = items
        };

        return Result<GetListOpportunityMobileResponse>.Success(result);
    }
}