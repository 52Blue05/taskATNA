using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Application.Models.Tenant
{
	public class TenantDto
	{
		public string Id { get; set; }
		public string? Name { get; set; } = "";
		public string? Logo { get; set; } = "";
		public string? ThemeColor { get; set; } = "";
	}
}
