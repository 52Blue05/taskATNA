using Sale_Saas.Application.Common.Models;
using Sale_Saas.Domain.Entities.Tenant;
using Sale_Saas.Infrastructure.Services.TenantService.DTOs;

namespace Sale_Saas.Infrastructure.Services.TenantService
{
    public interface ITenantService
    {
        Task<Tenant> CreateTenant(CreateTenantRequest request, bool isAdminCreated = false);
        Task<Tenant> UpdateTenant(CreateTenantRequest request);
        Task<string> DeleteByIds(DeleteRequest request);
        Tenant GetTenantInfoByTenantId(string tenantId);
        List<Tenant> GetListTenant(bool cnStr = true);
        List<Tenant> GetListTenantByGroup(Guid groupTenant);
        string GetTenantNameByConStr(string connectStr);
        string GetTenantIdBySubDomain(string subDomain);
        Task<List<string>> GetListTenantByListIds(string Ids);
        Task<string> GetTenantById(string id);
        Task<string> InnitTenantDatabase(string id, string sConnect, List<string> listMenuActive = null);
        Task<Tenant> GetTenantInfoByTenantName(string tenantname, Guid groupTenantId);
        Task<Result<List<Tenant>>> GetListTenantWithDeleted();
        Task<Result<string>> DropTenant(DropDbRequest request);
        List<Tenant> GetListTenantByAdmin(string tenantId);
        Task<bool> UpdateRemoveFeatureMenu();
        Task<Result<bool>> UpdateRemoveFeatureMenuByTenant(string tenantId);
    }

}
