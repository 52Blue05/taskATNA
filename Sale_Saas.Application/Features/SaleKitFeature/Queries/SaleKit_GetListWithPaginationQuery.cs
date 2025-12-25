using Sale_Saas.Application.Common.Mappings;
using Sale_Saas.Application.Features.SaleKitFeature.Dto;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.SaleKit;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Features.SaleKitFeature.Queries
{
    public record SaleKit_GetListWithPaginationQuery(SaleKitGetListWithPaginationQueryRequest RequestData) : IRequest<Result<PaginatedList<SaleKitDto>>>;

    public class SaleKit_GetListWithPaginationQueryHandler : IRequestHandler<SaleKit_GetListWithPaginationQuery, Result<PaginatedList<SaleKitDto>>>
    {
        private readonly IApplicationRoleService _roleService;
		private readonly IFeaturePermissionService _permissionService;
		private readonly IApplicationDbContext _context;
        private readonly IApplicationUserService _userService;
        private readonly IMapper _mapper;
        public SaleKit_GetListWithPaginationQueryHandler(
            IMapper mapper, IApplicationDbContext context, IApplicationRoleService roleService, IFeaturePermissionService permissionService)
        {
            _context = context;
            _mapper = mapper;
            _roleService = roleService;
            _permissionService = permissionService;
        }

        public async Task<Result<PaginatedList<SaleKitDto>>> Handle(SaleKit_GetListWithPaginationQuery request, CancellationToken cancellationToken)
        {
            if(request.RequestData.UserId == null)
            {
                throw new ApplicationException("Không tìm thấy dữ liệu");
            }
            //var access = await _permissionService.HasPermission(MenuType.Sale_SK_PQ, FeatureType.SALEKIT_XEMPHANQUYEN, (Guid)request.RequestData.UserId);
            
            var query = _context.SaleKits.Where(m => m.DeleteFlag != true)
                                         .OrderByDescending(x => x.CreatedDate)
                                         .ProjectTo<SaleKitDto>(_mapper.ConfigurationProvider)
                                         .AsNoTracking();

			//if (!access)
			//{
			//	var roles = await _roleService.GetListRoleByUserId((Guid)request.RequestData.UserId);
   //             if (!roles.Any()) throw new ApplicationException("Bạn không có quyền truy cập tài liệu");
   //             var ids = await _context.ApplicationRole_SaleKits
   //                             .Where(s => s.ApplicationRoleId != null &&
   //                                         roles.Select(s => s.Id).ToList()
   //                                         .Contains((Guid)s.ApplicationRoleId) && s.Access == true)
   //                             .Select(s => s.SaleKitId)
   //                             .ToListAsync();

   //             query = query.Where(s => ids.Contains(s.Id));
			//}

			if (request.RequestData.ParentId != null && request.RequestData.ParentId != Guid.Empty && string.IsNullOrEmpty(request.RequestData.TextSearch))
            {
                query = query.Where(s => s.ParentId == request.RequestData.ParentId);
            }

			if ((request.RequestData.ParentId == null || request.RequestData.ParentId == Guid.Empty) && string.IsNullOrEmpty(request.RequestData.TextSearch))
			{
				query = query.Where(s => s.ParentId == null);
			}

			if (!string.IsNullOrEmpty(request.RequestData.TextSearch))
            {
                query = query.Where(s => s.Name.ToLower().Contains(request.RequestData.TextSearch.ToLower()) ||
                                         s.FileName.ToLower().Contains(request.RequestData.TextSearch.ToLower()) ||
										 s.OriginalFileName.ToLower().Contains(request.RequestData.TextSearch.ToLower()));
            }

            var result = await query.ToListAsync();

            foreach (var item in result)
            {
                item.CreatedUser = await _userService.GetUserBasicInforById(item.LastModifiedApplicationUserId ?? Guid.Empty);
            }

            return Result<PaginatedList<SaleKitDto>>.Success(new PaginatedList<SaleKitDto>(result, result.Count, request.RequestData.PageIndex, request.RequestData.PageSize));
        }
    }
}
