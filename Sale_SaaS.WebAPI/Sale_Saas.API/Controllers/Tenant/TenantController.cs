using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Utilities;
using Sale_Saas.Infrastructure.Services.TenantService;
using Sale_Saas.Infrastructure.Services.TenantService.DTOs;
using System.Reflection;

namespace Sale_Saas.API.Controllers.Tenant
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantController : BaseController
    {
        private readonly ITenantService _tenantService;
        private readonly IApplicationDbContextInitialiser _applicationInitialiserService;
        private readonly IUserService _userService;
        private readonly IApplicationUserService _applicationUserService;
        private readonly ILoggerService _loggerService;
        private readonly IFileStorageService _storageService;
        public TenantController(ITenantService tenantService,
             IApplicationDbContextInitialiser applicationInitialiserService,
             IUserService customerTenantService, ILoggerService loggerService,
             IApplicationUserService applicationUserService,
             IFileStorageService storageService)
        {
            _tenantService = tenantService;
            _applicationInitialiserService = applicationInitialiserService;
            _userService = customerTenantService;
            _loggerService = loggerService;
            _storageService = storageService;
            _applicationUserService = applicationUserService;
        }

        // Create a new tenant
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Post(CreateTenantRequest request)
        {
            try
            {
                var result = await _tenantService.CreateTenant(request, true);
                //if(result!=null)
                //{
                //	var customer = _userService.CreateUser(new CreateUser { UserName= "admin@gmail.com", Email= "admin@gmail.com", FirstName = "Admin",
                //		LastName = "Super",
                //		Password="123",
                //		TenantRoles=new List<TenantRole> { new TenantRole() { TenantId= request.Id } },
                //		IsAdmin=true,
                //		},true);
                //	if (!string.IsNullOrEmpty(result.ConnectionString))
                //	{
                //		var appUserId=await _applicationInitialiserService.SeedAsync(result.ConnectionString);
                //		_userService.UpdateUserTenant(customer.Data[0].UserName, result.Id, appUserId);
                //	}
                //	return Ok(customer);
                //}
                return Ok(result);
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("create-tenant-by-user")]
        public async Task<IActionResult> CreateTenantByUser(CreateTenantByUserRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                request.Id = StringHelper.GenerateCode();
                var currentUserName = GetCurrentUserName();
                var userCurrent = _userService.GetUserAdminByUserName(currentUserName);
                if (userCurrent == null)
                {
                    return Ok(Result<string>.Failure("Bạn không có quyền tạo tài khoản."));
                }
                var checkTenantName = await _tenantService.GetTenantInfoByTenantName(request.Name, userCurrent.GroupTenantId ?? new Guid());
                if (checkTenantName != null)
                    return Ok(Result<string>.Failure("Tên tổ chức đã tồn tại."));
                var logo = "";
                if (request.Logo != null)
                {
                    var response = await _storageService.UploadFileAsync(new List<IFormFile>() { request.Logo });
                    logo = response[0].ServerPath;
                }

                var result = await _tenantService.CreateTenant(new CreateTenantRequest()
                {
                    Id = request.Id,
                    Name = request.Name,
                    Isolated = request.Isolated ?? false,
                    ConnectionString = request.ConnectionString,
                    Logo = logo,
                    ThemeColor = request.ThemeColor,
                    GroupTenantId = GetGroupTenant()
                });
                if (result != null && !string.IsNullOrEmpty(result.ConnectionString))
                {
                    _applicationUserService.SetConnectDB(result.ConnectionString);
                    await _userService.AddToTenant(new Application.Models.Identity.AddUserToTenantDto()
                    {
                        Id = userCurrent.Id,
                        TenantId = result.Id
                    });
                }
                return Ok(Result<Domain.Entities.Tenant.Tenant>.Success(result));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }

        }

        [HttpPut("update-tenant-by-user")]
        public async Task<IActionResult> UpdateTenantByUser([FromForm] UpdateTenantByUserRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var currentUserName = GetCurrentUserName();
                var userCurrent = _userService.GetUserAdminByUserName(currentUserName);
                if (userCurrent == null)
                {
                    return Ok(Result<string>.Failure("Bạn không có quyền tạo tài khoản."));
                }
                var logo = "";
                if (request.Logo != null)
                {
                    var response = await _storageService.UploadFileAsync(new List<IFormFile>() { request.Logo });
                    logo = response[0].ServerPath;
                }

                var result = await _tenantService.UpdateTenant(
                    new CreateTenantRequest()
                    {
                        Id = request.Id,
                        Name = request.Name,
                        Logo = logo,
                        ThemeColor = request.ThemeColor
                    }
                );
                return Ok(Result<Domain.Entities.Tenant.Tenant>.Success(result));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpDelete("delete-from-tenant-by-ids/{ids}/{ApplicationUserId}")]
        public async Task<IActionResult> DeleteByIds(string ids, Guid ApplicationUserId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var currentUserName = GetCurrentUserName();
                var userCurrent = _userService.GetUserAdminByUserName(currentUserName);
                if (userCurrent == null)
                {
                    return Ok(Result<string>.Failure("Bạn không có quyền tạo tài khoản."));
                }
                var request = new DeleteRequest()
                {
                    Ids = ids.Split(",").ToList(),
                    ApplicationUserId = ApplicationUserId
                };
                return Ok(Result<string>.Success(await _tenantService.DeleteByIds(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                _loggerService.WriteInfoLog("Start Get List Tenant");
                var result = _tenantService.GetListTenant();
                return Ok(Result<List<Domain.Entities.Tenant.Tenant>>.Success(result));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                _loggerService.WriteInfoLog("Start Get List Tenant");
                var group = GetGroupTenant() ?? Guid.Empty;
                var result = _tenantService.GetListTenantByGroup(group);
                return Ok(Result<List<Domain.Entities.Tenant.Tenant>>.Success(result));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }

        }

        [HttpGet("get-by-tenat-id")]
        public async Task<IActionResult> GetByTenatId(string tenantId)
        {
            try
            {
                var result = _tenantService.GetTenantInfoByTenantId(tenantId);
                return Ok(Result<Domain.Entities.Tenant.Tenant>.Success(result));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-tenat-by-sub-domain")]
        public async Task<IActionResult> GetTenantIdBySubDomain(string subDomain)
        {
            try
            {
                var result = _tenantService.GetTenantIdBySubDomain(subDomain);
                return Ok(Result<string>.Success(result));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-list-tenant-with-deleted")]
        [AllowAnonymous]
        public async Task<IActionResult> GetListTenantWithDeleted()
        {
            try
            {
                var targets = await _tenantService.GetListTenantWithDeleted();
                //var listId= targets.Data.Select(x => x.Id).ToList();
                return Ok(targets);
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpDelete("drop-tenant")]
        [AllowAnonymous]
        public async Task<IActionResult> DropTenant(DropDbRequest request)
        {
            try
            {
                return Ok(await _tenantService.DropTenant(request));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("update-feature-menu-tenant")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateRemoveFeatureMenu()
        {
            try
            {
                return Ok(await _tenantService.UpdateRemoveFeatureMenu());
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("update-feature-menu-tenant-by-tenant")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateRemoveFeatureMenuByTenant()
        {
            try
            {
                var currentTenant = GetCurrentTenant();
                return Ok(await _tenantService.UpdateRemoveFeatureMenuByTenant(currentTenant));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }
    }
}
