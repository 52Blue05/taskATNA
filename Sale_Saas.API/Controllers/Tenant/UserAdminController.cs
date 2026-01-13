using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Domain.Entities.Tenant;
using Sale_Saas.Infrastructure.Data;
using Sale_Saas.Infrastructure.Services.TenantService;
using Sale_Saas.Infrastructure.Services.TenantService.DTOs;
using System.Reflection;

namespace Sale_Saas.API.Controllers.Tenant
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserAdminController : ControllerBase
    {
        private readonly IUserAdminService _UserAdminService;
        private readonly IApplicationUserService _applicationUserService;
        private readonly ILoggerService _loggerService;

        public UserAdminController(IUserAdminService UserAdminService, IApplicationUserService applicationUserService, 
                                        IApplicationDbContextInitialiser applicationInitialiserService, ILoggerService loggerService)
        {
            _UserAdminService = UserAdminService;
            _applicationUserService = applicationUserService;
            _loggerService = loggerService;
        }

        // Create a new tenant
        [HttpPost]
        public async Task<IActionResult> Post(UserAdmin request)
        {
            try
            {
                var result = _UserAdminService.CreateUserAdmin(request);         
                return Ok(result);
            }
            catch(Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("get-list")]
        public async Task<IActionResult> Get(GetListWithPaginationQueryRequest request)
        {
            try
            {
                var result = _UserAdminService.GetListUserAdmin(request);           
                return Ok(result);
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("login-admin")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByCode(UserAdminDto login)
        {
            try
            {
                var result = _UserAdminService.Login(login);      
                return Ok(result);
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }
       
    }
}
