using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.Identity;

namespace Sale_Saas.Application.Features.BenefitFeature.Queries;

public record Benefit_GetListRoleOfUserNotExistBenefitQuery(Guid UserId, string RolePostionId) : IRequest<Result<IEnumerable<RoleDto>>>;

public class Benefit_GetListRoleOfUserNotExistBenefitQueryHandler : IRequestHandler<Benefit_GetListRoleOfUserNotExistBenefitQuery, Result<IEnumerable<RoleDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IFeaturePermissionService _permissionService;
    private readonly IApplicationRoleService _applicationRoleService;
    private readonly IApplicationUserService _userService;
    private readonly IEventLogService _eventLogService;

    public Benefit_GetListRoleOfUserNotExistBenefitQueryHandler(
           IMapper mapper,
           IApplicationDbContext context,
           IFeaturePermissionService permissionService,
           IApplicationRoleService applicationRoleService,
           IApplicationUserService userService,
           IEventLogService eventLogService)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        _applicationRoleService = applicationRoleService;
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _eventLogService = eventLogService ?? throw new ArgumentNullException(nameof(eventLogService));
    }

    public async Task<Result<IEnumerable<RoleDto>>> Handle(Benefit_GetListRoleOfUserNotExistBenefitQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ApplicationException("Id không được phép null");
        }

        var listRoleOfUser = await _applicationRoleService.GetListRoleByUserId(request.UserId);

        var listRoleOfUserIsEmployee = listRoleOfUser.Where(x => x.RolePositionId == request.RolePostionId
                                                              && x.DeleteFlag != true)
                                                     .Select(x => new RoleDto()
                                                     {
                                                         Id = x.Id,
                                                         DisplayName = x.DisplayName ?? "",
                                                         Name = x.Name ?? "",
                                                         RolePositionId = x.RolePositionId ?? ""
                                                     })
                                                     .ToList();

        var benefitOfUser = await _context.Benefits.Where(x => x.ApplicationUserId == request.UserId
                                                            && x.RolePositionId == request.RolePostionId
                                                            && x.DeleteFlag != true
                                                            && x.CreatedDate.Year == DateTime.Now.Year)
                                                   .Distinct()
                                                   .Select(x => x.ApplicationRoleId)
                                                   .ToListAsync();

        listRoleOfUserIsEmployee = listRoleOfUserIsEmployee.Where(x => !benefitOfUser.Contains(x.Id)).ToList();

        return Result<IEnumerable<RoleDto>>.Success(listRoleOfUserIsEmployee);
    }
}
