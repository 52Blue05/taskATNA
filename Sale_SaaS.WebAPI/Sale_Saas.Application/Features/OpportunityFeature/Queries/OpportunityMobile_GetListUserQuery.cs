using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.Identity;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.OpportunityFeature.Queries;

public record OpportunityMobile_GetListUserQuery(Guid UserId, GetListWithPaginationQueryRequest RequestData) : IRequest<Result<PaginatedList<UserBasicInfoDto>>>;

public class OpportunityMobile_GetListUserQueryHandler : IRequestHandler<OpportunityMobile_GetListUserQuery, Result<PaginatedList<UserBasicInfoDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventLogService _eventLogService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IApplicationRoleService _applicationRoleService;
    private readonly IMapper _mapper;

    public OpportunityMobile_GetListUserQueryHandler(IApplicationDbContext context, IEventLogService eventLogService, IApplicationUserService applicationUserService, IApplicationRoleService applicationRoleService, IMapper mapper)
    {
        _context = context;
        _eventLogService = eventLogService;
        _applicationUserService = applicationUserService;
        _applicationRoleService = applicationRoleService;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedList<UserBasicInfoDto>>> Handle(OpportunityMobile_GetListUserQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ApplicationException("Không tìm thấy người dùng");
        }

        var result = new List<ApplicationUser>();

        var listUser = await _context.ApplicationUsers.Where(x => x.DeleteFlag != true && x.Id != request.UserId)
                                                      .AsNoTracking()
                                                      .ToListAsync();

        if (!string.IsNullOrEmpty(request.RequestData.TextSearch))
        {
            listUser = listUser.Where(s => s.FullName.ToLower().Contains(request.RequestData.TextSearch.ToLower()))
                               .ToList();
        }

        foreach (var user in listUser)
        {
            var listRoleOfUser = await _applicationRoleService.GetListRoleByUserId(user.Id);

            var listRoleStringOfUser = listRoleOfUser.Select(x => x.RolePositionId).ToList();

            if (listRoleStringOfUser.Contains(RolePositionEnum.EMPLOYEE.ToString()))
            {
                result.Add(user);
            }
        }

        var resultMap = _mapper.Map<List<UserBasicInfoDto>>(result);

        return Result<PaginatedList<UserBasicInfoDto>>.Success(PaginatedList<UserBasicInfoDto>.CreateFromList(resultMap, request.RequestData.PageIndex, request.RequestData.PageSize));
    }
}
