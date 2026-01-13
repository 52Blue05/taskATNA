using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Domain.Entities
{
	public class FeatureMenu
	{
		public Guid Id { get; set; }
		public string? FeatureId { get; set; }
		public Feature? Feature { get; set; }
		public Guid? MenuId { get; set; }
		public Menu? Menu { get; set; }
		public ICollection<RolePositionFeatureMenu>? RolePositionFeatureMenus { get; set; }
	}
}
