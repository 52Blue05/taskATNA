using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Infrastructure.Services
{
    public interface ICurrentTenantService
    {
        string? ConnectionString { get; set; }
        string? TenantId { get; set; }
        public Task<bool> SetTenant(string tenant);
    }
}
