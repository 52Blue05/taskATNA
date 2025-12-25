using Microsoft.AspNetCore.Mvc;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.Identity;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Interfaces.Services;
using System.Reflection;

namespace Sale_Saas.API.Controllers
{
    public class ApplicationRoleDetailsController : BaseController
    {
        private readonly IApplicationRoleDetailService _applicationRoleDetailService;
        private readonly ILoggerService _loggerService;

        public ApplicationRoleDetailsController(IApplicationRoleDetailService applicationRoleDetailService, ILoggerService loggerService)
        {
            _applicationRoleDetailService = applicationRoleDetailService;
            _loggerService = loggerService;
        }

        [HttpPost("add-or-update")]
        public async Task<IActionResult> AddOrUpdate([FromBody] ApplicationRoleDetailRequest request)
        {            
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await _applicationRoleDetailService.AddOrUpdateAsync(request));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpGet("get-list-by-userid/{userId}")]
        public async Task<IActionResult> GetListByUserId(string userId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await _applicationRoleDetailService.GetListByApplicationUserId(userId));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }            
        }
        [HttpGet("get-list-by-applicationroleid/{applicationRoleId}")]
        public async Task<IActionResult> GetListByApplicationRoleId(Guid applicationRoleId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await _applicationRoleDetailService.GetListByApplicationRoleId(applicationRoleId.ToString()));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }            
        }
		
	}
}
