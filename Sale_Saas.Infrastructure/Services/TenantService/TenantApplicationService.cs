using Microsoft.Extensions.Configuration;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Interfaces.Services.TenantService;
using Sale_Saas.Application.Models.Tenant;
using Sale_Saas.Domain.Entities.Tenant;

namespace Sale_Saas.Infrastructure.Services.TenantService;

public class TenantApplicationService : ITenantApplicationService
{
    private readonly TenantDbContext _context;
    private readonly IConfiguration _configuration;
    
    public TenantApplicationService(TenantDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<string> GetTenantById(string id)
    {
        string idTenantNormalized = id.Trim();
        var connStr = await _context.Tenants.Where(t => t.Id == idTenantNormalized
                                                     && t.DeleteFlag != true)
                                            .Select(t => t.ConnectionString)
                                            .FirstOrDefaultAsync();

        return connStr;
    }

    public List<TenantWithAllUserDto> GetAllUserOfTenant(string id, int? PageIndex = null, int? PageSize = null)
    {
        var listTenant = _context.UserTenants.Where(x => x.TenantId == id && x.DeleteFlag != true).AsNoTracking().Select(x => x.TenantId).ToList();
        if (listTenant.Any())
        {
            var userTenantQuery = _context.UserTenants.Where(x => listTenant.Contains(x.TenantId) && x.DeleteFlag != true).OrderByDescending(s => s.CreatedDate).AsNoTracking();

            var distinctUserNames = userTenantQuery
                .GroupBy(x => x.UserName)
                .Select(g => g.FirstOrDefault().UserName);

            if (PageIndex != null && PageSize != null)
            {
                int skip = (PageIndex.Value - 1) * PageSize.Value;
                int take = PageSize.Value;

                distinctUserNames = distinctUserNames.Skip(skip).Take(take);
            }

            var userNamesList = distinctUserNames.ToList();

            var ids = userTenantQuery
                .Where(x => userNamesList.Contains(x.UserName) && x.DeleteFlag != true)
                .Select(x => x.ApplicationUserId)
                .ToList();

            var users = _context.UserTenants.Where(x => listTenant.Contains(x.TenantId) && ids.Contains(x.ApplicationUserId) && x.DeleteFlag != true).Include(x => x.Tenant).AsNoTracking().
                Select(x => new TenantWithAllUserDto()
                {
                    UserName = x.UserName,
                    TenantId = x.TenantId,
                    TenantName = x.Tenant.Name,
                    ApplicationUserId = x.ApplicationUserId,
                    ConnectString = x.Tenant.ConnectionString
                }).ToList();
            return users;
        }
        return null;
    }

    public List<UserWithAllTenantDto> GetListTenantOfUser(Guid id, bool cnStr = true)
    {
        var users = _context.UserTenants.Where(x => x.ApplicationUserId == id && x.DeleteFlag != true && x.Tenant != null && x.Tenant.DeleteFlag != true)
            .Include(x => x.Tenant).AsNoTracking()
            .OrderBy(s => s.Tenant!.CreatedDate)
            .Select(x => new UserWithAllTenantDto()
            {
                TenantId = x.TenantId,
                TenantName = x.Tenant!.Name,
                Logo = x.Tenant.Logo,
                ApplicationUserId = x.ApplicationUserId,
                ConnectString = cnStr ? x.Tenant.ConnectionString : ""
            }).ToList();
        if (cnStr == false)
        {
            /*var ids = users.Select(s => s.TenantId).ToList();
            var count = _context.UserTenants.*/
            foreach (var item in users)
            {
                item.Count = _context.UserTenants.Where(s => s.TenantId == item.TenantId && s.DeleteFlag != true).Count();
            }
        }
        return users;
    }

    public List<TenantDto> GetListTenantByGroupTenant(Guid groupTenantId)
    {
        var listTenant = _context.Tenants
                                .Where(s => s.DeleteFlag != true && s.GroupTenantId == groupTenantId)
                                .OrderByDescending(x => x.CreatedDate).AsNoTracking()
                                .Select(s => new TenantDto
                                {
                                    Id = s.Id,
                                    Name = s.Name,
                                }).ToList();
        return listTenant;
    }

    public List<string> GetListConnectionStringByUserAndTenant(List<TenantDto> listTenant, Guid userId)
    {
        var listTenantIds = listTenant.Select(x => x.Id).ToList();
        var listTenantIdsByUserAndTenant = _context.UserTenants
                                                .Where(s => s.DeleteFlag != true 
                                                         && s.ApplicationUserId == userId 
                                                         && listTenantIds.Contains(s.TenantId))
                                                .Select(s => s.TenantId)
                                                .ToList();
        var listConnectionString = _context.Tenants
                                .Where(s => s.DeleteFlag != true && listTenantIdsByUserAndTenant.Contains(s.Id))
                                .OrderByDescending(x => x.CreatedDate).AsNoTracking()
                                .Select(s => s.ConnectionString)
                                .ToList();

        if (listConnectionString == null || listConnectionString.Count <= 0)
        {
            throw new ApplicationException("Không có tổ chức nào");
        }

        return listConnectionString;
    }
}
