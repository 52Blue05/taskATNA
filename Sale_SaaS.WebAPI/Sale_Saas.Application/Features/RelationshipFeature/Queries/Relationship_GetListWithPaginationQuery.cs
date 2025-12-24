using System.Net;
using Sale_Saas.Application.Common.Mappings;
using Sale_Saas.Application.Features.RelationshipFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.RelationshipFeature.Queries
{
    public record Relationship_GetListWithPaginationQuery(Guid userId, GetListWithPaginationQueryRequest RequestData) : IRequest<Result<PaginatedList<RelationshipDto>>>;

    public class Relationship_GetListWithPaginationQueryHandler : IRequestHandler<Relationship_GetListWithPaginationQuery, Result<PaginatedList<RelationshipDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IApplicationUserService _UserService;
        private readonly IFeaturePermissionService _PermissionService;
        public Relationship_GetListWithPaginationQueryHandler(
            IMapper mapper, IApplicationDbContext context,
            IApplicationUserService UserService,
            IFeaturePermissionService PermissionService)
        {
            _context = context;
            _mapper = mapper;
            _UserService = UserService;
            _PermissionService = PermissionService;
        }

        public async Task<Result<PaginatedList<RelationshipDto>>> Handle(Relationship_GetListWithPaginationQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Relationships.Include(s => s.Customer)
                                              .Include(s => s.ApplicationUser)
                                              .Include(s => s.CurrentRelationship)
                                              .Include(s => s.TargetRelationship)
                                              .Include(s => s.YearToDate)
                                              .Include(s => s.RelationshipCustomer)
                                              .Where(m => m.DeleteFlag != true &&
                                                          m.ApplicationUser != null && m.ApplicationUser.DeleteFlag != true
                                              //&& m.CurrentRelationship != null && m.CurrentRelationship.DeleteFlag != true && // Khi add relationship from mục tiêu
                                              // m.TargetRelationship != null && m.TargetRelationship.DeleteFlag != true)
                                              )
                                              .OrderByDescending(x => x.CreatedDate)
                                              .ProjectTo<RelationshipDto>(_mapper.ConfigurationProvider)
                                              .AsNoTracking();

            if (!string.IsNullOrEmpty(request.RequestData.Roles))
            {
                request.RequestData.Roles = request.RequestData.Roles.Trim();
                request.RequestData.Roles = WebUtility.UrlDecode(request.RequestData.Roles);
                List<Guid> listRoleId = request.RequestData.Roles.Split(",").Select(s => Guid.Parse(s.Trim())).ToList();
                query = query.Where(x => listRoleId.Contains((Guid)x.ApplicationRoleId));
            }

            if (request.RequestData.RoleType != null)
            {
                var user = request.userId;

                if (request.RequestData.RoleType == RoleType.MYSELF.ToString())
                {
                    await _PermissionService.HasPermission(
                        MenuType.Sale_MQH,
                        FeatureType.MYSELF,
                        user,
                        true
                    );
                    query = query.Where(s => s.ApplicationUser.Id == request.RequestData.UserId);
                }
                else if (request.RequestData.RoleType == RoleType.MANAGER.ToString())
                {
                    await _PermissionService.HasPermission(
                        MenuType.Sale_MQH,
                        FeatureType.MANAGER,
                        user,
                        true
                    );
                    var ids = await _UserService.GetUserIdByRolePosition(RolePositionEnum.MANAGER.ToString());
                    query = query.Where(s => ids.Contains(s.ApplicationUser.Id));
                }
                else if (request.RequestData.RoleType == RoleType.EMPLOYEE.ToString())
                {
                    await _PermissionService.HasPermission(
                        MenuType.Sale_MQH,
                        FeatureType.EMPLOYEE,
                        user,
                        true
                    );
                    var ids = await _UserService.GetUserIdByRolePosition(RolePositionEnum.EMPLOYEE.ToString());
                    query = query.Where(s => ids.Contains(s.ApplicationUser.Id));
                    if (request.RequestData.UserId != null)
                    {
                        query = query.Where(s => s.ApplicationUser.Id == request.RequestData.UserId);
                    }
                    query = query.Where(s => s.ApplicationUser.Id != request.userId);
                }
                else
                {
                    throw new Exception($"Role type không hợp lệ: {request.RequestData.RoleType}");
                }
            }

            if (!string.IsNullOrEmpty(request.RequestData.TextSearch))
            {
                query = query.Where(s => s.CustomerName.ToLower().Contains(request.RequestData.TextSearch.ToLower()) ||
                                         s.Customer.ToLower().Contains(request.RequestData.TextSearch.ToLower()));
            }

            if (request.RequestData.StatusId != null)
            {
                query = query.Where(s => s.RelationshipStatus.Id == request.RequestData.StatusId);
            }

            if (request.RequestData.Time != null)
            {
                query = query.Where(s => s.CreatedDate.Year == request.RequestData.Time.Value.Year);
            }

            return Result<PaginatedList<RelationshipDto>>.Success(await query.PaginatedListAsync(request.RequestData.PageIndex, request.RequestData.PageSize));
        }
    }
}
