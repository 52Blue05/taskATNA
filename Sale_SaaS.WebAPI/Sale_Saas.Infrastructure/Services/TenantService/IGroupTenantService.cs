using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Models.GroupTenant;
using Sale_Saas.Domain.Entities.Tenant;
using Sale_Saas.Infrastructure.Services.TenantService.DTOs;

namespace Sale_Saas.Infrastructure.Services.TenantService;

public interface IGroupTenantService
{
	Task<Result<PaginatedList<GroupTenantHomeDto>>> GetListWithPaginationQuery(GetListWithPaginationQueryRequest request);
	Task<Result<string>> DeleteByIds(DeleteRequest request);
	Task<Result<GroupTenantHomeDto>> GetById(Guid id);
	Task<Result<GroupTenantHomeDto>> AddAsync(GroupTenantAddOrUpdateRequest request, Guid userId);
	//Task<Result<GroupTenantHomeDto>> UpdateAsync(GroupTenantAddOrUpdateRequest request, Guid userId);
	Task<Result<GroupTenantExtendPlanServiceDto>> ExtendPlanService(Guid id);
	Task<Result<GroupTenantDto>> AddExtendPlanService(Guid userId, Guid id);
	Task<bool> UpdateMenuActiveAsync(string newConnectionString, List<OrderDetail> orderDetails, bool isUpdate = false, Guid groupId = default);
	Task<IEnumerable<string>> GetConnectionStringsByGroupTenantAsync(Guid groupId);
	Task<bool> UpdateMenuActiveOfTenantAsync(string connectionString, List<OrderDetail> orderDetails, ApplicationDbContext context, bool isUpdate = false);
	Task<Result<GroupTenantHomeDto>> UpdatePassword(UpdatePasswordRequest request);
	Task<Result<GroupTenantHomeDto>> UpdatePlanServiceAsync(Guid userId, GroupTenantUpdatePlanServiceRequest request);
	Task<Result<GroupTenantHomeDto>> UpdateGroupTenantAsync(Guid userId, GroupTenantAddOrUpdateBaseRequest request);
	Task<string> GetCurrentPlanServiceNameActive(Guid groupId);
	Task<Result<GroupTenant>> FindByDomainAsync(string domain);
}
