using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Domain.Entities.Tenant
{
    public class ModuleTenant : BaseAuditableEntityTenant
    {
        [Key, MaxLength(250)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Code { get; set; }
        [MaxLength(250)]
        public string Name { get; set; }
        [MaxLength(250)]
        public string? NameEn { get; set; }
        public string? Icon { set; get; }
        public string? Link { set; get; }
        public string? NameController { set; get; }
        public string? NameAction { set; get; }
        public string? Parameter { set; get; }
        public string? ParentId { set; get; }
        public string? BreadcrumbNavigation { set; get; }

		public ICollection<PlanServiceModuleTenant>? PlanServiceModuleTenants { set; get; }

	}
}
