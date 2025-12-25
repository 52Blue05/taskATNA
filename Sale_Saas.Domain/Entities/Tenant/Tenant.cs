using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Domain.Entities.Tenant
{
    public class Tenant : BaseAuditableEntityTenant
    {
        [Key, MaxLength(250)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        public string Name { get; set; }
        public string? Host { get; set; }
        public string? SubDomain { get; set; }
        public string? Logo { get; set; }
        public string? ThemeColor { get; set; }
        public string? ConnectionString { get; set; }
        public bool? IsAdminCreated { get; set; }

		[ForeignKey("GroupTenantId")]
		public GroupTenant? GroupTenant { get; set; }
		public Guid? GroupTenantId { get; set; }
    }
}
