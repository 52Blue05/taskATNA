using Microsoft.AspNetCore.Mvc;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Infrastructure.Data;
using Sale_Saas.Infrastructure.Services.TenantService;
using Sale_Saas.Infrastructure.Services.TenantService.DTOs;
using System.Reflection;

namespace Sale_Saas.API.Controllers.Tenant
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModuleTenantController : ControllerBase
    {
        private readonly IModuleTenantService _moduleTenantService;
        private readonly IApplicationUserService _applicationUserService;
        private readonly ILoggerService _loggerService;

        public ModuleTenantController(IModuleTenantService ModuleTenantService, IApplicationUserService applicationUserService, 
                                            IApplicationDbContextInitialiser applicationInitialiserService, ILoggerService loggerService)
        {
            _moduleTenantService = ModuleTenantService;
            _applicationUserService = applicationUserService;
            _loggerService = loggerService;
        }

        // Create a new tenant
        [HttpPost]
        public async Task<IActionResult> Post(CreateModuleTenantDto request)
        {
            try
            {
                var result = _moduleTenantService.CreateModuleTenant(request);
                //if (!string.IsNullOrEmpty(result.ConnectionString))
                //{
                //    await _applicationInitialiserService.SeedAsync(result.ConnectionString);
                //}
    
                return Ok(result);
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-list-all")]
        public async Task<Result<List<ModuleTenantDto>>> GetListAll()
        {
            try
            {
                var result = await _moduleTenantService.GetListAllModuleTenant();

                return Result<List<ModuleTenantDto>>.Success(result.Data);
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Result<List<ModuleTenantDto>>.Failure(ex.Message);
            }
        }

        [HttpPost("get-list")]
        public async Task<IActionResult> Get(GetListWithPaginationQueryRequest request)
        {
            try
            {
                var result = _moduleTenantService.GetListModuleTenant(request);           
                return Ok(result);
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-by-code")]
        public async Task<IActionResult> GetByCode(string code)
        {
            try
            {
                var result = _moduleTenantService.GetModuleTenant(code);
                return Ok(result);
            }
            catch (Exception ex) 
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpPost("innitial")]
        public async Task<IActionResult> Innitial()
        {
            try
            {
                var result = _moduleTenantService.TrySeedAsync();
                return Ok(result);
            }
            catch(Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }
    }
}
