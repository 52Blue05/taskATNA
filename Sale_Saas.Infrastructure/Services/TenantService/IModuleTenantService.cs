using Sale_Saas.Application.Common.Models;
using Sale_Saas.Domain.Entities.Tenant;
using Sale_Saas.Infrastructure.Services.TenantService.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Infrastructure.Services.TenantService
{
    public interface IModuleTenantService
    {
        CreateModuleTenantDto CreateModuleTenant(CreateModuleTenantDto request);
        ModuleTenant GetModuleTenant(string code);
        PaginatedList<ModuleTenant> GetListModuleTenant(GetListWithPaginationQueryRequest request);
        bool TrySeedAsync();

        Task<Result<List<ModuleTenantDto>>> GetListAllModuleTenant(); 
    }
}
