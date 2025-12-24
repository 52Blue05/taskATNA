using System.Net;
using Sale_Saas.Application.Features.OpportunityFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.OpportunityFeature.Queries
{
    public record Opportunity_GetListWithPaginationQuery(GetListWithPaginationQueryRequest RequestData) : IRequest<Result<PaginatedList<OpportunityDto>>>;

    public class Opportunity_GetListWithPaginationQueryHandler : IRequestHandler<Opportunity_GetListWithPaginationQuery, Result<PaginatedList<OpportunityDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IFeaturePermissionService _permissionService;
        private readonly IApplicationRoleService _roleService;
        private readonly IFeaturePermissionService _PermissionService;
        private readonly IApplicationUserService _UserService;

        public Opportunity_GetListWithPaginationQueryHandler(IMapper mapper, IApplicationDbContext context,
                                                             IFeaturePermissionService permissionService, IApplicationRoleService roleService,
                                                             IFeaturePermissionService PermissionService, IApplicationUserService UserService)
        {
            _context = context;
            _mapper = mapper;
            _permissionService = permissionService;
            _roleService = roleService;
            _permissionService = permissionService;
            _UserService = UserService;
        }

        public async Task<Result<PaginatedList<OpportunityDto>>> Handle(Opportunity_GetListWithPaginationQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Opportunities.Where(m => m.DeleteFlag != true)
                                              .Include(s => s.ApplicationUser)
                                              .Include(s => s.Customer)
                                              .OrderByDescending(x => x.CreatedDate)
                                              .ProjectTo<OpportunityDto>(_mapper.ConfigurationProvider)
                                              .AsNoTracking();

            if (!string.IsNullOrEmpty(request.RequestData.Roles))
            {
                request.RequestData.Roles = request.RequestData.Roles.Trim();
                request.RequestData.Roles = WebUtility.UrlDecode(request.RequestData.Roles);
                List<Guid> listRoleId = request.RequestData.Roles.Split(",").Select(s => Guid.Parse(s.Trim())).ToList();
                query = query.Where(x => listRoleId.Contains((Guid)x.ApplicationRoleId));
            }

            if (!string.IsNullOrEmpty(request.RequestData.TextSearch))
            {
                query = query.Where(x => x.Customer.Code.ToLower().Contains(request.RequestData.TextSearch.ToLower()) ||
                                         x.ApplicationUser.FullName.ToLower().Contains(request.RequestData.TextSearch.ToLower()));
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

            if (request.RequestData.Time != null)
            {
                query = query.Where(s => s.CreatedDate.Date.Year == request.RequestData.Time.Value.Year);
            }

            if (request.RequestData.RoleType != null)
            {
                if (request.RequestData.RoleType == RoleType.MYSELF.ToString())
                {
                    query = query.Where(s => s.ApplicationUser.Id == request.RequestData.UserId);
                }
                else
                {
                    throw new Exception($"Role type không hợp lệ: {request.RequestData.RoleType}");
                }
            }

            var listOpportunity = await query.ToListAsync();
            List<OpportunityDto> listResult = new List<OpportunityDto>();

            if (listOpportunity != null && listOpportunity.Count > 0)
            {
                var listPending = listOpportunity.Where(x => x.OpportunityStatus.Code == OpportunityStatusEnum.PENDING.ToString())
                                                 .OrderByDescending(x => int.Parse(x?.WinningOppotunity ?? "0"))
                                                 .ToList();

                var listInActive = listOpportunity.Where(x => x.OpportunityStatus.Code == OpportunityStatusEnum.ACTIVE.ToString())
                                                  .OrderByDescending(x => int.Parse(x?.WinningOppotunity ?? "0"))
                                                  .ToList();

                var listOnHold = listOpportunity.Where(x => x.OpportunityStatus.Code == OpportunityStatusEnum.ONHOLD.ToString())
                                                  .OrderByDescending(x => int.Parse(x?.WinningOppotunity ?? "0"))
                                                  .ToList();

                var listCancel = listOpportunity.Where(x => x.OpportunityStatus.Code == OpportunityStatusEnum.CANCEL.ToString())
                                                  .OrderByDescending(x => int.Parse(x?.WinningOppotunity ?? "0"))
                                                  .ToList();

                var listFail = listOpportunity.Where(x => x.OpportunityStatus.Code == OpportunityStatusEnum.FAIL.ToString())
                                                  .OrderByDescending(x => int.Parse(x?.WinningOppotunity ?? "0"))
                                                  .ToList();

                var listClose = listOpportunity.Where(x => x.OpportunityStatus.Code == OpportunityStatusEnum.CLOSE.ToString())
                                                  .OrderByDescending(x => int.Parse(x?.WinningOppotunity ?? "0"))
                                                  .ToList();

                listResult.AddRange(listPending);
                listResult.AddRange(listInActive);
                listResult.AddRange(listOnHold);
                listResult.AddRange(listCancel);
                listResult.AddRange(listFail);
                listResult.AddRange(listClose);
            }

            var result = new PaginatedList<OpportunityDto>(listResult, listResult.Count, request.RequestData.PageIndex, request.RequestData.PageSize);

            return Result<PaginatedList<OpportunityDto>>.Success(result);
        }
    }
}
