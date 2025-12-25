using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Infrastructure.Services.TenantService.DTOs
{
    public class CreateTenantRequest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? Logo { get; set; }
        public string? ThemeColor { get; set; }
        public bool Isolated { get; set; }
        public string? ConnectionString { get; set; }
        public Guid? GroupTenantId { get; set; }
    }

	public class UpdateTenantByUserRequest
	{
		public string Id { get; set; }
		public string? Name { get; set; }
		public IFormFile? Logo { get; set; }
		public string? ThemeColor { get; set; }
	}
	public class CreateTenantByUserRequest
    {
        public string? Id { get; set; }
        public string Name { get; set; }
		public IFormFile? Logo { get; set; }
		public string? ThemeColor { get; set; }
		public bool? Isolated { get; set; }
        public string? ConnectionString { get; set; }
    }
}
