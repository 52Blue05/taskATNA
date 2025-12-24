using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Domain.Entities
{
    public class Menu : BaseAuditableEntity
    {
        public Menu()
        {
            SortOrder = 0;
            IsActivite = false;
        }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? NameEn { get; set; }
        public int SortOrder { get; set; }
        public string? Icon { set; get; }
        public string? Link { set; get; }
        public string? NameController { set; get; }
        public string? NameAction { set; get; }
        public string? Parameter { set; get; }
        public Guid? ParentId { set; get; }
        public ICollection<ApplicationRoleDetail>? RoleDetails { set; get; }
        public string? BreadcrumbNavigation { set; get; }
        public bool? IsActivite { set; get; }
		public ICollection<FeatureMenu>? FeatureMenus { get; set; }

	}
}
