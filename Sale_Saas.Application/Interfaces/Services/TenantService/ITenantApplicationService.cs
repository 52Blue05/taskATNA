using Sale_Saas.Application.Models.Tenant;

namespace Sale_Saas.Application.Interfaces.Services.TenantService;

public interface ITenantApplicationService
{
    Task<string> GetTenantById(string id);
    List<TenantWithAllUserDto> GetAllUserOfTenant(string id, int? PageIndex = null, int? PageSize = null);
    List<UserWithAllTenantDto> GetListTenantOfUser(Guid id, bool cnStr = true);
    List<TenantDto> GetListTenantByGroupTenant(Guid groupTenantId);
    List<string> GetListConnectionStringByUserAndTenant(List<TenantDto> listTenant, Guid userId);
}
