using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sale_Saas.Infrastructure.Services;
using System.IdentityModel.Tokens.Jwt;

namespace Sale_Saas.Infrastructure.Middleware
{
     public class TenantResolver : ControllerBase
     {
          private readonly RequestDelegate _next;
          public TenantResolver(RequestDelegate next)
          {
               _next = next;
          }

          // Get Tenant Id from incoming requests 
          public async Task InvokeAsync(HttpContext context, ICurrentTenantService currentTenantService)
          {
               //context.Request.Headers.TryGetValue("tenant", out var tenantFromHeader); // Tenant Id from incoming request header
               var tenantFromHeader = context.Request.Query["tenant"].ToString();
               if (string.IsNullOrEmpty(tenantFromHeader)) tenantFromHeader = GetTenantId(context);
               if (string.IsNullOrEmpty(tenantFromHeader) == false)
               {
                    await currentTenantService.SetTenant(tenantFromHeader);
               }
               await _next(context);
          }
          private string GetTenantId(HttpContext context)
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
                    var id = tokenS.Claims.First(claim => claim.Type == "Tenant").Value;
                    return id;
                    //var claims = User.Claims.ToList();
                    //var sid = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/tenant").Value;
                    //return sid;
               }
               catch
               {
                    return null;
               }
          }

     }
}
