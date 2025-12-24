using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using OfficeOpenXml;
using Sale_Saas.Application.Common.Interfaces;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.EmployeeSalaryFeature.Commands;
using Sale_Saas.Application.Features.EmployeeSalaryFeature.Dto;
using Sale_Saas.Application.Features.EmployeeSalaryFeature.Queries;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Utilities;
using Sale_Saas.Domain.Entities;
using Sale_Saas.Domain.Enums;
using Sale_Saas.Infrastructure.Services.TenantService;
using System.Reflection;

namespace Sale_Saas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeSalaryController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IApplicationUserService _applicationUserService;
        private readonly IApplicationRoleService _roleService;
        private readonly IApplicationDbContext _context;
        private readonly IInternalService _internalService;
        private readonly ITenantService _tenantService;
        private readonly ILoggerService _loggerService;

        public EmployeeSalaryController(IUserService customerTenantService, IApplicationDbContext context,
                                            IApplicationUserService applicationUserService, IApplicationRoleService roleService,
                                            IInternalService internalService, ITenantService tenantService, ILoggerService loggerService)
        {
            _userService = customerTenantService;
            _applicationUserService = applicationUserService;
            _roleService = roleService;
            _context = context;
            _internalService = internalService;
            _tenantService = tenantService;
            _loggerService = loggerService;
        }
        [HttpPost("filter")]
        public async Task<IActionResult> Filter([FromQuery] FilterQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new EmployeeSalary_GetListQuery(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll([FromQuery] GetAllQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new EmployeeSalary_GetAllQuery(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpPost("get-list-with-pagination")]
        public async Task<IActionResult> GetListWithPagination([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                request.UserId = GetCurrentUser();

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new EmployeeSalary_GetListWithPaginationQuery(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-list-profile-all-by-user")]
        public async Task<IActionResult> GetListProfileAllByUser()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                //request.UserId = GetCurrentUser();
                var userName = GetCurrentUserName();
                var listTenant = _userService.ListUserTenant(GetCurrentUser() ?? Guid.Empty);
                var listResult = new List<EmployeeInfo>();
                if (listTenant != null)
                {
                    EmployeeInfo userInfo = null;
                    foreach (var item in listTenant)
                    {
                        _applicationUserService.SetConnectDB(item.ConnectString ?? "");
                        if (listResult.FindIndex(x => x.UserName == userName) == -1)
                        {
                            userInfo = new EmployeeInfo();
                            userInfo.UserName = userName;
                            userInfo.TenantNameRoleInfos = new List<TenantNameRoleInfo>();
                            userInfo.ApplicationUser = await _applicationUserService.GetUserBasicById(item.ApplicationUserId.Value);
                            listResult.Add(userInfo);
                        }
                        var userRoles = (await _roleService.GetListRoleByUserId(item.ApplicationUserId.Value))
                            .Select(s => new RoleBasic()
                            {
                                DisplayName = s.DisplayName,
                                Id = s.Id,
                                Name = s.Name,
                                Description = s.Description
                            }).ToList();
                        userInfo.TenantNameRoleInfos.Add(new TenantNameRoleInfo()
                        {
                            TenantName = item.TenantName,
                            RoleInfos = userRoles
                        });
                    }
                }
                return Ok(Result<List<EmployeeInfo>>.Success(listResult));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("get-list-profile-all-admin")]
        public async Task<IActionResult> GetListProfileAllAdmin([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var id = GetCurrentUser() ?? Guid.Empty;

                var listUserTenant = _userService.ListAllUserTenant(id, request.PageIndex, request.PageSize);
                var listResult = new List<EmployeeInfo>();
                if (listUserTenant != null)
                {
                    EmployeeInfo userInfo = new EmployeeInfo();
                    foreach (var item in listUserTenant)
                    {
                        _applicationUserService.SetConnectDB(item.ConnectString ?? "");
                        if (listResult.FindIndex(x => x.UserName == item.UserName) == -1)
                        {
                            userInfo = new EmployeeInfo();
                            userInfo.UserName = item.UserName;
                            userInfo.TenantNameRoleInfos = new List<TenantNameRoleInfo>();
                            userInfo.ApplicationUser = await _applicationUserService.GetUserBasicById(item.ApplicationUserId.Value);
                            listResult.Add(userInfo);
                        }
                        var userRoles = (await _roleService.GetListRoleByUserId(item.ApplicationUserId.Value))
                            .Select(s => new RoleBasic()
                            {
                                DisplayName = s.DisplayName,
                                Id = s.Id,
                                Name = s.Name,
                                Description = s.Description,
                            }).ToList();

                        var tmp = userInfo.TenantNameRoleInfos.Where(s => s.TenantName == item.TenantName).FirstOrDefault();
                        if (tmp == null)
                        {
                            userInfo.TenantNameRoleInfos.Add(new TenantNameRoleInfo()
                            {
                                TenantName = item.TenantName,
                                RoleInfos = userRoles
                            });
                        }
                        else
                        {
                            var existingRoleIds = tmp.RoleInfos.Select(s => s.Id).ToList();
                            var newRoles = userRoles.Where(role => !existingRoleIds.Contains(role.Id)).ToList();
                            tmp.RoleInfos.AddRange(newRoles);
                        }

                    }
                    listResult.Add(userInfo);
                }
                var result = new PaginatedList<EmployeeInfo>(listResult, listResult.Count, request.PageIndex, request.PageSize);
                return Ok(Result<PaginatedList<EmployeeInfo>>.Success(result));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("mobile/get-list-employee")]
        public async Task<IActionResult> MobileGetListProfileAllAdmin([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var listUserTenant = _userService.ListAllUserTenant(GetCurrentUser() ?? Guid.Empty);
                var listResult = new List<EmployeeInfo>();
                if (listUserTenant != null)
                {
                    EmployeeInfo userInfo = null;
                    foreach (var item in listUserTenant)
                    {
                        _applicationUserService.SetConnectDB(item.ConnectString);
                        if (listResult.FindIndex(x => x.UserName == item.UserName) == -1)
                        {
                            userInfo = new EmployeeInfo();
                            userInfo.UserName = item.UserName;
                            userInfo.TenantNameRoleInfos = new List<TenantNameRoleInfo>();
                            userInfo.ApplicationUser = await _applicationUserService.GetUserBasicById(item.ApplicationUserId.Value);
                            listResult.Add(userInfo);
                        }
                        var userRoles = (await _roleService.GetListRoleByUserId(item.ApplicationUserId.Value))
                            .Where(s => s.Name != "Admin")
                            .Select(s => new RoleBasic()
                            {
                                DisplayName = s.DisplayName,
                                Id = s.Id,
                                Name = s.Name,
                                Description = s.Description,
                            }).ToList();
                        if (userRoles.Count == 0)
                        {
                            continue;
                        }
                        userInfo.TenantNameRoleInfos.Add(new TenantNameRoleInfo()
                        {
                            TenantName = item.TenantName,
                            RoleInfos = userRoles
                        });
                    }

                }

                if (request.TextSearch != null)
                {
                    listResult = listResult.Where(s => s.ApplicationUser.Code.ToUpper().Contains(request.TextSearch.ToUpper()) ||
                                                       s.ApplicationUser.FullName.ToUpper().Contains(request.TextSearch.ToUpper()) ||
                                                       s.ApplicationUser.Email.ToUpper().Contains(request.TextSearch.ToUpper())).ToList();
                }

                listResult = listResult.Where(s => s.TenantNameRoleInfos.Count != 0).ToList();

                var result = new PaginatedList<EmployeeInfo>(listResult, listResult.Count, request.PageIndex, request.PageSize);
                return Ok(Result<PaginatedList<EmployeeInfo>>.Success(result));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }


        [HttpGet("get-column-table-role-salary-all")]
        public async Task<IActionResult> GetColumnTableRoleSalaryAll() // Thu Nhập theo vị trí tất cả
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var listUserTenant = _userService.ListAllUserTenant(GetCurrentUser() ?? Guid.Empty, 1, 100);
                var listResult = new List<TenantNameRoles>();
                if (listUserTenant != null)
                {
                    foreach (var item in listUserTenant)
                    {
                        _applicationUserService.SetConnectDB(item.ConnectString);
                        var listId = listResult.Select(x => x.Id).ToList();
                        var userRoles = (await _roleService.GetListRoleByUserId(item.ApplicationUserId.Value))
                            .Where(s => s.RolePositionId != RolePositionEnum.ADMIN.ToString())
                            .Select(s => new TenantNameRoles()
                            {
                                DisplayName = s.DisplayName,
                                Id = s.Id,
                                Name = s.Name,
                                Description = s.Description,
                                TenantName = item.TenantName
                            }).Where(x => !listId.Contains(x.Id)).ToList();
                        listResult.AddRange(userRoles);
                    }
                    listResult = listResult.OrderBy(x => x.TenantName).ThenBy(x => x.DisplayName).ToList();
                }
                var result = new PaginatedList<TenantNameRoles>(listResult, listResult.Count, 1, 100);
                return Ok(Result<PaginatedList<TenantNameRoles>>.Success(result));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-column-table-role-salary-by-user")] // Thu Nhập theo vị trí của tôi
        public async Task<IActionResult> GetColumnTableRoleSalaryByUser()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var listUserTenant = _userService.ListUserTenant(GetCurrentUser() ?? Guid.Empty, true);
                var listResult = new List<TenantNameRoles>();
                if (listUserTenant != null)
                {
                    foreach (var item in listUserTenant)
                    {
                        _applicationUserService.SetConnectDB(item.ConnectString ?? "");
                        var listId = listResult.Select(x => x.Id).ToList();
                        var userRoles = (await _roleService.GetListRoleByUserId(item.ApplicationUserId.Value))
                            .Select(s => new TenantNameRoles()
                            {
                                DisplayName = s.DisplayName,
                                Id = s.Id,
                                Name = s.Name,
                                Description = s.Description,
                                TenantName = item.TenantName
                            }).Where(x => !listId.Contains(x.Id)).ToList();
                        listResult.AddRange(userRoles);
                    }
                    listResult = listResult.OrderBy(x => x.TenantName).ThenBy(x => x.DisplayName).ToList();
                }
                var result = new PaginatedList<TenantNameRoles>(listResult, listResult.Count, 1, 100);
                return Ok(Result<PaginatedList<TenantNameRoles>>.Success(result));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("get-list-profile-all")]
        public async Task<IActionResult> GetListProfileAll([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var listUserTenant = _userService.ListAllUserTenant(GetCurrentUser() ?? Guid.Empty);
                var listResult = new List<EmployeeInfo>();
                if (listUserTenant != null)
                {
                    EmployeeInfo userInfo = null;
                    foreach (var item in listUserTenant)
                    {
                        _applicationUserService.SetConnectDB(item.ConnectString);
                        if (listResult.FindIndex(x => x.UserName == item.UserName) == -1)
                        {
                            userInfo = new EmployeeInfo();
                            userInfo.UserName = item.UserName;
                            userInfo.TenantNameRoleInfos = new List<TenantNameRoleInfo>();
                            userInfo.ApplicationUser = await _applicationUserService.GetUserBasicById(item.ApplicationUserId.Value);
                            listResult.Add(userInfo);
                        }
                        var userRoles = (await _roleService.GetListRoleByUserId(item.ApplicationUserId.Value))
                            .Where(s => s.Name != "Admin")
                            .Select(s => new RoleBasic()
                            {
                                DisplayName = s.DisplayName,
                                Id = s.Id,
                                Name = s.Name,
                                Description = s.Description,
                            }).ToList();
                        if (userRoles.Count == 0)
                        {
                            continue;
                        }
                        userInfo.TenantNameRoleInfos.Add(new TenantNameRoleInfo()
                        {
                            TenantName = item.TenantName,
                            RoleInfos = userRoles
                        });
                    }

                }

                if (request.TextSearch != null)
                {
                    listResult = listResult.Where(s => s.ApplicationUser.Code.ToUpper().Contains(request.TextSearch.ToUpper()) ||
                                                       s.ApplicationUser.FullName.ToUpper().Contains(request.TextSearch.ToUpper()) ||
                                                       s.ApplicationUser.Email.ToUpper().Contains(request.TextSearch.ToUpper())).ToList();
                }

                listResult = listResult.Where(s => s.TenantNameRoleInfos.Count != 0).ToList();

                var result = new PaginatedList<EmployeeInfo>(listResult, listResult.Count, request.PageIndex, request.PageSize);
                return Ok(Result<PaginatedList<EmployeeInfo>>.Success(result));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-list-all-by-user-with-pagination")]
        public async Task<IActionResult> GetListAllByUserWithPagination()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                //request.UserId = GetCurrentUser();
                var userId = GetCurrentUser() ?? Guid.Empty;
                var listTenant = _userService.ListUserTenant(userId);
                var listResult = new List<EmployeeSalaryUserReportDto>();
                if (listTenant != null)
                {
                    var salary = new EmployeeSalaryUserReportDto()
                    {
                        UserName = GetCurrentUserName(),
                        IncomeBeforeTax = 0,
                        IncomeNonTax = 0,
                        Dependent = 0,
                        Insurance = 0,
                        IncomeTax = 0,
                        PersonalIncomeTax = 0,
                        IncomeRecevied = 0
                    };
                    foreach (var item in listTenant)
                    {
                        var newReq = new EmployeeSalarySumFilter()
                        {
                            ConnectString = item.ConnectString,
                            UserId = item.ApplicationUserId,
                            Year = DateTime.Now.Year
                        };

                        var items = await Mediator.Send(new EmployeeSalary_GetSumByUserIdQuery(userId, newReq));
                        if (items.Data != null && items.Data.Count > 0)
                        {
                            salary.IncomeBeforeTax += items.Data[0].IncomeBeforeTax;
                            salary.IncomeNonTax += items.Data[0].IncomeNonTax;
                            salary.Dependent += items.Data[0].Dependent;
                            salary.Insurance += items.Data[0].Insurance;
                            salary.PersonalIncomeTax += items.Data[0].IncomeTax;
                            salary.PersonalIncomeTax += items.Data[0].PersonalIncomeTax;
                            salary.IncomeRecevied += items.Data[0].IncomeRecevied;
                        }
                    }
                    listResult.Add(salary);
                }
                return Ok(Result<List<EmployeeSalaryUserReportDto>>.Success(listResult));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }


        [HttpPost("get-list-all-with-pagination")]
        public async Task<IActionResult> GetListAllWithPagination([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                //request.UserId = GetCurrentUser();
                var listUserTenant = _userService.ListAllUserTenant(GetCurrentUser() ?? Guid.Empty);
                var listResult = new List<EmployeeSalaryUserReportDto>();
                if (listUserTenant != null)
                {
                    var userId = GetCurrentUser() ?? Guid.Empty;
                    EmployeeSalaryUserReportDto salary = null;
                    foreach (var item in listUserTenant)
                    {
                        _applicationUserService.SetConnectDB(item.ConnectString ?? "");
                        if (listResult.FindIndex(x => x.UserName == item.UserName) == -1)
                        {
                            var userRoles = (await _roleService.GetListRoleByUserId(item.ApplicationUserId.Value))
                                                               .Where(x => x.Name != "Admin")
                                                               .Select(s => s.DisplayName)
                                                               .ToList();
                            if (userRoles.Count == 0)
                            {
                                continue;
                            }
                            var newItem = new EmployeeSalaryUserReportDto()
                            {
                                UserName = item.UserName,
                                IncomeBeforeTax = 0,
                                IncomeNonTax = 0,
                                Dependent = 0,
                                Insurance = 0,
                                IncomeTax = 0,
                                PersonalIncomeTax = 0,
                                IncomeRecevied = 0,
                                TenantNameRoles = new TenantNameRole()
                                {
                                    TenantName = item.TenantName,
                                    RoleNames = userRoles
                                },
                                ApplicationUser = await _applicationUserService.GetUserBasicById(item.ApplicationUserId.Value),
                            };
                            listResult.Add(newItem);
                        }
                        var newReq = new EmployeeSalarySumFilter()
                        {
                            ConnectString = item.ConnectString,
                            UserId = item.ApplicationUserId,
                            Year = request.Time.HasValue ? request.Time.Value.Year : DateTime.Now.Year
                        };

                        var items = await Mediator.Send(new EmployeeSalary_GetSumByUserIdQuery(userId, newReq));
                        if (items.Data != null && items.Data.Count > 0)
                        {
                            salary = listResult.Where(x => x.UserName == item.UserName).FirstOrDefault();
                            if (salary != null)
                            {
                                salary.IncomeBeforeTax = items.Data[0].IncomeBeforeTax;
                                salary.IncomeNonTax = items.Data[0].IncomeNonTax;
                                salary.Dependent = items.Data[0].Dependent;
                                salary.Insurance = items.Data[0].Insurance;
                                salary.IncomeTax = items.Data[0].IncomeTax;
                                salary.PersonalIncomeTax = items.Data[0].PersonalIncomeTax;
                                salary.IncomeRecevied = items.Data[0].IncomeRecevied;
                            }
                        }
                    }

                }

                EmployeeSalaryUserReportDto temp = listResult.FirstOrDefault(item => item.ApplicationUser.Id == GetCurrentUser());

                if (temp != null)
                {
                    listResult.Remove(temp);
                    listResult.Insert(0, temp);
                }

                var result = new PaginatedList<EmployeeSalaryUserReportDto>(listResult, listResult.Count, request.PageIndex, request.PageSize);

                return Ok(Result<PaginatedList<EmployeeSalaryUserReportDto>>.Success(result));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("get-list-all-with-pagination-v2")]
        public async Task<IActionResult> GetListAllWithPaginationV2([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;
                var currentTenant = GetCurrentTenant();


                request.UserId = userId;

                return Ok(await Mediator.Send(new EmployeeSalary_GetListIncomeInformationWithPaginationQuery(request, currentTenant)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("get-list-all-by-user-with-pagination-v2")]
        public async Task<IActionResult> GetListAllByUserWithPaginationV2([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;
                var currentTenant = GetCurrentTenant();


                request.UserId = userId;

                return Ok(await Mediator.Send(new EmployeeSalary_GetIncomeInformationByUserQuery(request, currentTenant)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("get-employee-salary-detail-by-user-v2")]
        public async Task<IActionResult> GetEmployeeSalaryDetailByUserV2([FromBody] GetEmployeeSalaryDetailOfUser request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new EmployeeSalary_GetEmployeeSalaryDetailByUserQuery(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-list-role-all-by-user")]
        public async Task<IActionResult> GetListRoleAllByUser()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                //request.UserId = GetCurrentUser();
                var userName = GetCurrentUserName();
                var listTenant = _userService.ListUserTenant(GetCurrentUser() ?? Guid.Empty);
                var listResult = new List<EmployeeSalaryAllRoleReportDto>();
                if (listTenant != null)
                {
                    var userId = GetCurrentUser() ?? Guid.Empty;
                    EmployeeSalaryAllRoleReportDto userInfo = null;
                    foreach (var item in listTenant)
                    {
                        _applicationUserService.SetConnectDB(item.ConnectString ?? "");
                        GetListWithPaginationQueryRequest request = new GetListWithPaginationQueryRequest()
                        {
                            PageIndex = 1,
                            PageSize = 100,
                            UserId = item.ApplicationUserId
                        };
                        var listData = await Mediator.Send(new EmployeeSalaryByRole_GetListWithPaginationQuery(userId, request));
                        foreach (var m in listData.Data.Items)
                        {
                            if (listResult.FindIndex(x => x.Month == m.Month) == -1)
                            {
                                userInfo = new EmployeeSalaryAllRoleReportDto();
                                userInfo.Month = m.Month;
                                userInfo.Year = m.Year;
                                userInfo.IncomeOther = m.IncomeOther;
                                userInfo.TotalIncome = m.TotalIncome;
                                userInfo.TenantNameIncomRoles = new List<TenantNameIncomRole>()
                                {
                                    new TenantNameIncomRole
                                    {
                                        TenantName = item.TenantName,
                                        IncomeRoles = m.IncomeRoles
                                    }
                                };
                                listResult.Add(userInfo);
                            }
                            else
                            {
                                userInfo.IncomeOther += m.IncomeOther;
                                userInfo.TotalIncome += m.TotalIncome;
                                userInfo.TenantNameIncomRoles.Add(new TenantNameIncomRole()
                                {
                                    TenantName = item.TenantName,
                                    IncomeRoles = m.IncomeRoles
                                });
                            }
                        }
                    }
                    if (listResult.Any())
                    {
                        listResult = listResult.OrderBy(s => s.Year).ThenBy(s => s.Month).ToList();
                    }
                }
                return Ok(Result<List<EmployeeSalaryAllRoleReportDto>>.Success(listResult));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("get-list-role-with-pagination")]
        public async Task<IActionResult> GetListRoleWithPagination([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                request.UserId = GetCurrentUser();
                var userId = GetCurrentUser() ?? Guid.Empty;
                return Ok(await Mediator.Send(new EmployeeSalaryByRole_GetListWithPaginationQuery(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("get-list-admin-role-all-with-pagination")]
        public async Task<IActionResult> GetListAdminRoleAllWithPagination([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                //request.UserId = GetCurrentUser();
                var listUserTenant = _userService.ListAllUserTenant(GetCurrentUser() ?? Guid.Empty, request.PageIndex, request.PageSize);
                var listResult = new List<EmployeeSalaryAllAdminRoleReportDto>();
                if (listUserTenant != null)
                {
                    var userId = GetCurrentUser() ?? Guid.Empty;
                    EmployeeSalaryAllAdminRoleReportDto userInfo = null;
                    foreach (var item in listUserTenant)
                    {
                        _applicationUserService.SetConnectDB(item.ConnectString ?? "");
                        GetListWithPaginationQueryRequest requestNew = new GetListWithPaginationQueryRequest()
                        {
                            PageIndex = 1,
                            PageSize = 100,
                            UserId = item.ApplicationUserId,
                            Time = request.Time
                        };
                        var listData = await Mediator.Send(new EmployeeSalaryAdminByRole_GetListWithPaginationQuery(userId, requestNew));
                        foreach (var m in listData.Data.Items)
                        {
                            if (listResult.FindIndex(x => x.UserName == item.UserName) == -1)
                            {
                                userInfo = new EmployeeSalaryAllAdminRoleReportDto();
                                userInfo.ApplicationUser = m.ApplicationUser; // await _applicationUserService.GetUserBasicById(item.ApplicationUserId.Value);
                                userInfo.UserName = item.UserName;
                                userInfo.IncomeOther = m.IncomeOther;
                                userInfo.TotalIncome = m.TotalIncome;
                                userInfo.TenantNameIncomRoles.Add(new TenantNameIncomRole()
                                {
                                    TenantName = item.TenantName,
                                    IncomeRoles = m.IncomeRoles
                                });
                                listResult.Add(userInfo);
                            }
                            else
                            {
                                userInfo.IncomeOther += m.IncomeOther;
                                userInfo.TotalIncome += m.TotalIncome;
                                userInfo.TenantNameIncomRoles.Add(new TenantNameIncomRole()
                                {
                                    TenantName = item.TenantName,
                                    IncomeRoles = m.IncomeRoles
                                });
                            }
                        }
                    }

                }
                var result = new PaginatedList<EmployeeSalaryAllAdminRoleReportDto>(listResult, listResult.Count, request.PageIndex, request.PageSize);
                return Ok(Result<PaginatedList<EmployeeSalaryAllAdminRoleReportDto>>.Success(result));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("get-list-admin-role-all-with-pagination-v2")]
        public async Task<IActionResult> GetListAdminRoleAllWithPaginationV2([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;
                var currentTenant = GetCurrentTenant();


                request.UserId = userId;

                return Ok(await Mediator.Send(new EmployeeSalary_GetListIncomeDetailByRoleWithPaginationQuery(request, currentTenant)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("get-list-role-all-by-user-with-pagination-v2")]
        public async Task<IActionResult> GetListRoleAllByUserWithPaginationV2([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;
                var currentTenant = GetCurrentTenant();


                request.UserId = userId;

                return Ok(await Mediator.Send(new EmployeeSalary_GetListIncomeDetailByRoleByUserQuery(request, currentTenant)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }


        [HttpPost("get-list-admin-role-with-pagination")]
        public async Task<IActionResult> GetListAdminRoleWithPagination([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                //request.UserId = GetCurrentUser();
                var userId = GetCurrentUser() ?? Guid.Empty;
                return Ok(await Mediator.Send(new EmployeeSalaryAdminByRole_GetListWithPaginationQuery(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpGet("get-by-id/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new EmployeeSalary_GetByIdQuery(userId, id)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("add-or-update")]
        public async Task<IActionResult> AddOrUpdate([FromBody] List<AddOrUpdateRequest> request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new EmployeeSalary_AddOrUpdateCommand(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpDelete("delete-by-ids/{ids}/{ApplicationUserId}")]
        public async Task<IActionResult> DeleteByIds(string ids, Guid ApplicationUserId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var request = new DeleteRequest()
                {
                    Ids = ids.Split(",").ToList(),
                    ApplicationUserId = ApplicationUserId
                };

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new EmployeeSalary_DeleteByIdCommand(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("import")]
        public async Task<IActionResult> Import(IFormFile file)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                //List<EmployeeSalary> listEmpSalary = new List<EmployeeSalary>();
                List<EmployeeTenant> listEmpTenant = new List<EmployeeTenant>();
                List<string> conStr = new List<string>();
                var loginUserId = GetCurrentUser();
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        var rowcount = worksheet.Dimension.Rows;
                        var columnCount = worksheet.Dimension.Columns;
                        for (int row = 2; row <= rowcount; row++)
                        {
                            var empSalary = new EmployeeSalary();
                            empSalary.Id = Guid.NewGuid();
                            empSalary.DeleteFlag = false;
                            empSalary.CreatedDate = DateTime.Now;
                            empSalary.LastModifiedDate = DateTime.Now;
                            empSalary.CreatedApplicationUserId = loginUserId;
                            empSalary.LastModifiedApplicationUserId = loginUserId;

                            for (int column = 1; column <= columnCount; column++)
                            {
                                if (worksheet.Cells[row, column].Value != null)
                                {
                                    switch (worksheet.Cells[1, column].Value.ToString())
                                    {
                                        case "Email nhân viên":
                                            var userEmail = worksheet.Cells[row, column].Value.ToString() ?? "";
                                            var userEmailNormalize = userEmail.Trim();
                                            var user = await _applicationUserService.GetUserByEmail(userEmailNormalize);
                                            empSalary.UserId = user.Data != null ? user.Data.Id : Guid.Empty;
                                            empSalary.EmployeeCode = user.Data != null ? user.Data.Code : string.Empty;
                                            break;
                                        case "Tháng":
                                            empSalary.Month = int.Parse(worksheet.Cells[row, column].Value.ToString() ?? "0");
                                            break;
                                        case "Năm":
                                            empSalary.Year = int.Parse(worksheet.Cells[row, column].Value.ToString() ?? "0");
                                            break;
                                        case "Trước thuế":
                                            empSalary.IncomeBeforeTax = decimal.Parse(worksheet.Cells[row, column].Value.ToString() ?? "0");
                                            break;
                                        case "Không thuế":
                                            empSalary.IncomeNonTax = decimal.Parse(worksheet.Cells[row, column].Value.ToString() ?? "0");
                                            break;
                                        case "Giảm trừ":
                                            empSalary.Dependent = decimal.Parse(worksheet.Cells[row, column].Value.ToString() ?? "0");
                                            break;
                                        case "BHXH":
                                            empSalary.Insurance = decimal.Parse(worksheet.Cells[row, column].Value.ToString() ?? "0");
                                            break;
                                        case "Thu nhập chịu thuế":
                                            empSalary.IncomeTax = decimal.Parse(worksheet.Cells[row, column].Value.ToString() ?? "0");
                                            break;
                                        case "Thuế TNCN":
                                            empSalary.PersonalIncomeTax = decimal.Parse(worksheet.Cells[row, column].Value.ToString() ?? "0");
                                            break;
                                        case "Thu nhập khác":
                                            empSalary.IncomeOther = decimal.Parse(worksheet.Cells[row, column].Value.ToString() ?? "0");
                                            break;
                                        case "Thực nhận":
                                            empSalary.IncomeRecevied = decimal.Parse(worksheet.Cells[row, column].Value.ToString() ?? "0");
                                            break;
                                        case "Mã vai trò":
                                            var roleName = worksheet.Cells[row, column].Value.ToString();
                                            var roleNameNormalize = roleName != null ? roleName.Trim() : null;
                                            var role = await _roleService.GetByRoleName(worksheet.Cells[row, column].Value.ToString() ?? "");
                                            if (role.Data == null)
                                                return Ok(Result<string>.Failure($"Không tìm thấy mã vai trò {worksheet.Cells[row, column].Value.ToString()} nên không thể import file này. Vui lòng liên hệ với quản trị viên"));
                                            empSalary.Role = role.Data;
                                            break;
                                        case "Mã tổ chức":
                                            var listConnStr = await _tenantService.GetListTenantByListIds(worksheet.Cells[row, column].Value.ToString() ?? "");
                                            if (listConnStr.Count <= 0)
                                                return Ok(Result<string>.Failure($"Mã tổ chức {worksheet.Cells[row, column].Value.ToString()} không được tìm thấy nên không thể import file này. Vui lòng liên hệ với quản trị viên"));
                                            conStr = listConnStr;
                                            break;
                                    }
                                }
                            }
                            foreach (var con in conStr)
                            {
                                EmployeeTenant employeeTenant = new EmployeeTenant();
                                employeeTenant.ConnecttionStr = con.ToString();
                                EmployeeSalary employeeSalary = new EmployeeSalary
                                {
                                    Id = Guid.NewGuid(),
                                    DeleteFlag = false,
                                    CreatedDate = DateTime.Now,
                                    LastModifiedDate = DateTime.Now,
                                    CreatedApplicationUserId = loginUserId,
                                    LastModifiedApplicationUserId = loginUserId,
                                    UserId = empSalary.UserId,
                                    User = empSalary.User,
                                    EmployeeCode = empSalary.EmployeeCode,
                                    Month = empSalary.Month,
                                    Year = empSalary.Year,
                                    IncomeBeforeTax = empSalary.IncomeBeforeTax,
                                    IncomeNonTax = empSalary.IncomeNonTax,
                                    Dependent = empSalary.Dependent,
                                    Insurance = empSalary.Insurance,
                                    IncomeTax = empSalary.IncomeTax,
                                    PersonalIncomeTax = empSalary.PersonalIncomeTax,
                                    IncomeOther = empSalary.IncomeOther ?? 0,
                                    IncomeRecevied = empSalary.IncomeRecevied,
                                    //RoleId = empSalary.RoleId,
                                    Note = string.Empty,
                                    Role = empSalary.Role
                                };
                                employeeTenant.EmployeeSalary = employeeSalary;
                                listEmpTenant.Add(employeeTenant);
                            }
                        }
                    }
                }
                if (listEmpTenant.Count > 0)
                {
                    var userId = GetCurrentUser() ?? Guid.Empty;
                    return Ok(await Mediator.Send(new EmployeeSalary_ImportCommand(userId, listEmpTenant)));
                }
                return Ok(Result<string>.Failure("File import khác file mẫu!"));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("import-excel")]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                Guid userId = GetCurrentUser() ?? Guid.Empty;

                if (file != null && file.Length != 0)
                {
                    return Ok(await Mediator.Send(new EmployeeSalary_ImportExcelCommand(userId, file)));
                }

                return Ok(Result<string>.Failure("File import khác file mẫu!"));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("mobile/import-excel")]
        public async Task<IActionResult> MobileImportExcel(IFormFile file)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                Guid userId = GetCurrentUser() ?? Guid.Empty;

                if (file != null && file.Length != 0)
                {
                    return Ok(await Mediator.Send(new EmployeeSalaryMobile_ImportExcelCommand(userId, file)));
                }

                return Ok(Result<string>.Failure("File import khác file mẫu!"));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }


        [HttpGet("export-income-to-date")]
        public async Task<IActionResult> ExportYear([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                string locale = GetLocale();
                request.UserId = GetCurrentUser();

                var listUserTenant = _userService.ListAllUserTenant(GetCurrentUser() ?? Guid.Empty);
                var listData = new List<EmployeeSalaryUserReportDto>();
                if (listUserTenant != null)
                {
                    EmployeeSalaryUserReportDto salary = null;
                    foreach (var item in listUserTenant)
                    {
                        _applicationUserService.SetConnectDB(item.ConnectString ?? "");
                        if (listData.FindIndex(x => x.UserName == item.UserName) == -1)
                        {
                            var userRoles = (await _roleService.GetListRoleByUserId(item.ApplicationUserId.Value)).Select(s => s.DisplayName).ToList();
                            var newItem = new EmployeeSalaryUserReportDto()
                            {
                                UserName = item.UserName,
                                IncomeBeforeTax = 0,
                                IncomeNonTax = 0,
                                Dependent = 0,
                                Insurance = 0,
                                IncomeTax = 0,
                                PersonalIncomeTax = 0,
                                IncomeRecevied = 0,
                                TenantNameRoles = new TenantNameRole()
                                {
                                    TenantName = item.TenantName,
                                    RoleNames = userRoles
                                },
                                ApplicationUser = await _applicationUserService.GetUserBasicById(item.ApplicationUserId.Value),
                            };
                            listData.Add(newItem);
                        }
                        var newReq = new EmployeeSalarySumFilter()
                        {
                            ConnectString = item.ConnectString,
                            UserId = item.ApplicationUserId,
                            Year = request.Time.HasValue ? request.Time.Value.Year : DateTime.Now.Year
                        };
                        var userId = GetCurrentUser() ?? Guid.Empty;
                        var items = await Mediator.Send(new EmployeeSalary_GetSumByUserIdQuery(userId, newReq));
                        if (items.Data != null && items.Data.Count > 0)
                        {
                            salary = listData.Where(x => x.UserName == item.UserName).FirstOrDefault();
                            if (salary != null)
                            {
                                salary.IncomeBeforeTax += items.Data[0].IncomeBeforeTax;
                                salary.IncomeNonTax += items.Data[0].IncomeNonTax;
                                salary.Dependent += items.Data[0].Dependent;
                                salary.Insurance += items.Data[0].Insurance;
                                salary.IncomeTax += items.Data[0].IncomeTax;
                                //salary.PersonalIncomeTax += items.Data[0].IncomeTax;
                                salary.PersonalIncomeTax += items.Data[0].PersonalIncomeTax;
                                salary.IncomeRecevied += items.Data[0].IncomeRecevied;
                            }
                        }
                    }

                }

                if (listData == null)
                    throw new ApplicationException("Xuất file excel thất bại!");

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage excel = new ExcelPackage();
                string listName = locale == LocaleEnum.vi_VN.ToString() ? "Danh sách thu nhập trong năm" : "List income during the year";
                var workSheet = excel.Workbook.Worksheets.Add(listName);
                workSheet = ExcelExportHelper.GetStyle(workSheet, 11);

                workSheet.Cells[1, 1].Value = "ID";
                workSheet.Cells[1, 2].Value = "MÃ NHÂN VIÊN";
                workSheet.Cells[1, 3].Value = "HỌ VÀ TÊN";
                workSheet.Cells[1, 4].Value = "VỊ TRÍ";
                workSheet.Cells[1, 5].Value = "TỔNG THU NHẬP TRƯỚC THUẾ";
                workSheet.Cells[1, 6].Value = "TỔNG THU NHẬP KHÔNG CHỊU THUẾ";
                workSheet.Cells[1, 7].Value = "TỔNG GIẢM TRỪ GIA CẢNH";
                workSheet.Cells[1, 8].Value = "TỔNG TIỀN BHXH NLD ĐÓNG";
                workSheet.Cells[1, 9].Value = "TỔNG THU NHẬP CHỊU THUẾ";
                workSheet.Cells[1, 10].Value = "TỔNG THUẾ TNCN TẠM THU";
                workSheet.Cells[1, 11].Value = "TỔNG THU NHẬP NHẬN ĐƯỢC";

                if (locale == LocaleEnum.en_US.ToString())
                {
                    workSheet.Cells[1, 2].Value = "HUMAN RESOURCE CODE";
                    workSheet.Cells[1, 3].Value = "FULL NAME";
                    workSheet.Cells[1, 4].Value = "POSITION";
                    workSheet.Cells[1, 5].Value = "TOTAL INCOME BEFORE TAXES";
                    workSheet.Cells[1, 6].Value = "TOTAL INCOME IS NOT TAXABLE";
                    workSheet.Cells[1, 7].Value = "TOTAL DEDUCTION DUE TO FAMILY CIRCUMSTANCES";
                    workSheet.Cells[1, 8].Value = "TOTAL SOCIAL INSURANCE AMOUNT";
                    workSheet.Cells[1, 9].Value = "TOTAL TAXABLE INCOME";
                    workSheet.Cells[1, 10].Value = "TOTAL TEMPORARY TAX COLLECTED";
                    workSheet.Cells[1, 11].Value = "TOTAL INCOME RECEIVED";
                }

                int currRow = 2;

                foreach (var item in listData)
                {
                    workSheet.Row(currRow).Height = 20;
                    workSheet.Cells[currRow, 1].Value = item.ApplicationUser.Id;
                    workSheet.Cells[currRow, 2].Value = item.ApplicationUser.Code;
                    workSheet.Cells[currRow, 3].Value = item.ApplicationUser.FullName;
                    workSheet.Cells[currRow, 4].Value = item.TenantNameRoles.RoleNames;
                    workSheet.Cells[currRow, 5].Value = item.IncomeBeforeTax;
                    workSheet.Cells[currRow, 6].Value = item.IncomeNonTax;
                    workSheet.Cells[currRow, 7].Value = item.Dependent;
                    workSheet.Cells[currRow, 8].Value = item.Insurance;
                    workSheet.Cells[currRow, 9].Value = item.IncomeTax;
                    workSheet.Cells[currRow, 10].Value = item.PersonalIncomeTax;
                    workSheet.Cells[currRow, 11].Value = item.IncomeRecevied;
                    currRow++;
                }

                workSheet.Cells.AutoFitColumns();

                return File(excel.GetAsByteArray(), "application/vnd.ms-excel", String.Format("{0}.xlsx", listName));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("import-sample")]
        public async Task<IActionResult> ImportExample()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                string locale = GetLocale();
                string userName = GetCurrentUserName();
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage excel = new ExcelPackage();

                #region Sheet Salary
                var salarySheet = excel.Workbook.Worksheets.Add("Danh sách thu nhập trong năm");
                salarySheet = ExcelExportHelper.GetStyle(salarySheet, 14);

                salarySheet.Cells[1, 1].Value = "Email nhân viên";
                //salarySheet.Cells[1, 2].Value = "Mã nhân viên";
                salarySheet.Cells[1, 2].Value = "Tháng";
                salarySheet.Cells[1, 3].Value = "Năm";
                salarySheet.Cells[1, 4].Value = "Trước thuế";
                salarySheet.Cells[1, 5].Value = "Không thuế";
                salarySheet.Cells[1, 6].Value = "Giảm trừ";
                salarySheet.Cells[1, 7].Value = "BHXH";
                salarySheet.Cells[1, 8].Value = "Thu nhập chịu thuế";
                salarySheet.Cells[1, 9].Value = "Thuế TNCN";
                salarySheet.Cells[1, 10].Value = "Thu nhập khác";
                salarySheet.Cells[1, 11].Value = "Thực nhận";
                salarySheet.Cells[1, 12].Value = "Mã vai trò";
                salarySheet.Cells[1, 13].Value = "Mã tổ chức";

                int currRow = 2;

                for (var index = 1; index <= 5; index++)
                {
                    salarySheet.Row(currRow).Height = 20;
                    salarySheet.Cells[currRow, 1].Value = $"taikhoan{index}@gmail.com";
                    //salarySheet.Cells[currRow, 2].Value = $"{index}40426-ABAWWE";
                    salarySheet.Cells[currRow, 2].Value = $"{index}";
                    salarySheet.Cells[currRow, 3].Value = "2024";
                    salarySheet.Cells[currRow, 4].Value = $"{index}00";
                    salarySheet.Cells[currRow, 5].Value = $"{index}00";
                    salarySheet.Cells[currRow, 6].Value = $"{index}00";
                    salarySheet.Cells[currRow, 7].Value = $"{index}00";
                    salarySheet.Cells[currRow, 8].Value = $"{index}00";
                    salarySheet.Cells[currRow, 9].Value = $"{index}00";
                    salarySheet.Cells[currRow, 10].Value = $"{index}00";
                    salarySheet.Cells[currRow, 11].Value = $"{index}00";
                    salarySheet.Cells[currRow, 12].Value = "Sale";
                    salarySheet.Cells[currRow, 13].Value = $"dev{index}";
                    currRow++;
                }

                salarySheet.Cells.AutoFitColumns();
                #endregion

                #region Sheet role
                var tenants = _userService.ListUserTenant(GetCurrentUser() ?? Guid.Empty);


                var roleSheet = excel.Workbook.Worksheets.Add("Danh sách vị trí");
                roleSheet = ExcelExportHelper.GetStyle(roleSheet, 4);

                roleSheet.Cells[1, 1].Value = "Mã vị trí";
                roleSheet.Cells[1, 2].Value = "Tên vị trí";
                roleSheet.Cells[1, 3].Value = "Ghi chú";
                roleSheet.Cells[1, 4].Value = "Mã tổ chức";

                var tenantSheet = excel.Workbook.Worksheets.Add("Danh sách tổ chức");
                tenantSheet = ExcelExportHelper.GetStyle(tenantSheet, 2);
                tenantSheet.Cells[1, 1].Value = "Mã tổ chức";
                tenantSheet.Cells[1, 2].Value = "Tên tổ chức";

                var tenantSheetRow = 2;
                foreach (var tenant in tenants)
                {
                    tenantSheet.Row(tenantSheetRow).Height = 20;
                    tenantSheet.Cells[tenantSheetRow, 1].Value = tenant.TenantId;
                    tenantSheet.Cells[tenantSheetRow, 2].Value = tenant.TenantName;
                    tenantSheetRow++;

                    var roleSheetRow = 2;
                    if (!string.IsNullOrEmpty(tenant.ConnectString))
                    {
                        _applicationUserService.SetConnectDB(tenant.ConnectString);
                        var roles = await _roleService.GetAllQuery(new GetAllQueryRequest());
                        if (roles.Data != null)
                        {
                            foreach (var item in roles.Data)
                            {
                                roleSheet.Row(roleSheetRow).Height = 20;
                                roleSheet.Cells[roleSheetRow, 1].Value = item.Name;
                                roleSheet.Cells[roleSheetRow, 2].Value = item.DisplayName;
                                roleSheet.Cells[roleSheetRow, 3].Value = tenant.TenantName;
                                roleSheet.Cells[roleSheetRow, 4].Value = tenant.TenantId;
                                roleSheetRow++;
                            }
                        }

                    }
                }

                roleSheet.Cells.AutoFitColumns();
                tenantSheet.Cells.AutoFitColumns();
                #endregion


                return File(excel.GetAsByteArray(), "application/vnd.ms-excel", String.Format("{0}.xlsx", "File mẫu import thu nhập"));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod().Name);
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("import-sample-v2")]
        public async Task<IActionResult> ImportExampleV2()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                string locale = GetLocale();
                string userName = GetCurrentUserName();
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage excel = new ExcelPackage();

                #region Sheet Salary
                var salarySheet = excel.Workbook.Worksheets.Add("Danh sách thu nhập trong năm");
                salarySheet = ExcelExportHelper.GetStyle(salarySheet, 14);

                salarySheet.Cells[1, 1].Value = "Mã tổ chức";
                salarySheet.Cells[1, 2].Value = "Email nhân viên";
                salarySheet.Cells[1, 3].Value = "Tháng";
                salarySheet.Cells[1, 4].Value = "Năm";
                salarySheet.Cells[1, 5].Value = "Mã vị trí 1";
                salarySheet.Cells[1, 6].Value = "Thu nhập theo vị trí 1";
                salarySheet.Cells[1, 7].Value = "Mã vị trí 2";
                salarySheet.Cells[1, 8].Value = "Thu nhập theo vị trí 2";
                salarySheet.Cells[1, 9].Value = "Mã vị trí 3";
                salarySheet.Cells[1, 10].Value = "Thu nhập theo vị trí 3";
                salarySheet.Cells[1, 11].Value = "Mã vị trí 4";
                salarySheet.Cells[1, 12].Value = "Thu nhập theo vị trí 4";
                salarySheet.Cells[1, 13].Value = "Mã vị trí 5";
                salarySheet.Cells[1, 14].Value = "Thu nhập theo vị trí 5";
                salarySheet.Cells[1, 15].Value = "Các khoản thu nhập khác";
                salarySheet.Cells[1, 16].Value = "Tổng thu nhập trước thuế";
                salarySheet.Cells[1, 17].Value = "Tổng thu nhập không chịu thuế";
                salarySheet.Cells[1, 18].Value = "Tổng giảm trừ gia cảnh";
                salarySheet.Cells[1, 19].Value = "Tổng tiền BHXH NLD đóng tháng";
                salarySheet.Cells[1, 20].Value = "Tổng thu nhập chịu thuế";
                salarySheet.Cells[1, 21].Value = "Tổng thuế TNCN tạm thu";
                salarySheet.Cells[1, 22].Value = "Tổng thu nhập nhận được";

                int currRow = 2;

                int currentMonth = DateTime.Now.Month;
                int currentYear = DateTime.Now.Year;

                for (var index = 1; index <= 5; index++)
                {
                    salarySheet.Row(currRow).Height = 20;
                    salarySheet.Cells[currRow, 1].Value = $"dev{index}";
                    salarySheet.Cells[currRow, 2].Value = $"taikhoan{index}@gmail.com";
                    salarySheet.Cells[currRow, 3].Value = $"{currentMonth}";
                    salarySheet.Cells[currRow, 4].Value = $"{currentYear}";
                    salarySheet.Cells[currRow, 5].Value = $"Sale";
                    salarySheet.Cells[currRow, 6].Value = $"{index}00";
                    salarySheet.Cells[currRow, 7].Value = $"Supplier";
                    salarySheet.Cells[currRow, 8].Value = $"{index}00";
                    salarySheet.Cells[currRow, 9].Value = $"";
                    salarySheet.Cells[currRow, 10].Value = $"";
                    salarySheet.Cells[currRow, 11].Value = $"";
                    salarySheet.Cells[currRow, 12].Value = $"";
                    salarySheet.Cells[currRow, 13].Value = $"";
                    salarySheet.Cells[currRow, 14].Value = $"";
                    salarySheet.Cells[currRow, 15].Value = $"{index}00";
                    salarySheet.Cells[currRow, 16].Value = $"{index}00";
                    salarySheet.Cells[currRow, 17].Value = $"{index}00";
                    salarySheet.Cells[currRow, 18].Value = $"{index}00";
                    salarySheet.Cells[currRow, 19].Value = $"{index}00";
                    salarySheet.Cells[currRow, 20].Value = $"{index}00";
                    salarySheet.Cells[currRow, 21].Value = $"{index}00";
                    salarySheet.Cells[currRow, 22].Value = $"{index}00";
                    currRow++;
                }

                salarySheet.Cells.AutoFitColumns();
                #endregion

                #region Sheet role
                var tenants = _userService.ListUserTenant(GetCurrentUser() ?? Guid.Empty);


                var roleSheet = excel.Workbook.Worksheets.Add("Danh sách vị trí");
                roleSheet = ExcelExportHelper.GetStyle(roleSheet, 4);

                roleSheet.Cells[1, 1].Value = "Mã vị trí";
                roleSheet.Cells[1, 2].Value = "Tên vị trí";
                roleSheet.Cells[1, 3].Value = "Ghi chú";
                roleSheet.Cells[1, 4].Value = "Mã tổ chức";

                var tenantSheet = excel.Workbook.Worksheets.Add("Danh sách tổ chức");
                tenantSheet = ExcelExportHelper.GetStyle(tenantSheet, 2);
                tenantSheet.Cells[1, 1].Value = "Mã tổ chức";
                tenantSheet.Cells[1, 2].Value = "Tên tổ chức";

                var tenantSheetRow = 2;
                foreach (var tenant in tenants)
                {
                    tenantSheet.Row(tenantSheetRow).Height = 20;
                    tenantSheet.Cells[tenantSheetRow, 1].Value = tenant.TenantId;
                    tenantSheet.Cells[tenantSheetRow, 2].Value = tenant.TenantName;
                    tenantSheetRow++;

                    var roleSheetRow = 2;
                    if (!string.IsNullOrEmpty(tenant.ConnectString))
                    {
                        _applicationUserService.SetConnectDB(tenant.ConnectString);
                        var roles = await _roleService.GetAllQuery(new GetAllQueryRequest());
                        if (roles.Data != null)
                        {
                            foreach (var item in roles.Data)
                            {
                                roleSheet.Row(roleSheetRow).Height = 20;
                                roleSheet.Cells[roleSheetRow, 1].Value = item.Name;
                                roleSheet.Cells[roleSheetRow, 2].Value = item.DisplayName;
                                roleSheet.Cells[roleSheetRow, 3].Value = tenant.TenantName;
                                roleSheet.Cells[roleSheetRow, 4].Value = tenant.TenantId;
                                roleSheetRow++;
                            }
                        }

                    }
                }

                roleSheet.Cells.AutoFitColumns();
                tenantSheet.Cells.AutoFitColumns();
                #endregion


                return File(excel.GetAsByteArray(), "application/vnd.ms-excel", String.Format("{0}.xlsx", "File mẫu import thu nhập"));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod().Name);
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("mobile/import-sample")]
        public async Task<IActionResult> MobileImportExample()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                var excel = await Mediator.Send(new EmployeeSalaryMobile_ImportSampleCommand(userId));

                return File(excel.GetAsByteArray(), "application/vnd.ms-excel", String.Format("{0}.xlsx", "Danh sách chi tiết thu nhập theo ticket"));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("export-by-job-position")]
        public async Task<IActionResult> ExportPosition([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                string locale = GetLocale();
                request.UserId = GetCurrentUser();

                var listUserTenant = _userService.ListAllUserTenant(GetCurrentUser() ?? Guid.Empty, request.PageIndex, request.PageSize);
                var listData = new List<EmployeeSalaryAllAdminRoleReportDto>();
                if (listUserTenant != null)
                {
                    var userId = GetCurrentUser() ?? Guid.Empty;
                    EmployeeSalaryAllAdminRoleReportDto userInfo = null;
                    foreach (var item in listUserTenant)
                    {
                        _applicationUserService.SetConnectDB(item.ConnectString ?? "");
                        GetListWithPaginationQueryRequest requestNew = new GetListWithPaginationQueryRequest()
                        {
                            PageIndex = 1,
                            PageSize = 100,
                            UserId = item.ApplicationUserId,
                            Time = request.Time
                        };
                        var listEmployee = await Mediator.Send(new EmployeeSalaryAdminByRole_GetListWithPaginationQuery(userId, requestNew));
                        if (listEmployee.Data != null)
                        {
                            foreach (var m in listEmployee.Data.Items)
                            {
                                if (listData.FindIndex(x => x.UserName == item.UserName) == -1)
                                {
                                    userInfo = new EmployeeSalaryAllAdminRoleReportDto();
                                    userInfo.ApplicationUser = m.ApplicationUser;
                                    userInfo.UserName = item.UserName;
                                    userInfo.IncomeOther = m.IncomeOther;
                                    userInfo.TotalIncome = m.TotalIncome;
                                    userInfo.TenantNameIncomRoles = new List<TenantNameIncomRole>();
                                    userInfo.TenantNameIncomRoles.Add(new TenantNameIncomRole()
                                    {
                                        TenantName = item.TenantName,
                                        IncomeRoles = m.IncomeRoles
                                    });
                                    listData.Add(userInfo);
                                }
                                else
                                {
                                    userInfo.IncomeOther += m.IncomeOther;
                                    userInfo.TotalIncome += m.TotalIncome;
                                    userInfo.TenantNameIncomRoles.Add(new TenantNameIncomRole()
                                    {
                                        TenantName = item.TenantName,
                                        IncomeRoles = m.IncomeRoles
                                    });
                                }
                            }
                        }

                    }

                }

                if (listData == null)
                    throw new ApplicationException("Xuất file excel thất bại!");

                List<string> listTenant = new List<string>();
                List<string> listLabelTenant = new List<string>();
                foreach (var item in listData)
                {
                    foreach (var tenant in item.TenantNameIncomRoles)
                    {
                        if (listTenant.Where(lt => lt.Contains(tenant.TenantName)).FirstOrDefault() != null)
                        {

                            continue;
                        }
                        listTenant.Add(tenant.TenantName ?? "");
                    }
                }

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage excel = new ExcelPackage();
                string listName = locale == LocaleEnum.vi_VN.ToString() ? "Danh sách thu nhập theo vị trí" : "List income by job position";
                var workSheet = excel.Workbook.Worksheets.Add(listName);
                workSheet = ExcelExportHelper.GetStyle(workSheet, 11);

                workSheet.Cells[1, 1].Value = "ID";
                workSheet.Cells[1, 2].Value = "MÃ NHÂN VIÊN";
                workSheet.Cells[1, 3].Value = "HỌ VÀ TÊN";
                workSheet.Cells[1, 4].Value = "VỊ TRÍ";
                workSheet.Cells[1, 5].Value = "TỔNG THU NHẬP TRƯỚC THUẾ";
                workSheet.Cells[1, 6].Value = "TỔNG THU NHẬP KHÔNG CHỊU THUẾ";
                workSheet.Cells[1, 7].Value = "TỔNG GIẢM TRỪ GIA CẢNH";
                workSheet.Cells[1, 8].Value = "TỔNG TIỀN BHXH NLD ĐÓNG";
                workSheet.Cells[1, 9].Value = "TỔNG THU NHẬP CHỊU THUẾ";
                workSheet.Cells[1, 10].Value = "TỔNG THUẾ TNCN TẠM THU";
                workSheet.Cells[1, 11].Value = "TỔNG THU NHẬP NHẬN ĐƯỢC";

                if (locale == LocaleEnum.en_US.ToString())
                {
                    workSheet.Cells[1, 2].Value = "HUMAN RESOURCE CODE";
                    workSheet.Cells[1, 3].Value = "FULL NAME";
                    workSheet.Cells[1, 4].Value = "POSITION";
                    workSheet.Cells[1, 5].Value = "TOTAL INCOME BEFORE TAXES";
                    workSheet.Cells[1, 6].Value = "TOTAL INCOME IS NOT TAXABLE";
                    workSheet.Cells[1, 7].Value = "TOTAL DEDUCTION DUE TO FAMILY CIRCUMSTANCES";
                    workSheet.Cells[1, 8].Value = "TOTAL SOCIAL INSURANCE AMOUNT";
                    workSheet.Cells[1, 9].Value = "TOTAL TAXABLE INCOME";
                    workSheet.Cells[1, 10].Value = "TOTAL TEMPORARY TAX COLLECTED";
                    workSheet.Cells[1, 11].Value = "TOTAL INCOME RECEIVED";
                }

                //foreach (var item in listData)
                //{
                //    workSheet.Row(currRow).Height = 20;
                //    workSheet.Cells[currRow, 1].Value = item.ApplicationUser.Id;
                //    workSheet.Cells[currRow, 2].Value = item.ApplicationUser.Code;
                //    workSheet.Cells[currRow, 3].Value = item.ApplicationUser.FullName;
                //    workSheet.Cells[currRow, 4].Value = item.TenantNameRoles.RoleNames;
                //    workSheet.Cells[currRow, 5].Value = item.IncomeBeforeTax;
                //    workSheet.Cells[currRow, 6].Value = item.IncomeNonTax;
                //    workSheet.Cells[currRow, 7].Value = item.Dependent;
                //    workSheet.Cells[currRow, 8].Value = item.Insurance;
                //    workSheet.Cells[currRow, 9].Value = item.IncomeTax;
                //    workSheet.Cells[currRow, 10].Value = item.PersonalIncomeTax;
                //    workSheet.Cells[currRow, 11].Value = item.IncomeRecevied;
                //    currRow++;
                //}

                workSheet.Cells.AutoFitColumns();

                return File(excel.GetAsByteArray(), "application/vnd.ms-excel", String.Format("{0}.xlsx", listName));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("mobile/tong-thu-nhap-cua-ban-than")]
        public async Task<IActionResult> GetTotalIncomeOfCurrentUser([FromQuery] Guid groupTenantId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                Guid userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new EmployeeSalaryMobile_GetTotalIncomeCurrentUserQuery(userId, groupTenantId)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("mobile/thong-tin-thu-nhap-cua-ban-than-theo-nam")]
        public async Task<IActionResult> GetTotalIncomeOfCurrentUserByYear()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                Guid userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new EmployeeSalaryMobile_GetTotalIncomeCurrentUserByYearQuery(userId)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("mobile/thu-nhap-truoc-thue-cua-ban-than-theo-nam/{year}")]
        public async Task<IActionResult> GetListIncomeBeforeTaxOfCurrentUserByYear(int year)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                Guid userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new EmployeeSalaryMobile_GetListIncomeBeforeTaxByYearQuery(userId, year)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("mobile/cac-khoan-thu-nhap-khong-chiu-thue-cua-ban-than-theo-nam/{year}")]
        public async Task<IActionResult> GetListIncomeNonTaxOfCurrentUserByYear(int year)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                Guid userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new EmployeeSalaryMobile_GetListIncomeNonTaxByYearQuery(userId, year)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("mobile/thong-tin-giam-tru-gia-canh-cua-ban-than-theo-nam/{year}")]
        public async Task<IActionResult> GetListIncomeDependentOfCurrentUserByYear(int year)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                Guid userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new EmployeeSalaryMobile_GetListDependentByYearQuery(userId, year)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("mobile/thong-tin-bao-hiem-xa-hoi-cua-ban-than-theo-nam/{year}")]
        public async Task<IActionResult> GetListIncomeInsuranceOfCurrentUserByYear(int year)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                Guid userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new EmployeeSalaryMobile_GetListInsuranceByYearQuery(userId, year)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("mobile/thong-tin-thu-nhap-chiu-thue-cua-ban-than-theo-nam/{year}")]
        public async Task<IActionResult> GetListIncomeTaxOfCurrentUserByYear(int year)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                Guid userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new EmployeeSalaryMobile_GetListIncomeTaxByYearQuery(userId, year)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("mobile/thue-thu-nhap-ca-nhan-tam-thu-cua-ban-than-theo-nam/{year}")]
        public async Task<IActionResult> GetListIncomePersonalTaxOfCurrentUserByYear(int year)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                Guid userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new EmployeeSalaryMobile_GetListIncomePersonalTaxByYearQuery(userId, year)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("mobile/thu-nhap-nhan-duoc-cua-ban-than-theo-nam/{year}")]
        public async Task<IActionResult> GetListIncomeReceivedOfCurrentUserByYear(int year)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                Guid userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new EmployeeSalaryMobile_GetListIncomeReceivedByYearQuery(userId, year)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }
    }
}
