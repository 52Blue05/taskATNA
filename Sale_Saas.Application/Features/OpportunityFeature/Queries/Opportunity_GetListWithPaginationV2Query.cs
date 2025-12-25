using Sale_Saas.Application.Features.OpportunityFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.OpportunityFeature.Queries;

public record Opportunity_GetListWithPaginationV2Query(Guid UserId, OpportunityGetListWithPaginationRequest RequestData) : IRequest<Result<PaginatedList<OpportunityDto>>>;

public class Opportunity_GetListWithPaginationV2QueryHandler : IRequestHandler<Opportunity_GetListWithPaginationV2Query, Result<PaginatedList<OpportunityDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IFeaturePermissionService _permissionService;

    public Opportunity_GetListWithPaginationV2QueryHandler(IMapper mapper, IApplicationDbContext context, IFeaturePermissionService permissionService)
    {
        _context = context;
        _mapper = mapper;
        _permissionService = permissionService;
    }

    public async Task<Result<PaginatedList<OpportunityDto>>> Handle(Opportunity_GetListWithPaginationV2Query request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy người dùng");
        }

        var query = _context.Opportunities.Where(m => m.DeleteFlag != true)
                                          .Include(x => x.OpportunityOpponents)
                                          .Include(x => x.OpportunityStatus)
                                          .Include(x => x.Customer)
                                          .Include(x => x.OpportunityHistories)
                                          .Include(x => x.ApplicationUser)
                                          .OrderByDescending(x => x.CreatedDate)
                                          .ProjectTo<OpportunityDto>(_mapper.ConfigurationProvider)
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

        if (request.RequestData.Time != null)
        {
            query = query.Where(s => s.CreatedDate.Date.Year == request.RequestData.Time.Value.Year);
        }
        //if (request.RequestData.IsCurrentYear == true)
        //{
        //    query = query.Where(s => s.CreatedDate.Date.Year == DateTime.Now.Year);
        //}

        var listOpportunity = await query.ToListAsync();
        List<OpportunityDto> listResult = new List<OpportunityDto>();

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



        return Result<PaginatedList<OpportunityDto>>.Success(PaginatedList<OpportunityDto>.CreateFromList(listResult, request.RequestData.PageIndex, request.RequestData.PageSize));
    }
}
