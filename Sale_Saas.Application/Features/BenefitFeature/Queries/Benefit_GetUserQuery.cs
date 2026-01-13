using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.Identity;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.BenefitFeature.Queries
{
     public record Benefit_GetUserQuery(Guid userId, GetAllQueryRequest RequestData) : IRequest<Result<IEnumerable<UserBasicInfoDto>>>;

     public class Benefit_GetUserQueryHandler : IRequestHandler<Benefit_GetUserQuery, Result<IEnumerable<UserBasicInfoDto>>>
     {
          private readonly IApplicationDbContext _context;
          private readonly IMapper _mapper;
          private readonly IFeaturePermissionService _permissionService;
          private readonly IApplicationUserService _userService;
          private readonly IEventLogService _eventLogService;

          public Benefit_GetUserQueryHandler(
                 IMapper mapper,
                 IApplicationDbContext context,
                 IFeaturePermissionService permissionService,
                 IApplicationUserService userService,
                 IEventLogService eventLogService)
          {
               _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
               _context = context ?? throw new ArgumentNullException(nameof(context));
               _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
               _userService = userService ?? throw new ArgumentNullException(nameof(userService));
               _eventLogService = eventLogService ?? throw new ArgumentNullException(nameof(eventLogService));
          }

          public async Task<Result<IEnumerable<UserBasicInfoDto>>> Handle(Benefit_GetUserQuery request, CancellationToken cancellationToken)
          {
               if (request.RequestData.UserId == null)
               {
                    throw new ApplicationException("Missing user ID in request data");
               }

               var userId = (Guid)request.RequestData.UserId;

               var position = await _permissionService.GetPositionByUser(userId);

               var distinctIds = await GetDistinctBenefitUserIds();

               var ids = await GetFilteredUserIds(position, distinctIds);

               var query = _context.ApplicationUsers
                                        .Where(con => con.DeleteFlag != true && ids.Contains(con.Id))
                                        .Select(con => _mapper.Map<UserBasicInfoDto>(con))
                                        .AsNoTracking();

               if (!string.IsNullOrEmpty(request.RequestData.TextSearch))
               {
                    query = query.Where(s => s.FullName.Contains(request.RequestData.TextSearch) ||
                                                   s.Code.Contains(request.RequestData.TextSearch) ||
                                                   s.Email.Contains(request.RequestData.TextSearch));
               }
               var users = await query.ToListAsync();

               var eventLog = await _eventLogService.Create("BenefitFeature", "BenefitFeature", "Benefit_GetUserQuery", request.userId);

               return Result<IEnumerable<UserBasicInfoDto>>.Success(users);
          }

          private async Task<List<Guid>> GetFilteredUserIds(string position, List<Guid> distinctIds)
          {
               var ids = new List<Guid>();

               if (position == RolePositionEnum.MANAGER.ToString())
               {
                    ids = await _userService.GetUserIdByRolePositionAndPermission(
                         RolePositionEnum.EMPLOYEE.ToString(),
                         MenuType.Sale_QL,
                         FeatureType.QL_THUAHUONG
                    );
               }
               else if (position == RolePositionEnum.ADMINISTRATOR.ToString())
               {
                    ids = await _userService.GetUserIdByRolePositionAndPermission(
                         RolePositionEnum.MANAGER.ToString(),
                         MenuType.Sale_QL,
                         FeatureType.QL_THUAHUONG
                    );
               }
               else
               {
                    return new List<Guid>();
               }

               ids = ids.Where(s => !distinctIds.Contains(s)).ToList();
               return ids;

          }

          private async Task<List<Guid>> GetDistinctBenefitUserIds()
          {
               var idsNullable = await _context.Benefits
                                                    .Where(s => !s.DeleteFlag && s.ApplicationUserId != null)
                                                    .Select(s => s.ApplicationUserId)
                                                    .Distinct()
                                                    .ToListAsync();

               var ids = idsNullable.Select(id => id ?? Guid.Empty).ToList();

               return ids;
          }
     }
}
