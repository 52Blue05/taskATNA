using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Models.Identity;
using Sale_Saas.Domain.Entities.Tenant;
using Sale_Saas.Infrastructure.Services.TenantService.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Infrastructure.Services.TenantService
{
    public interface IUserAdminService
    {
        UserAdmin CreateUserAdmin(UserAdmin request);
        UserAdmin GetUserAdmin(string userName);
        PaginatedList<UserAdmin> GetListUserAdmin(GetListWithPaginationQueryRequest request);
        Result<LoginDto> Login(UserAdminDto request);
    }
}
