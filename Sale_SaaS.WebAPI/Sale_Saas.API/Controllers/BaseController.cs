using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Sale_Saas.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Sale_Saas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
	//[Authorize]
	public class BaseController : ControllerBase
    {
        private IMediator _mediator;
        protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>();
        protected string GetTenantId()
        {
            try
            {
                var claims = User.Claims.ToList();
                var sid = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/tenant")?.Value ?? "";
                return sid;
            }
            catch
            {
                return "";
            }
        }

        protected Guid? GetCurrentUser()
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                string authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault() ?? "";
                if (string.IsNullOrEmpty(authHeader)) return null;
                authHeader = authHeader.Replace("Bearer ", "");
                var jsonToken = handler.ReadToken(authHeader);
                var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                if (tokenS == null) { return null; }
                var id = tokenS.Claims.First(claim => claim.Type == "ApplicationUserId").Value;
                return Guid.Parse(id);
                //var claims = User.Claims.ToList();
                //var sid = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/tenant").Value;
                //return sid;
            }
            catch
            {
                return null;
            }
        }

        protected string? GetCurrentUserAdmin()
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                string authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault() ?? "";
                if (string.IsNullOrEmpty(authHeader)) return null;
                authHeader = authHeader.Replace("Bearer ", "");
                var jsonToken = handler.ReadToken(authHeader);
                var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                if (tokenS == null) { return null; }
                var isSaaS = tokenS.Claims.First(claim => claim.Type == "IsSaaS").Value;
                bool isAdmin = bool.Parse(isSaaS);
                if (isAdmin)
                {
                    var email = tokenS.Claims.First(claim => claim.Type == "Email").Value;
                    return email;
                }
                return null;
                //var claims = User.Claims.ToList();
                //var sid = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/tenant").Value;
                //return sid;
            }
            catch
            {
                return null;
            }
        }

        protected Guid? GetGroupTenant()
		{
			try
			{
				var handler = new JwtSecurityTokenHandler();
				string authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault() ?? "";
				if (string.IsNullOrEmpty(authHeader)) return null;
				authHeader = authHeader.Replace("Bearer ", "");
				var jsonToken = handler.ReadToken(authHeader);
				var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
				if (tokenS == null) { return null; }
				var id = tokenS.Claims.First(claim => claim.Type == "GroupTenant").Value;
				return Guid.Parse(id);
			}
			catch
			{
				return null;
			}
		}

		protected string GetCurrentUserName()
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                string authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault() ?? "";
                if (string.IsNullOrEmpty(authHeader)) return "";
                authHeader = authHeader.Replace("Bearer ", "");
                var jsonToken = handler.ReadToken(authHeader);
                var tokenS = handler.ReadToken(authHeader) as JwtSecurityToken;
                if (tokenS == null) { return ""; }
                var userName = tokenS.Claims.First(claim => claim.Type == "Email").Value; //"UserName"
                return userName;             
            }
            catch
            {
                return "";
            }
        }

        protected string GetCurrentTenant()
        {
            try
            {
				if (HttpContext.Request.Query.ContainsKey("tenant"))
				{
					return HttpContext.Request.Query["tenant"].ToString();
				}
				var handler = new JwtSecurityTokenHandler();
                string authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault() ?? "";
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
        protected string GetLocale()
        {
			try
			{
				if (HttpContext.Request.Query.ContainsKey("locale"))
				{
					return HttpContext.Request.Query["locale"].ToString();
				}
                return LocaleEnum.vi_VN.ToString();
			}
			catch
			{
				return LocaleEnum.vi_VN.ToString();
			}
		}

        protected string GetCurrentToken()
        {
            try
            {
               
                var handler = new JwtSecurityTokenHandler();
                string authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault() ?? "";
                if (string.IsNullOrEmpty(authHeader)) return "";
                authHeader = authHeader.Replace("Bearer ", "");
            
                return authHeader;
            }
            catch
            {
                return "";
            }
        }
    }
}
