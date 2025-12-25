using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Domain.Entities.Tenant
{
    public class UserAdmin : BaseAuditableEntityTenant
    {
        [Key, MaxLength(125)]
        public string UserName { get; set; }
        [MaxLength(125)]
        public string Password { get; set; }
        [MaxLength(125)]
        public string FullName { get; set; }
        [MaxLength(125)]
        public string Email { get; set; }
    }
}
