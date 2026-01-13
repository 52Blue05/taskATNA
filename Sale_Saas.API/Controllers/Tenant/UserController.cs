using Microsoft.AspNetCore.Mvc;
using Sale_Saas.Infrastructure.Services.TenantService.DTOs;
using Sale_Saas.Infrastructure.Services.TenantService;
using Sale_Saas.Application.Models.Identity;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.MenuFeature.Commands;
using Sale_Saas.Application.Features.MenuFeature.Dto;
using Sale_Saas.Infrastructure.Services;
using Sale_Saas.Application.Utilities;
using Sale_Saas.Domain.Entities.Tenant;
using Sale_Saas.Application.Interfaces.Services;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Org.BouncyCastle.Crypto;
using Sale_Saas.Domain.Entities;

namespace Sale_Saas.API.Controllers.Tenant
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : BaseController
    {
        private readonly ITenantService _tenantService;
        private readonly IUserService _userService;
        private readonly IApplicationUserService _applicationUserService;
        private readonly ICurrentTenantService _currentTenantService;
        private readonly ILoggerService _loggerService;

        public UserController(ITenantService tenantService, IUserService customerTenantService, IServiceProvider serviceProvider,
            IApplicationUserService applicationUserService,  ICurrentTenantService currentTenantService,
            ILoggerService loggerService)
        {
            _tenantService = tenantService;
            _userService = customerTenantService;
            _applicationUserService = applicationUserService;
            _currentTenantService= currentTenantService;
            _loggerService = loggerService;
        }

        [HttpPost("get-list-customer-tenant")]
        public async Task<IActionResult> GetListCustomerTenant(GetListWithPaginationQueryRequest request)
        {
            try
            {
                var result = _userService.GetListUser(request);           
                return Ok(result);
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-list-tenant-by-user")]
        public async Task<IActionResult> ListUserTenant(string userName="")
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var id = GetCurrentUser() ?? Guid.Empty;
                var result = _userService.ListUserTenant(id, false);
                return Ok(Result<List<UserTenantDto>>.Success(result));
            }  
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<List<string>>.Failure(ex.Message));
            }
  
        }

        [HttpGet("get-by-id")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
				var result = _userService.GetUser(id);
				return Ok(Result<User>.Success(result));
			}
            catch(Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
			}     
        }

        [HttpPost()]
        public async Task<IActionResult> CreateCustomerTenant(CreateUser request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var currentUserName = GetCurrentUserName();
                if(_userService.GetUserAdminByUserName(currentUserName) == null)
                {
                    return Ok(Result<string>.Failure("Bạn không có quyền tạo tài khoản."));
                }
                request.Code = StringHelper.GenerateCode();
                var result = _userService.CreateUser(request);
                if (result.Succeeded && result.Data != null )
                {
                    foreach (var item in result.Data)
                    {
                        string sConnect = "";
                        if (item.Tenant != null)
                            sConnect = item.Tenant.ConnectionString ?? "";
                        else
                        {
                            var tenant = _tenantService.GetTenantInfoByTenantId(item.TenantId);
                            sConnect = tenant?.ConnectionString ?? "";
                        }
                        _currentTenantService.TenantId=item.TenantId;
                        _currentTenantService.ConnectionString = sConnect;
                        AddOrUpdateApplicationUserRequest requestUser = new AddOrUpdateApplicationUserRequest();
                        requestUser.UserName = request.UserName;
                        requestUser.Password = request.Password;
                        requestUser.FirstName = request.FirstName;
                        requestUser.LastName = request.LastName;
                        requestUser.Address = request.Address;
                        requestUser.Email = request.Email;
						requestUser.Phone = request.Phone;
                        requestUser.Code = request.Code;
                        requestUser.Id = item.ApplicationUserId;

                        var addOrUpdateApplicationUserRequests = new List<AddOrUpdateApplicationUserRequest>();
                        addOrUpdateApplicationUserRequests.Add(requestUser);
                        var users = await _applicationUserService.AddUserIntoTenant(addOrUpdateApplicationUserRequests, sConnect);
                        //Log
                        if (users.Succeeded && users.Data != null)
                        {
                            //_userService.UpdateUserTenant(request.UserName, item.TenantId, users.Data[0].Id.Value);
                            var listRole = request.TenantRoles.Where(x => x.TenantId == item.TenantId).Select(x => x.ApplicationRoleNames).FirstOrDefault();
                            if (listRole != null && listRole.Any())
                            {
                                await _applicationUserService.SaveApplicationRolesAsync(new ApplicationUserRequest()
                                {
                                    Id = users.Data[0].Id.ToString(),
                                    ConnectString = sConnect,
                                    UserName = request.UserName,
                                    LastModifiedApplicationUserId = Guid.Empty,
                                    ApplicationRoleNames = listRole,
                                }); ;
                            }
                            else
                                await _applicationUserService.SaveApplicationRolesAdminAsync(new ApplicationUserAdminRequest()
                                {
                                    ConnectString = sConnect,
                                    UserName = request.UserName,
                                    LastModifiedApplicationUserId = Guid.Empty
                                });
                        }
                    }
                }              
                return Ok(result);
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }

        }

        [HttpPost("mobile/register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                var newUser = await _userService.RegisterUser(new UserMainTenantDto()
                {
                   Email = request.Email,
                   FullName = request.FullName,
                   Password = request.Password,
                   Code = request.Code,
                   Address = request.Address,
                   Phone = request.Phone,
                   DateOfBirth = request.DateOfBirth
                }, null, request.GroupTenantId, userId, request.IsHasOtp ?? false, request.OtpCode ?? "", request.DeviceID ?? "");

                return Ok(newUser);
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpDelete("mobile/delete")]
        [Authorize]
        public async Task<IActionResult> MobileDelete()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;
                return Ok(await _userService.MobileDelete(userId));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }
    }
}
