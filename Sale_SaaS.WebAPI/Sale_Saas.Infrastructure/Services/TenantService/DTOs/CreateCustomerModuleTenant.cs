using Sale_Saas.Domain.Entities.Tenant;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Infrastructure.Services.TenantService.DTOs
{
    public class CreateCustomerModuleTenant
    {
        public Guid CustomerTenantId { get; set; }
        public List<string> ModuleCodes { get; set; }
        public string TenantId { get; set; }
    }
}
