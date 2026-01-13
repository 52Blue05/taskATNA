using Microsoft.AspNetCore.Authorization;

namespace Sale_Saas.Infrastructure.Authentication;

public sealed class HasPermissionAttribute : AuthorizeAttribute
{
	public HasPermissionAttribute(string permission) : base(policy: permission) { }
}
