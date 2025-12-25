using Sale_Saas.Application.Models.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Infrastructure.Services.TenantService.DTOs
{
    public class UserLoginDto
    {
        public Guid Id { get; set; } 
        public string UserName { get; set; }
        public bool? IsAdmin { get; set; }
        public List<UserTenantDto> UserTenantDtos { get; set; }
        public Guid? GroupTenantId { get; set; }

    }

    public class UserTenantDto
    {
        public string TenantId { get; set; }
        public string TenantName { get; set; }
        public string? ConnectString { get; set; }
        public string? Logo { get; set; }
        public Guid? ApplicationUserId { get; set; }
        public int? Count { get; set; }
    }

    public class UserTenantMobileDto
    {
        public string TenantId { get; set; }
        public string TenantName { get; set; }
        public string? ConnectString { get; set; }
        public string? Logo { get; set; }
        public Guid? ApplicationUserId { get; set; }
        public int? Count { get; set; }
        public List<ApplicationRoleDto>? Roles { get; set; }
    }

    public class AllUserTenantDto
    {
        public string UserName { get; set; }
        public string TenantId { get; set; }
        public string TenantName { get; set; }
        public string? ConnectString { get; set; }
        public Guid? ApplicationUserId { get; set; }
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string? UserName { get; set; }
        public string? Code { get; set; }
        public string? FullName { get; set; }
    }
}
