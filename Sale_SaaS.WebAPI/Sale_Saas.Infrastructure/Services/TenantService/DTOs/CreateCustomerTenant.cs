using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Infrastructure.Services.TenantService.DTOs
{
    public class CreateUser
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<TenantRole> TenantRoles { get; set; }
        public string? Email { set; get; }
        public string? Phone { get; set; }
		public string? Address { get; set; }
		public string? Code { get; set; }
		public bool? IsAdmin { get; set; }
    }

    public class TenantRole
    {
        public string TenantId { get; set; }
        public List<string>? ApplicationRoleNames { set; get; }
    }

    public class CreateUserSuper
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }       
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Code { get; set; }
        public string TenantId { get; set; }
        public bool? IsAdmin { get; set; }
    }
}
