using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Text.Json;

namespace Sale_Saas.Infrastructure.Authentication;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
	private readonly IServiceScopeFactory _serviceScopeFactory;
	public PermissionAuthorizationHandler(IServiceScopeFactory serviceScopeFactory)
	{
		_serviceScopeFactory = serviceScopeFactory;
	}
	protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
	{
		var httpContext = context.Resource as HttpContext;
		if (httpContext == null)
		{
			return;
		}

		Guid? id = GetCurrentUser(httpContext);
		if(id is null || id == Guid.Empty)
		{
			await ReturnResponse(httpContext);
			return;
		}
		using (IServiceScope scope = _serviceScopeFactory.CreateScope())
		{
			IFeaturePermissionService permissionService = scope.ServiceProvider
				.GetRequiredService<IFeaturePermissionService>();

			try
			{
				var tenant = GetCurrentTenant(httpContext);
				List<string> permissions = await permissionService.GetPolicy((Guid)id, tenant);
				if (permissions.Contains(requirement.Permission))
				{
					context.Succeed(requirement);
				}
				else
				{
					await ReturnResponse(httpContext);
					return;
				}
			}
			finally
			{
				if (permissionService is IDisposable disposableService)
				{
					disposableService.Dispose();
				}
			}
		}
	}

	private async Task ReturnResponse(HttpContext httpContext)
	{
		httpContext.Response.StatusCode = StatusCodes.Status200OK;
		httpContext.Response.ContentType = "application/json";
		var response = Result<string>.Failure("Bạn không có đủ quyền truy cập");
		var json = JsonSerializer.Serialize(response);
		await httpContext.Response.WriteAsync(json);
		await httpContext.Response.CompleteAsync();
	}

	protected Guid? GetCurrentUser(HttpContext context)
	{
		try
		{
			var handler = new JwtSecurityTokenHandler();
			string authHeader = context.Request.Headers["Authorization"];
			if (string.IsNullOrEmpty(authHeader)) return null;
			authHeader = authHeader.Replace("Bearer ", "");
			var jsonToken = handler.ReadToken(authHeader);
			var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
			if (tokenS == null) { return null; }
			var id = tokenS.Claims.First(claim => claim.Type == "ApplicationUserId").Value;
			return Guid.Parse(id);
		}
		catch
		{
			return null;
		}
	}

	protected string GetCurrentTenant(HttpContext context)
	{
		try
		{
			if (context.Request.Query.ContainsKey("tenant"))
			{
				return context.Request.Query["tenant"].ToString();
			}
			var handler = new JwtSecurityTokenHandler();
			string authHeader = context.Request.Headers["Authorization"];
			if (string.IsNullOrEmpty(authHeader)) return "";
			authHeader = authHeader.Replace("Bearer ", "");
			var jsonToken = handler.ReadToken(authHeader);
			var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
			if (tokenS == null) { return ""; }
			var tenant = tokenS.Claims.First(claim => claim.Type == "Tenant").Value;
			return tenant;
		}
		catch
		{
			return "";
		}
	}
}
