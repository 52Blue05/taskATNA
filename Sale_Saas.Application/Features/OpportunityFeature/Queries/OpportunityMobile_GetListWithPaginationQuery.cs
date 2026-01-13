using Sale_Saas.Application.Features.OpportunityFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Domain.Entities;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.OpportunityFeature.Queries
{
    public record OpportunityMobile_GetListWithPaginationQuery(OpportunityGetListWithPaginationRequest RequestData) : IRequest<Result<GetListOpportunityMobileResponse>>;

    public class OpportunityMobile_GetListWithPaginationQueryHandler : IRequestHandler<OpportunityMobile_GetListWithPaginationQuery, Result<GetListOpportunityMobileResponse>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IFeaturePermissionService _permissionService;
        public OpportunityMobile_GetListWithPaginationQueryHandler(IMapper mapper, IApplicationDbContext context, IFeaturePermissionService permissionService)
        {
            _context = context;
            _mapper = mapper;
            _permissionService = permissionService;
        }

        public async Task<Result<GetListOpportunityMobileResponse>> Handle(OpportunityMobile_GetListWithPaginationQuery request, CancellationToken cancellationToken)
        {
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

            if (request.RequestData.UserId != null)
            {
                var access = await _permissionService.HasPermission(MenuType.Sale_CH, FeatureType.VIEW, (Guid)request.RequestData.UserId);
                if (access != true)
                {
                    // Sale/Supplier
                    query = query.Where(s => s.ApplicationUser.Id == request.RequestData.UserId);
                }
                else
                {
                    // Sale Director (Main Role)
                    query = query.Where(s => s.CreatedApplicationUserId == request.RequestData.UserId
                                          || s.CreatedApplicationUserId == null
                                          || s.ApplicationUser.Id == request.RequestData.UserId);
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

            var listOpportunity = await query.ToListAsync();
            List<OpportunityMobileDto> listResult = new List<OpportunityMobileDto>();

            if (listOpportunity != null && listOpportunity.Count > 0)
            {
                var listPending = listOpportunity.Where(x => x.OpportunityStatus.Code == OpportunityStatusEnum.PENDING.ToString())
                                                 .OrderByDescending(x => int.Parse(x.WinningOppotunity ?? "0"))
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
                    LoaiTien = ""
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
                                                TenKhachHang = x.Customer.Fullname,
                                                NguoiQuyetDinh = x.Accountable,
                                                NhuCau = x.Need,
                                                NgayBatDau = x.OpportunityStartDate,
                                                NgayKetThuc = x.OpportunityEndDate,
                                                TongTien = x.TotalMoney,
                                                KhaNangThang = x.WinningOppotunity,
                                                LoaiTien = x.TypeMoney,
                                                VonDuKien = x.EstimatedMoney.ToString(),
                                                TienHoaHong = x.CommissionMoney.ToString(),
                                            })
                                            .ToList();

            var totalOpportunity = listResultVietnamese.Count;
            var items = listResultVietnamese.Skip((request.RequestData.PageIndex - 1) * request.RequestData.PageSize).Take(request.RequestData.PageSize).ToList();

            var result = new GetListOpportunityMobileResponse()
            {
                SoCoHoi = totalOpportunity,
                TongTienCoHoi = items.Sum(x => x.TongTien).ToString(),
                LoaiTien = items[0].LoaiTien,
                DanhSachCoHoi = items
            };

            return Result<GetListOpportunityMobileResponse>.Success(result);
        }
    }
}
