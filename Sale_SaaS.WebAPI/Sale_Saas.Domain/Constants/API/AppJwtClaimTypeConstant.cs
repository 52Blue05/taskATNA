
using static System.Net.Mime.MediaTypeNames;

namespace Sale_Saas.Domain.Constants.API;

public static class AppJwtClaimTypeConstant
{
    public const string ApplicationUserId = nameof(ApplicationUserId);
    public const string UserName = nameof(UserName);
    public const string Avatar = nameof(Avatar);
    public const string FullName = nameof(FullName);
    public const string Email = nameof(Email);
    public const string ApplicationRoleDetails = nameof(ApplicationRoleDetails);
    public const string Roles = nameof(Roles);
    public const string Tenant = nameof(Tenant);
	public const string GroupTenant = nameof(GroupTenant);
    public const string IsSaaS = nameof(IsSaaS);
}
