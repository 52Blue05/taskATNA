using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.ApplicationUserStatusFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.ApplicationService;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.Identity;
using Sale_Saas.Application.Utilities;
using Sale_Saas.Domain.Constants.API;
using Sale_Saas.Domain.Enums;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
namespace Sale_Saas.Infrastructure.Services.Identity;

public class ApplicationUserService : IApplicationUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _storageService;
    private readonly ImageConstant _imageConstant;
    private readonly IFeaturePermissionService _permissionService;
    private readonly ApplicationUserStatusConstant _applicationUserStatusConstant;
    private readonly IInternalService _internalService;
    private readonly IMapper _mapper;
    private readonly TenantDbContext _tenantContext;
    private readonly IApplicationRoleService _roleService;

    public ApplicationUserService(UserManager<ApplicationUser> userManager,
                                    SignInManager<ApplicationUser> signInManager,
                                    RoleManager<ApplicationRole> roleManager,
                                    IFileStorageService storageService,
                                    ApplicationDbContext context,
                                    IOptions<ImageConstant> imageConstant,
                                    IFeaturePermissionService permissionService,
                                    IOptions<ApplicationUserStatusConstant> applicationUserStatusConstant,
                                    IInternalService internalService,
                                    IMapper mapper,
                                    TenantDbContext tenantContext,
                                    IApplicationRoleService roleService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _storageService = storageService;
        _context = context;
        _imageConstant = imageConstant.Value;
        _permissionService = permissionService;
        _applicationUserStatusConstant = applicationUserStatusConstant.Value;
        _internalService = internalService;
        _mapper = mapper;
        _tenantContext = tenantContext;
        _roleService = roleService;
    }
    private string GenerateToken(string applicationUserId, string userName, string roles, string applicationRoleDetails,
                                string avatar, string lastName, string firstName, string email, string tenant, string groupTenant)
    {

        var authClaims = new[]
        {
            new Claim(AppJwtClaimTypeConstant.ApplicationUserId, applicationUserId),
            new Claim(AppJwtClaimTypeConstant.UserName, userName),
            new Claim(AppJwtClaimTypeConstant.FullName, lastName + " " + firstName),
            new Claim(AppJwtClaimTypeConstant.Email, email),
            new Claim(AppJwtClaimTypeConstant.Avatar, (!string.IsNullOrEmpty(avatar) ? avatar : _imageConstant.FileNoImagePerson)),
            new Claim(AppJwtClaimTypeConstant.Roles, roles),
            new Claim(AppJwtClaimTypeConstant.ApplicationRoleDetails, applicationRoleDetails),
            new Claim(AppJwtClaimTypeConstant.Tenant, tenant),
            new Claim(AppJwtClaimTypeConstant.GroupTenant, groupTenant)
        };

        SymmetricSecurityKey authSigningKey = new(Encoding.UTF8.GetBytes(JWTConstant.Secret));

        JwtSecurityToken token = new(
            JWTConstant.ValidIssuer,
            JWTConstant.ValidAudience,
            expires: DateTime.Now.AddDays(7),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256Signature)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
    public DateTime GetValidTo(string jwt)
    {
        JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
        JwtSecurityToken? jwtSecurityToken = handler.ReadJwtToken(jwt);
        return jwtSecurityToken.ValidTo;
    }
    public string RefreshToken(string jwt, int exp)
    {
        JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
        JwtSecurityToken? jwtSecurityToken = handler.ReadJwtToken(jwt);
        SymmetricSecurityKey authSigningKey = new(Encoding.UTF8.GetBytes(JWTConstant.Secret));

        JwtSecurityToken token = new(
            JWTConstant.ValidIssuer,
            JWTConstant.ValidAudience,
            expires: DateTime.Now.AddDays(exp),
            claims: jwtSecurityToken.Claims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256Signature)
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    private string ConvertToString(List<ApplicationRoleDetailDto> applicationRoleDetails)
    {
        string result = string.Empty;

        if (applicationRoleDetails != null && applicationRoleDetails.Count > 0)
        {
            foreach (ApplicationRoleDetailDto item in applicationRoleDetails)
            {
                result += item.NameController + "," + item.NameAction + "," + item.Permision.ToString() + "|";
            }
        }

        return result;
    }

    public async Task<Result<LoginDto>> Login(LoginRequest request, string tenantId, string groupTenantId)
    {
        _context.CurrentTenantId = tenantId;
        var user = await _userManager.FindByEmailAsync(request.UserName);
        if (user == null)
        {
            throw new ApplicationException("Không tìm thấy tài khoản trong tổ chức");
        }

        #region old-logic
        /*else if (await _userManager.CheckPasswordAsync(user, request.Password) == false)
        {
            throw new ApplicationException("Sai mật khẩu.");
        }
        else if (user.ApplicationUserStatusId == _applicationUserStatusConstant.InActive)
        {
            throw new ApplicationException("Tài khoản chưa được kích hoạt.");
        }

        var result = await _signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe, true);
        if (!result.Succeeded)
        {
            throw new ApplicationException("Sai mật khẩu.");
        }*/
        #endregion

        var roles = await _userManager.GetRolesAsync(user);

        var applicationRoleDetails = await (from ard in _context.ApplicationRoleDetails
                                            join m in _context.Menus on ard.MenuId equals m.Id
                                            join ar in _context.ApplicationRoles on ard.ApplicationRoleId equals ar.Id
                                            join ur in _context.UserRoles on ar.Id equals ur.RoleId
                                            where ur.UserId == user.Id
                                              && !string.IsNullOrEmpty(m.NameController)
                                               && !string.IsNullOrEmpty(m.NameAction)
                                            select new ApplicationRoleDetailDto()
                                            {
                                                NameController = m.NameController,
                                                NameAction = m.NameAction,
                                                Permision = ard.Permission
                                            }).AsNoTracking().ToListAsync();

        string token = GenerateToken(user.Id.ToString(), user.UserName ?? string.Empty, string.Join("|", roles),
                                        ConvertToString(applicationRoleDetails),
                                        user.Avatar ?? string.Empty,
                                        user.LastName ?? string.Empty,
                                        user.FirstName ?? string.Empty,
                                        user.Email ?? string.Empty, tenantId ?? string.Empty,
                                        groupTenantId);
        DateTime ValidTo = GetValidTo(token);
        var refresh = GenerateRefreshToken();
        return Result<LoginDto>.Success(new LoginDto() { Token = token, ValidTo = ValidTo, AccessToken = token, RefreshToken = refresh });
    }

    public async Task<Result<LoginDto>> LoginWithNoTenant(ApplicationUserWithTenantDto request)
    {
        var groupTenant = request.GroupTenantId.ToString() ?? "";
        string token = GenerateToken(request.Id.ToString(), request.UserName ?? string.Empty, "",
                                        ConvertToString(new List<ApplicationRoleDetailDto>()),
                                        string.Empty,
                                        request.LastName ?? string.Empty,
                                        request.FirstName ?? string.Empty,
                                        request.Email ?? string.Empty, string.Empty,
                                        groupTenant);
        DateTime ValidTo = GetValidTo(token);
        var refresh = GenerateRefreshToken();
        return Result<LoginDto>.Success(new LoginDto() { Token = token, ValidTo = ValidTo, AccessToken = token, RefreshToken = refresh });
    }

    public async Task<Result<ApplicationUserDto>> GetById(Guid id)
    {
        var user = await (from u in _userManager.Users
                          join aus in _context.ApplicationUserStatuses on u.ApplicationUserStatusId equals aus.Id into u_aus
                          from aus in u_aus.DefaultIfEmpty()
                          where u.Id == id
                          select new ApplicationUserDto()
                          {
                              Id = u.Id,
                              Avatar = u.Avatar ?? string.Empty,
                              LastName = u.LastName ?? string.Empty,
                              FirstName = u.FirstName ?? string.Empty,
                              Email = u.Email ?? string.Empty,
                              Phone = u.PhoneNumber ?? string.Empty,
                              UserName = u.UserName ?? string.Empty,
                              FullName = u.FullName ?? string.Empty,
                              Code = u.Code,
                              DateOfBirth = u.DateOfBirth,
                              StartDate = u.StartDate,
                              EndDate = u.EndDate,
                              Review = u.Review ?? string.Empty,
                              Notes = u.Notes ?? string.Empty,
                              ApplicationUserStatus = aus != null ? new ApplicationUserStatusDto
                              {
                                  Id = aus.Id,
                                  Code = aus.Code ?? "",
                                  Name = aus.Name ?? ""
                              } : null
                          }
                        ).AsNoTracking().FirstOrDefaultAsync();

        if (user == null) throw new ApplicationException("Không tìm thấy tài khoản này.");

        user.ApplicationRoles = await (from ur in _context.UserRoles
                                       join r in _context.Roles on ur.RoleId equals r.Id
                                       where ur.UserId == id && r.RolePositionId != RolePositionEnum.ADMIN.ToString()
                                       select new ApplicationRoleDto()
                                       {
                                           Id = r.Id,
                                           Name = r.Name ?? string.Empty,
                                           DisplayName = r.DisplayName ?? string.Empty,
                                           Description = r.Description ?? string.Empty,
                                           RolePositionId = r.RolePositionId ?? string.Empty
                                       }).AsNoTracking().ToListAsync();

        return Result<ApplicationUserDto>.Success(user);
    }

    public async Task<Result<ApplicationUser>> GetUserById(Guid id)
    {
        var user = await _context.ApplicationUsers.Where(a => a.Id == id).AsNoTracking().FirstOrDefaultAsync();

        if (user == null) throw new ApplicationException("Không tìm thấy tài khoản này.");

        return Result<ApplicationUser>.Success(user);
    }

    public async Task<CreatedUserDto> GetUserBasicInforById(Guid userId)
    {
        var user = await _context.ApplicationUsers.Where(a => a.Id == userId)
                                                  .AsNoTracking()
                                                  .FirstOrDefaultAsync();
        var userRole = (await _roleService.GetListRoleByUserId(userId))
                                           .Where(x => x.Name == "Admin" || x.Name == "Administrator" || x.Name == "SaleDirector")
                                           .Select(s => s.Name)
                                           .FirstOrDefault();

        if (user == null) throw new ApplicationException("Không tìm thấy tài khoản này.");

        var result = new CreatedUserDto()
        {
            Id = user.Id,
            FullName = user.FullName,
            Role = userRole ?? ""
        };

        return result;
    }

    public void SetConnectDB(string connectString)
    {
        _context.SetConnectString(connectString);
    }

    public void ClearConnectDB()
    {
        _context.ChangeTracker.Clear();
        //_context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
    }

    public void DropDB()
    {
        _context.Database.EnsureDeleted();
    }

    public async Task<UserBasicInfoDto> GetUserBasicById(Guid id)
    {
        var avatar = await GetAvatar(id);

        var user = await (from u in _userManager.Users
                          join aus in _context.ApplicationUserStatuses on u.ApplicationUserStatusId equals aus.Id into u_aus
                          from aus in u_aus.DefaultIfEmpty()
                          where u.Id == id
                          select new UserBasicInfoDto()
                          {
                              Id = u.Id,
                              LastName = u.LastName ?? string.Empty,
                              FirstName = u.FirstName ?? string.Empty,
                              Email = u.Email ?? string.Empty,
                              Phone = u.PhoneNumber ?? string.Empty,
                              FullName = u.FullName ?? string.Empty,
                              Code = u.Code,
                              Address = u.Address ?? string.Empty,
                              DateOfBirth = u.DateOfBirth,
                              Avatar = avatar ?? "",
                          }
                        ).AsNoTracking().FirstOrDefaultAsync();

        return user;
    }
    public async Task<Result<PaginatedList<ApplicationUserDto>>> GetListWithPaginationQuery(GetListApplicationUserWithPaginationQueryRequest request)
    {
        var query = from u in _context.ApplicationUsers
                    join aus in _context.ApplicationUserStatuses on u.ApplicationUserStatusId equals aus.Id into u_aus
                    from aus in u_aus.DefaultIfEmpty()
                    where u.DeleteFlag != true
                    select new ApplicationUserDto()
                    {
                        Id = u.Id,
                        Avatar = u.Avatar ?? string.Empty,
                        LastName = u.LastName ?? string.Empty,
                        FirstName = u.FirstName ?? string.Empty,
                        Email = u.Email ?? string.Empty,
                        Phone = u.PhoneNumber ?? string.Empty,
                        UserName = u.UserName ?? string.Empty,
                        FullName = u.FullName ?? string.Empty,
                        Address = u.Address ?? string.Empty,
                        Code = u.Code,
                        DateOfBirth = u.DateOfBirth,
                        StartDate = u.StartDate,
                        EndDate = u.EndDate,
                        Review = u.Review ?? string.Empty,
                        Notes = u.Notes ?? string.Empty,
                        ApplicationUserStatus = aus != null ? new ApplicationUserStatusDto
                        {
                            Id = aus.Id,
                            Code = aus.Code ?? "",
                            Name = aus.Name ?? ""
                        } : null
                    };

        if (!string.IsNullOrEmpty(request.TextSearch))
        {
            query = query.Where(x => x.UserName.Contains(request.TextSearch)
                                 || (x.FirstName + " " + x.LastName).Contains(request.TextSearch)
                                 || x.Email.Contains(request.TextSearch));
        }
        if (!string.IsNullOrEmpty(request.Email))
            query = query.Where(x => x.Email.Contains(request.Email.Trim()));
        if (!string.IsNullOrEmpty(request.FullName))
            query = query.Where(x => x.FullName.Contains(request.FullName.Trim()));
        if (!string.IsNullOrEmpty(request.RoleName))
        {
            query = from q in query
                    join ur in _context.UserRoles on q.Id equals ur.UserId
                    join ar in _context.ApplicationRoles on ur.RoleId equals ar.Id
                    where ar.DisplayName.Contains(request.RoleName.Trim())
                    select q;
        }
        /*
				if (request.UserId != null)
				{
					var position = await _permissionService.GetRolePositionByUser((Guid)request.UserId);
					if (position == RolePositionEnum.EMPLOYEE.ToString())
					{
						request.RoleType = RoleType.MYSELF.ToString();
					}
				}*/

        //3. Paging
        int totalRecords = await query.CountAsync();
        if (!string.IsNullOrEmpty(request.OrderCol))
        {
            if (string.IsNullOrEmpty(request.OrderDir)) request.OrderDir = "ASC";
            query = FunctionUtilServices.OrderByDynamic(query, request.OrderCol, request.OrderDir.ToUpper() == "DESC" ? false : true);
        }
        query = query.Skip((request.PageIndex - 1) * request.PageSize)
                            .Take(request.PageSize);

        var data = await query.AsNoTracking().ToListAsync();

        List<ApplicationRoleDto> applicationRoles = await (from ur in _context.UserRoles
                                                           join ar in _context.ApplicationRoles on ur.RoleId equals ar.Id
                                                           where data.Select(m => m.Id).ToList().Contains(ur.UserId) && ar.IsModified != false
                                                           select new ApplicationRoleDto()
                                                           {
                                                               Id = ar.Id,
                                                               DisplayName = ar.DisplayName ?? string.Empty,
                                                               ApplicationUserId = ur.UserId,
                                                               RolePositionId = ar.RolePositionId ?? string.Empty
                                                           }).AsNoTracking().ToListAsync();
        foreach (var user in data)
        {
            user.ApplicationRoles = applicationRoles.Where(m => m.ApplicationUserId == user.Id).ToList();
        }

        return Result<PaginatedList<ApplicationUserDto>>.Success(new PaginatedList<ApplicationUserDto>(data, totalRecords, request.PageIndex, request.PageSize));
    }

    private async Task<List<string>> GetListRoleByUserId(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return new List<string>();
        }
        return (List<string>)await _userManager.GetRolesAsync(user);
    }

    public async Task<Result<List<ApplicationUserDto>>> GetAllQueryAsync(GetAllQueryRequest request)
    {
        var query = from u in _context.ApplicationUsers
                    join aus in _context.ApplicationUserStatuses on u.ApplicationUserStatusId equals aus.Id into u_aus
                    from aus in u_aus.DefaultIfEmpty()
                    select new ApplicationUserDto()
                    {
                        Id = u.Id,
                        Avatar = u.Avatar ?? string.Empty,
                        LastName = u.LastName ?? string.Empty,
                        FirstName = u.FirstName ?? string.Empty,
                        Email = u.Email ?? string.Empty,
                        Phone = u.PhoneNumber ?? string.Empty,
                        UserName = u.UserName ?? string.Empty,
                        FullName = u.FullName ?? string.Empty,
                        Code = u.Code,
                        DateOfBirth = u.DateOfBirth,
                        StartDate = u.StartDate,
                        EndDate = u.EndDate,
                        Review = u.Review ?? string.Empty,
                        Notes = u.Notes ?? string.Empty,
                        ApplicationUserStatus = aus != null ? new ApplicationUserStatusDto
                        {
                            Id = aus.Id,
                            Code = aus.Code ?? "",
                            Name = aus.Name ?? ""
                        } : null
                    };

        if (!string.IsNullOrEmpty(request.TextSearch))
        {
            query = query.Where(x => x.UserName.Contains(request.TextSearch)
                                     || (x.FirstName + " " + x.LastName).Contains(request.TextSearch)
                                     || x.Email.Contains(request.TextSearch));
        }

        if (!string.IsNullOrEmpty(request.RoleType))
        {
            if (request.RoleType == RoleType.EMPLOYEE.ToString())
            {
                var ids = await GetUserIdByRolePosition(RolePositionEnum.EMPLOYEE.ToString());
                query = query.Where(s => s.Id != null && ids.Contains((Guid)s.Id));
            }
        }

        var data = await query.AsNoTracking().ToListAsync();

        List<ApplicationRoleDto> applicationRoles = await (from ur in _context.UserRoles
                                                           join ar in _context.ApplicationRoles on ur.RoleId equals ar.Id
                                                           where data.Select(m => m.Id).ToList().Contains(ur.UserId)
                                                           select new ApplicationRoleDto()
                                                           {
                                                               Id = ar.Id,
                                                               DisplayName = ar.DisplayName ?? string.Empty,
                                                               ApplicationUserId = ur.UserId,
                                                               RolePositionId = ar.RolePositionId ?? string.Empty
                                                           }).AsNoTracking().ToListAsync();
        foreach (var user in data)
        {
            user.ApplicationRoles = applicationRoles.Where(m => m.ApplicationUserId == user.Id).ToList();
        }

        return Result<List<ApplicationUserDto>>.Success(data);
    }
    public async Task<Result<List<ApplicationUserDto>>> FilterQueryAsync(FilterQueryRequest request)
    {
        var query = from u in _context.ApplicationUsers
                    join aus in _context.ApplicationUserStatuses on u.ApplicationUserStatusId equals aus.Id into u_aus
                    from aus in u_aus.DefaultIfEmpty()
                    select new ApplicationUserDto()
                    {
                        Id = u.Id,
                        Avatar = u.Avatar ?? string.Empty,
                        LastName = u.LastName ?? string.Empty,
                        FirstName = u.FirstName ?? string.Empty,
                        Email = u.Email ?? string.Empty,
                        Phone = u.PhoneNumber ?? string.Empty,
                        UserName = u.UserName ?? string.Empty,
                        FullName = u.FullName ?? string.Empty,
                        Code = u.Code,
                        DateOfBirth = u.DateOfBirth,
                        StartDate = u.StartDate,
                        EndDate = u.EndDate,
                        Review = u.Review ?? string.Empty,
                        Notes = u.Notes ?? string.Empty,
                        ApplicationUserStatus = aus != null ? new ApplicationUserStatusDto
                        {
                            Id = aus.Id,
                            Code = aus.Code ?? "",
                            Name = aus.Name ?? ""
                        } : null
                    };

        if (!string.IsNullOrEmpty(request.TextSearch))
        {
            query = query.Where(x => x.UserName.Contains(request.TextSearch) ||
                                     x.FullName.Contains(request.TextSearch));
        }

        if (!string.IsNullOrEmpty(request.RoleType))
        {
            if (request.RoleType == RoleType.EMPLOYEE.ToString())
            {
                var ids = await GetUserIdByRolePosition(RolePositionEnum.EMPLOYEE.ToString());
                query = query.Where(s => s.Id != null && ids.Contains((Guid)s.Id));
            }
        }

        if (!string.IsNullOrEmpty(request.Code))
        {
            query = query.Where(x => x.Code.Contains(request.Code));
        }

        if (request.TotalRecord != null)
        {
            query = query.Take(request.TotalRecord.Value);
        }

        var data = await query.AsNoTracking().ToListAsync();

        return Result<List<ApplicationUserDto>>.Success(data);
    }

    public async Task<Result<List<ApplicationUserDto>>> AddOrUpdateAsync(List<AddOrUpdateApplicationUserRequest> request, string sConnect = "")
    {
        ApplicationUser? obj = null;
        List<ApplicationUserDto> updatedSuccess = new List<ApplicationUserDto>();
        if (sConnect != "")
        {
            _context.CurrentTenantConnectionString = sConnect;
            _context.Database.SetConnectionString(sConnect);
        };
        foreach (AddOrUpdateApplicationUserRequest addOrUpdateRequest in request)
        {
            if (addOrUpdateRequest.Id == null)
            {
                obj = await _userManager.FindByNameAsync(addOrUpdateRequest.UserName);
                if (obj != null) return Result<List<ApplicationUserDto>>.Failure($"{addOrUpdateRequest.UserName} đã có trong dữ liệu.");
                //obj = await _context.ApplicationUsers.Where(x => x.UserName == addOrUpdateRequest.UserName).FirstOrDefaultAsync();
                //if (obj != null) return Result<List<ApplicationUserDto>>.Failure($"{addOrUpdateRequest.UserName} đã có trong dữ liệu.");

                obj = new ApplicationUser()
                {
                    Id = Guid.NewGuid(),
                    UserName = addOrUpdateRequest.UserName,
                    Email = addOrUpdateRequest.Email ?? "",
                    NormalizedEmail = addOrUpdateRequest.UserName.ToUpper(),
                    FirstName = addOrUpdateRequest.FirstName,
                    LastName = addOrUpdateRequest.LastName,
                    FullName = (addOrUpdateRequest.LastName ?? "") + " " + (addOrUpdateRequest.FirstName ?? ""),
                    StartDate = addOrUpdateRequest.StartDate,
                    Avatar = addOrUpdateRequest.Avatar ?? "",
                    PhoneNumber = addOrUpdateRequest.Phone ?? "",
                    DateOfBirth = addOrUpdateRequest.DateOfBirth,
                    CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId ?? Guid.Empty
                };
            }
            else
            {
                obj = await _context.ApplicationUsers.FindAsync(addOrUpdateRequest.Id.Value);

                if (obj == null) return Result<List<ApplicationUserDto>>.Failure($"Không tìm thấy User có id: {addOrUpdateRequest.Id.Value}");
            }

            if (addOrUpdateRequest.Data != null)
            {
                obj = (ApplicationUser)_internalService.MapValueToObject(new ApplicationUser(), addOrUpdateRequest.Data, obj);
            }

            obj.LastModifiedDate = DateTime.Now;
            obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId ?? Guid.Empty;

            if (addOrUpdateRequest.Id == null)
            {

                var exist = await ValidateAttribute(obj.UserName, obj.Email, obj.PhoneNumber);
                if (!string.IsNullOrEmpty(exist)) Result<List<ApplicationUserDto>>.Failure(exist);

                var status = await _context.ApplicationUserStatuses.FirstOrDefaultAsync(s => s.Code == "ACTIVE");
                if (status != null)
                {
                    obj.ApplicationUserStatusId = status.Id;
                    obj.ApplicationUserStatus = status;
                }
                obj.Code = StringHelper.GenerateCode();
                obj.FullName = (obj.LastName ?? "") + " " + (obj.FirstName ?? "");
                var result = await _userManager.CreateAsync(obj, addOrUpdateRequest.Password);
                if (!result.Succeeded) return Result<List<ApplicationUserDto>>.Failure($"Tạo tài khoản {addOrUpdateRequest.UserName} không thành công.");

            }
            else
            {
                var exist = await ValidateAttribute(obj.UserName, obj.Email, obj.PhoneNumber, obj.Id);
                if (!string.IsNullOrEmpty(exist)) return Result<List<ApplicationUserDto>>.Failure(exist);

                var result = await _userManager.UpdateAsync(obj);
                if (!result.Succeeded) return Result<List<ApplicationUserDto>>.Failure($"Cập nhật không thành công.");
            }
            updatedSuccess.Add(_mapper.Map<ApplicationUserDto>(obj));
        }

        await _context.SaveChangesAsync();
        return Result<List<ApplicationUserDto>>.Success(updatedSuccess);
    }

    private async Task<string> ValidateAttribute(string? Username = "", string? Email = "", string? Phone = "", Guid? Id = null)
    {
        var query = _context.ApplicationUsers.Where(s => s.DeleteFlag != true).AsNoTracking();
        if (Id != null)
        {
            query = query.Where(s => s.Id != Id);
        }
        if (!string.IsNullOrEmpty(Username))
        {
            var exist = await query.Where(s => s.UserName == Username).FirstOrDefaultAsync();
            if (exist != null) return $"Tên tài khoản {Username} đã có trong dữ liệu.";
        }
        if (!string.IsNullOrEmpty(Email))
        {
            var exist = await query.Where(s => s.Email == Email).FirstOrDefaultAsync();
            if (exist != null) return $"Email {Username} đã có trong dữ liệu.";
        }
        if (!string.IsNullOrEmpty(Phone))
        {
            var exist = await query.Where(s => s.PhoneNumber == Phone).FirstOrDefaultAsync();
            if (exist != null) return $"Số điện thoại {Phone} đã có trong dữ liệu.";
        }
        return "";
    }

    public async Task<Result<string>> ApplicationRoleAssign(ApplicationRoleAssignRequest request)
    {
        if (request.Id == null || request.Id == Guid.Empty || request.ApplicationRoleIds == null || request.ApplicationRoleIds.Count == 0)
        {
            throw new ApplicationException("Không có dữ liệu gửi đến máy chủ");
        }

        var user = await _userManager.FindByIdAsync(request.Id.ToString());
        if (user == null) throw new ApplicationException($"Tài khoản này không có trong dữ liệu.");

        var roles = await _context.ApplicationRoles
                                  .Where(s => request.ApplicationRoleIds.Contains(s.Id))
                                  .Select(s => s.Name)
                                  .ToListAsync();
        if (!roles.Any()) throw new ApplicationException($"Không tìm thấy quyền trong dữ liệu");

        foreach (var name in roles)
        {
            if (await _userManager.IsInRoleAsync(user, name) == false)
            {
                IdentityResult identityResult = await _userManager.AddToRoleAsync(user, name);
                if (!identityResult.Succeeded) throw new ApplicationException($"Cập nhật quyền mới không thành công.");
            }
        }

        return Result<string>.Success(string.Empty);
    }
    public async Task<Result<ApplicationUserDto>> UpdateUserWithManyRole(ApplicationRoleAssignRequest request)
    {
        if (request.Id == null || request.Id == Guid.Empty ||
            (request.IsAdmin == false && (request.ApplicationRoleIds == null || request.ApplicationRoleIds.Count == 0)))
        {
            throw new ApplicationException("Không có dữ liệu gửi đến máy chủ");
        }

        var user = await _userManager.FindByIdAsync(request.Id.ToString());
        if (user == null) throw new ApplicationException($"Tài khoản này không có trong dữ liệu.");
        var adminRole = await _context.Roles.Where(s => s.Name == "Admin").Select(s => s.Id).FirstOrDefaultAsync();
        var userRoles = await _context.UserRoles.Where(us => us.UserId == user.Id && us.RoleId != adminRole).ToListAsync();

        foreach (var item in userRoles)
        {
            _context.UserRoles.Remove(item);
        }
        await _context.SaveChangesAsync();
        if (request.ApplicationRoleIds.Count > 0)
        {
            var roles = await _context.ApplicationRoles
                                      .Where(s => request.ApplicationRoleIds.Contains(s.Id))
                                      .Select(s => s.Name)
                                      .ToListAsync();
            if (!roles.Any()) throw new ApplicationException($"Không tìm thấy quyền trong dữ liệu");
            foreach (var name in roles)
            {
                if (await _userManager.IsInRoleAsync(user, name) == false)
                {
                    IdentityResult identityResult = await _userManager.AddToRoleAsync(user, name);
                    if (!identityResult.Succeeded) throw new ApplicationException($"Cập nhật quyền mới không thành công.");
                }
            }
            await _context.SaveChangesAsync();
        }
        var response = await GetById(user.Id);
        return Result<ApplicationUserDto>.Success(response.Data);
    }
    public async Task<Result<string>> SaveApplicationRolesAsync(ApplicationUserRequest request)
    {
        if (!string.IsNullOrEmpty(request.ConnectString))
        {
            _context.CurrentTenantConnectionString = request.ConnectString;
            _context.Database.SetConnectionString(request.ConnectString);
        }
        IdentityResult identityResult = null;
        if (request.Id == null) throw new ApplicationException($"Không tìm thấy tham số UserId.");
        var user = await _userManager.FindByIdAsync(request.Id);
        if (user == null) throw new ApplicationException($"Tài khoản này không có trong dữ liệu.");

        identityResult = await _userManager.RemoveFromRolesAsync(user, await _userManager.GetRolesAsync(user));
        if (!identityResult.Succeeded) throw new ApplicationException($"Xóa các quyền trước khi khởi tạo quyền mới không thành công.");

        if (request.ApplicationRoleNames != null)
        {
            foreach (string role in request.ApplicationRoleNames)
            {
                identityResult = await _userManager.AddToRoleAsync(user, role);
                if (!identityResult.Succeeded) throw new ApplicationException($"Khởi tạo quyền mới không thành công.");
            }
        }

        if (request.LastModifiedApplicationUserId != null)
        {
            user.LastModifiedApplicationUserId = request.LastModifiedApplicationUserId.Value;
        }

        user.LastModifiedDate = DateTime.Now;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
            return Result<string>.Success(string.Empty);
        }

        throw new ApplicationException("Lưu không thành công.");
    }
    public async Task<Result<string>> ChangePasswordAsync(ChangePasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());
        if (user == null) throw new ApplicationException("Không tìm thấy tài khoản trong dữ liệu này.");

        var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
        if (result.Succeeded)
        {
            user.LastModifiedApplicationUserId = request.LastModifiedApplicationUserId;
            result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Result<string>.Success(user.Id.ToString());
            }
        }

        throw new ApplicationException("Đổi mật khẩu không thành công.");
    }
    public async Task<Result<string>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());
        if (user == null) throw new ApplicationException("Không tìm thấy tài khoản trong dữ liệu này.");

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

        var result = await _userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);
        if (result.Succeeded)
        {
            user.LastModifiedApplicationUserId = request.LastModifiedApplicationUserId;
            result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Result<string>.Success(user.Id.ToString());
            }
        }

        throw new ApplicationException("Đổi mật khẩu không thành công.");
    }
    private async Task<Result<string>> DeleteById(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) throw new ApplicationException("Không tìm thấy tài khoản này trong dữ liệu.");

        user.DeleteFlag = true;
        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            return Result<string>.Success(string.Empty);
        }

        throw new ApplicationException($"Xóa UserId: {id.ToString()} không thành công");
    }
    public async Task<Result<string>> DeleteByIds(DeleteRequest request)
    {
        string result = string.Empty;

        if (request.Ids == null) throw new ApplicationException("Không tìm thấy tham số Id.");
        List<Guid> ids = request.Ids.Select(m => Guid.Parse(m)).ToList();
        var query = await _context.ApplicationUsers.Where(m => ids.Contains(m.Id)).ToListAsync();
        if (query == null || query.Count == 0) throw new ApplicationException($"Không tìm thấy trong dữ liệu có Id: {string.Join(";", request.Ids)}");

        foreach (var item in query)
        {
            item.DeleteFlag = true;
            item.LastModifiedDate = DateTime.Now;
            item.LastModifiedApplicationUserId = request.ApplicationUserId;

            var identityResult = await _userManager.RemoveFromRolesAsync(item, await _userManager.GetRolesAsync(item));
            if (!identityResult.Succeeded) throw new ApplicationException($"Xóa các quyền trước khi khởi tạo quyền mới không thành công.");
        }

        var goals = await _context.Goals.Where(m => m.UserSuggestId != null && ids.Contains((Guid)m.UserSuggestId)).ToListAsync();
        foreach (var goal in goals)
        {
            goal.DeleteFlag = true;
            goal.LastModifiedDate = DateTime.Now;
            goal.LastModifiedApplicationUserId = request.ApplicationUserId;
        }
        _context.Goals.UpdateRange(goals);

        var benefits = await _context.Benefits.Where(m => m.ApplicationUserId != null && ids.Contains((Guid)m.ApplicationUserId)).ToListAsync();
        foreach (var benefit in benefits)
        {
            benefit.DeleteFlag = true;
            benefit.LastModifiedDate = DateTime.Now;
            benefit.LastModifiedApplicationUserId = request.ApplicationUserId;
        }
        _context.Benefits.UpdateRange(benefits);

        var relationships = await _context.Relationships.Where(m => m.ApplicationUserId != null && ids.Contains((Guid)m.ApplicationUserId)).ToListAsync();
        foreach (var relationship in relationships)
        {
            relationship.DeleteFlag = true;
            relationship.LastModifiedDate = DateTime.Now;
            relationship.LastModifiedApplicationUserId = request.ApplicationUserId;
        }
        _context.Relationships.UpdateRange(relationships);

        _context.ApplicationUsers.UpdateRange(query);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        return Result<string>.Success(string.Empty);
    }
    private async Task<string> SaveFile(IFormFile file)
    {
        var originalFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
        await _storageService.SaveFileAsync(file.OpenReadStream(), _imageConstant.UserImagePath + "/" + fileName);
        return fileName;

    }
    private async Task DeleteFile(string fileName)
    {
        await _storageService.DeleteFileAsync(_imageConstant.UserImagePath + "/" + fileName);
    }
    public async Task<Result<string>> DeleteAvatarByApplicationUserId(string applicationUserId)
    {
        var user = await _userManager.FindByIdAsync(applicationUserId);
        if (user == null) throw new ApplicationException($"Tài khoản này không tồn tại.");

        if (!string.IsNullOrEmpty(user.Avatar))
        {
            await DeleteFile(user.Avatar);
        }

        await _userManager.UpdateAsync(user);

        return Result<string>.Success(_imageConstant.NoImageAvailable);
    }

    public async Task<Result<string>> SaveApplicationRolesAdminAsync(ApplicationUserAdminRequest request)
    {
        if (!string.IsNullOrEmpty(request.ConnectString))
        {
            _context.CurrentTenantConnectionString = request.ConnectString;
            _context.Database.SetConnectionString(request.ConnectString);
        }
        if (request.UserName == null) throw new ApplicationException($"Không tìm thấy tham số UserId.");
        var user = await _userManager.FindByNameAsync(request.UserName);
        if (user == null) throw new ApplicationException($"Tài khoản này không có trong dữ liệu.");

        var identityResult = await _userManager.RemoveFromRolesAsync(user, await _userManager.GetRolesAsync(user));
        if (!identityResult.Succeeded) throw new ApplicationException($"Xóa các quyền trước khi khởi tạo quyền mới không thành công.");

        identityResult = await _userManager.AddToRoleAsync(user, ApplicationRoleConstant.Administrator);
        if (!identityResult.Succeeded) throw new ApplicationException($"Khởi tạo quyền mới không thành công.");


        if (request.LastModifiedApplicationUserId != null)
        {
            user.LastModifiedApplicationUserId = request.LastModifiedApplicationUserId.Value;
        }
        user.LastModifiedDate = DateTime.Now;
        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            return Result<string>.Success(string.Empty);
        }
        throw new ApplicationException("Lưu không thành công.");
    }

    public async Task<Result<List<ApplicationUserDto>>> UpdateEmployeeAsync(List<ApplicationUserDto> request, Guid? ModifiedUser = null)
    {
        foreach (var item in request)
        {
            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(s => s.Id == item.Id);
            if (user == null) throw new ApplicationException($"Không tìm thấy nhân sự với Id: {item.Id}");
            user.FullName = item.FullName;
            user.StartDate = item.StartDate;
            user.EndDate = item.EndDate;
            user.Email = item.Email;
            user.PhoneNumber = item.Phone;
            user.Review = item.Review;
            user.Notes = item.Notes;
            user.LastModifiedDate = DateTime.Now;
            user.LastModifiedApplicationUserId = ModifiedUser ?? user.LastModifiedApplicationUserId;
            _context.ApplicationUsers.Update(user);
        }
        await _context.SaveChangesAsync();
        return Result<List<ApplicationUserDto>>.Success(request);
    }

    public async Task<List<Guid>> GetUserIdByRolePosition(string position)
    {
        var lstRole = await _roleManager.Roles.Where(r => r.RolePositionId == position).ToListAsync();

        var userIds = lstRole.SelectMany(r => _userManager.GetUsersInRoleAsync(r.Name ?? "").Result)
                             .Select(u => u.Id)
                             .Distinct()
                             .ToList();

        return userIds;
    }

    public async Task<Result<List<ApplicationUserDto>>> UpdateReviewEmployeeAsync(List<AddOrUpdateRequest> request, Guid? ModifiedUser = null)
    {
        List<ApplicationUserDto> updated = new List<ApplicationUserDto>();
        foreach (var item in request)
        {
            if (item.Data == null || item.Id == null)
                throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ");
            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(s => s.Id == item.Id);
            if (user == null) throw new ApplicationException($"Không tìm thấy nhân sự với Id: {item.Id}");
            var obj = (EmployeeUpdateDto)_internalService.MapValueToObject(new EmployeeUpdateDto(), item.Data, new EmployeeUpdateDto());
            user.EndDate = obj.EndDate;
            user.Review = obj.Review;
            user.Notes = obj.Notes;

            user.LastModifiedDate = DateTime.Now;
            user.LastModifiedApplicationUserId = ModifiedUser ?? user.LastModifiedApplicationUserId;
            _context.ApplicationUsers.Update(user);
            await _context.SaveChangesAsync();

            var result = await GetById((Guid)item.Id);
            if (result.Data != null) updated.Add(result.Data);
        }


        return Result<List<ApplicationUserDto>>.Success(updated);
    }

    public async Task<Result<string>> GetUserIdByCode(string code)
    {
        string userId = await _context.ApplicationUsers.Where(app => app.Code == code).Select(app => app.Code).FirstOrDefaultAsync();
        return Result<string>.Success(userId);
    }

    public async Task<Result<Guid>> GetUserIdByEmail(string email)
    {
        Guid userId = await _context.ApplicationUsers.Where(app => app.Email == email).Select(app => app.Id).FirstOrDefaultAsync();
        return Result<Guid>.Success(userId);
    }

    public async Task<Result<ApplicationUser>> GetUserByEmail(string email)
    {
        var user = await _context.ApplicationUsers.Where(x => x.Email == email && x.DeleteFlag != true)
                                                  .FirstOrDefaultAsync();

        if (user == null)
        {
            throw new ApplicationException("Không tìm thấy user với email");
        }

        return Result<ApplicationUser>.Success(user);
    }

    public async Task<Result<List<string>>> GetConnectStrByEmail(string email)
    {
        var connectStr = await (from user in _tenantContext.Users
                                join userTenant in _tenantContext.UserTenants on user.UserName equals userTenant.UserName
                                join tenant in _tenantContext.Tenants on userTenant.TenantId equals tenant.Id
                                where user.Email == email && user.DeleteFlag != true
                                select new
                                {
                                    connectStr = tenant.ConnectionString
                                }).ToListAsync();

        List<string> result = new List<string>();
        foreach (var user in connectStr)
        {
            result.Add(user.connectStr);
        }

        return Result<List<string>>.Success(result);
    }

    public async Task<Result<string>> GetTenantIdByConnStr(string connStr)
    {
        var tenantId = await _tenantContext.Tenants.Where(t => t.ConnectionString == connStr).FirstOrDefaultAsync();

        return Result<string>.Success(tenantId.Id);
    }

    public async Task<Result<List<ApplicationUserDto>>> AddUserIntoTenant(List<AddOrUpdateApplicationUserRequest> request, string sConnect = "")
    {
        ApplicationUser? obj = null;
        List<ApplicationUserDto> updatedSuccess = new List<ApplicationUserDto>();
        if (sConnect != "")
        {
            _context.CurrentTenantConnectionString = sConnect;
            _context.Database.SetConnectionString(sConnect);
        };
        foreach (AddOrUpdateApplicationUserRequest addOrUpdateRequest in request)
        {
            obj = await _userManager.FindByNameAsync(addOrUpdateRequest.UserName);
            if (obj != null) return Result<List<ApplicationUserDto>>.Failure($"{addOrUpdateRequest.UserName} đã có trong dữ liệu.");

            if (string.IsNullOrEmpty(addOrUpdateRequest.Code))
            {
                addOrUpdateRequest.Code = StringHelper.GenerateCode();
            }

            obj = new ApplicationUser()
            {
                Id = addOrUpdateRequest.Id ?? new Guid(),
                Code = addOrUpdateRequest.Code,
                Address = addOrUpdateRequest.Address,
                UserName = addOrUpdateRequest.UserName,
                Email = addOrUpdateRequest.Email ?? "",
                NormalizedEmail = addOrUpdateRequest.UserName.ToUpper(),
                FirstName = addOrUpdateRequest.FirstName,
                LastName = addOrUpdateRequest.LastName,
                FullName = StringHelper.GetFullName(addOrUpdateRequest.FirstName, addOrUpdateRequest.LastName),
                StartDate = addOrUpdateRequest.StartDate,
                Avatar = addOrUpdateRequest.Avatar ?? "",
                PhoneNumber = addOrUpdateRequest.Phone ?? "",
                DateOfBirth = addOrUpdateRequest.DateOfBirth,
                CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId ?? Guid.Empty
            };

            if (addOrUpdateRequest.Data != null)
            {
                obj = (ApplicationUser)_internalService.MapValueToObject(new ApplicationUser(), addOrUpdateRequest.Data, obj);
            }

            obj.LastModifiedDate = DateTime.Now;
            obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId ?? Guid.Empty;

            var exist = await ValidateAttribute(obj.UserName, obj.Email, obj.PhoneNumber);
            if (!string.IsNullOrEmpty(exist)) Result<List<ApplicationUserDto>>.Failure(exist);

            var status = await _context.ApplicationUserStatuses.FirstOrDefaultAsync(s => s.Code == "ACTIVE");
            if (status != null)
            {
                obj.ApplicationUserStatusId = status.Id;
                obj.ApplicationUserStatus = status;
            }
            obj.FullName = (obj.LastName ?? "") + " " + (obj.FirstName ?? "");
            var result = await _userManager.CreateAsync(obj, addOrUpdateRequest.Password);
            if (!result.Succeeded) return Result<List<ApplicationUserDto>>.Failure($"Tạo tài khoản {addOrUpdateRequest.UserName} không thành công.");

            updatedSuccess.Add(_mapper.Map<ApplicationUserDto>(obj));
        }

        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        return Result<List<ApplicationUserDto>>.Success(updatedSuccess);
    }

    public async Task<Result<ApplicationUserDto>> AddUserWithManyRole(ApplicationUserDto user, List<Guid>? roles)
    {
        if (string.IsNullOrEmpty(user.Code)) user.Code = StringHelper.GenerateCode();
        if (user.Id == null || user.Id == Guid.Empty) user.Id = Guid.NewGuid();

        IdentityResult result;

        var m_user = await _context.ApplicationUsers
                                      .Where(s => s.Id == user.Id)
                                      .FirstOrDefaultAsync();
        var status = await _context.ApplicationUserStatuses.FirstOrDefaultAsync(s => s.Code == "ACTIVE");

        if (m_user != null)
        {
            m_user.Code = user.Code;
            m_user.FirstName = user.FirstName;
            m_user.LastName = user.LastName;
            m_user.FullName = user.FullName;
            m_user.Address = user.Address;
            m_user.Email = user.Email;
            m_user.PhoneNumber = user.Phone;
            m_user.DateOfBirth = user.DateOfBirth;
            m_user.UserName = user.UserName;
            m_user.CreatedApplicationUserId = user.CreatedApplicationUserId;
            m_user.LastModifiedApplicationUserId = user.LastModifiedApplicationUserId;
            m_user.StartDate = null;
            m_user.EndDate = null;
            m_user.Notes = null;
            m_user.Review = null;
            m_user.DeleteFlag = false;
            m_user.ApplicationUserStatus = status;
            m_user.StartDate = DateTime.Now;
            result = await _userManager.UpdateAsync(m_user);
        }
        else
        {
            m_user = new ApplicationUser
            {
                Id = (Guid)user.Id,
                Code = user.Code,
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                FullName = user.FullName ?? "",
                Address = user.Address ?? "",
                Email = user.Email ?? "",
                PhoneNumber = user.Phone ?? "",
                DateOfBirth = user.DateOfBirth,
                UserName = user.UserName,
                CreatedApplicationUserId = user.CreatedApplicationUserId,
                LastModifiedApplicationUserId = user.LastModifiedApplicationUserId,
                ApplicationUserStatus = status,
                StartDate = DateTime.Now
            };

            var existUser = await _userManager.Users.Where(u => u.UserName == user.UserName && u.DeleteFlag == true).FirstOrDefaultAsync();

            if (existUser != null)
            {
                existUser.UserName = $"{user.UserName}_deleted";
                existUser.Email = $"{user.Email}_deleted";
                await _userManager.UpdateAsync(existUser);
            }

            result = await _userManager.CreateAsync(m_user, "123456");
        }

        if (!result.Succeeded) throw new ApplicationException($"Tạo tài khoản không thành công.");

        if (user.IsAdmin == true)
        {
            var role = await _context.ApplicationRoles.Where(s => s.Name == "Admin").FirstOrDefaultAsync();
            var identityResult = await _userManager.AddToRoleAsync(m_user, role!.Name!);
        }

        if (roles != null && roles.Any())
        {
            var m_roles = await _context.ApplicationRoles.Where(s => roles.Contains(s.Id) && !s.DeleteFlag).ToListAsync();
            foreach (var role in m_roles)
            {
                var identityResult = await _userManager.AddToRoleAsync(m_user, role.Name);
                if (!identityResult.Succeeded) throw new ApplicationException($"Khởi tạo quyền mới không thành công.");
            }
        }

        await _context.SaveChangesAsync();
        var response = await GetById(m_user.Id);
        return Result<ApplicationUserDto>.Success(response.Data);

    }

    public async Task<Result<PaginatedList<ApplicationUserDto>>> GetUserInTenantPaginationQuery(GetListApplicationUserWithPaginationQueryRequest request)
    {
        var query = from u in _context.ApplicationUsers
                    join aus in _context.ApplicationUserStatuses on u.ApplicationUserStatusId equals aus.Id into u_aus
                    from aus in u_aus.DefaultIfEmpty()
                    where u.DeleteFlag != true
                    select new ApplicationUserDto()
                    {
                        Id = u.Id,
                        Avatar = u.Avatar ?? string.Empty,
                        LastName = u.LastName ?? string.Empty,
                        FirstName = u.FirstName ?? string.Empty,
                        Email = u.Email ?? string.Empty,
                        Phone = u.PhoneNumber ?? string.Empty,
                        UserName = u.UserName ?? string.Empty,
                        FullName = u.FullName ?? string.Empty,
                        Address = u.Address ?? string.Empty,
                        Code = u.Code,
                        DateOfBirth = u.DateOfBirth,
                        StartDate = u.StartDate,
                        EndDate = u.EndDate,
                        Review = u.Review ?? string.Empty,
                        Notes = u.Notes ?? string.Empty,
                        ApplicationUserStatus = aus != null ? new ApplicationUserStatusDto
                        {
                            Id = aus.Id,
                            Code = aus.Code ?? "",
                            Name = aus.Name ?? ""
                        } : null
                    };



        if (!string.IsNullOrEmpty(request.TextSearch))
        {
            query = query.Where(x => x.UserName.Contains(request.TextSearch)
                                 || (x.FirstName + " " + x.LastName).Contains(request.TextSearch)
                                 || x.Email.Contains(request.TextSearch));
        }
        if (!string.IsNullOrEmpty(request.FullName))
            query = query.Where(x => x.FullName.Contains(request.FullName));
        if (!string.IsNullOrEmpty(request.UserName))
            query = query.Where(x => x.UserName.Contains(request.UserName));
        if (!string.IsNullOrEmpty(request.FirstName))
            query = query.Where(x => x.FirstName.Contains(request.FirstName));
        if (!string.IsNullOrEmpty(request.LastName))
            query = query.Where(x => x.LastName.Contains(request.LastName));
        if (!string.IsNullOrEmpty(request.Email))
            query = query.Where(x => x.Email.Contains(request.Email));
        if (!string.IsNullOrEmpty(request.Address))
            query = query.Where(x => x.Address.Contains(request.Address));
        if (!string.IsNullOrEmpty(request.Phone))
            query = query.Where(x => x.Phone.Contains(request.Phone));
        if (request.UserId != null)
        {
            var position = await _permissionService.GetPositionByUser((Guid)request.UserId);
            if (position != "ADMINISTRATOR")
            {
                /*var access = await _permissionService.HasPermission(MenuType.NS_TTTC, FeatureType.VIEW, (Guid)request.UserId);
				if (access == false)
				{
					query = query.Where(s => s.Id == request.UserId);
				}*/
            }
        }

        //3. Paging
        int totalRecords = await query.CountAsync();
        if (!string.IsNullOrEmpty(request.OrderCol))
        {
            if (string.IsNullOrEmpty(request.OrderDir)) request.OrderDir = "ASC";
            query = FunctionUtilServices.OrderByDynamic(query, request.OrderCol, request.OrderDir.ToUpper() == "DESC" ? false : true);
        }
        query = query.Skip((request.PageIndex - 1) * request.PageSize)
                            .Take(request.PageSize);

        var data = await query.AsNoTracking().ToListAsync();

        List<ApplicationRoleDto> applicationRoles = await (from ur in _context.UserRoles
                                                           join ar in _context.ApplicationRoles on ur.RoleId equals ar.Id
                                                           where data.Select(m => m.Id).ToList().Contains(ur.UserId)
                                                           select new ApplicationRoleDto()
                                                           {
                                                               Id = ar.Id,
                                                               DisplayName = ar.DisplayName ?? string.Empty,
                                                               ApplicationUserId = ur.UserId,
                                                               RolePositionId = ar.RolePositionId ?? string.Empty
                                                           }).AsNoTracking().ToListAsync();
        foreach (var user in data)
        {
            user.ApplicationRoles = applicationRoles.Where(m => m.ApplicationUserId == user.Id).ToList();
        }

        return Result<PaginatedList<ApplicationUserDto>>.Success(new PaginatedList<ApplicationUserDto>(data, totalRecords, request.PageIndex, request.PageSize));
    }

    public async Task<Result<ApplicationUserDto>> GetProfile(Guid id)
    {
        var con = _context.GetConnectString();
        var user = await (from u in _tenantContext.Users
                          join aus in _tenantContext.UserStatus on u.ApplicationUserStatusId equals aus.Id into u_aus
                          from aus in u_aus.DefaultIfEmpty()
                          join gt in _tenantContext.GroupTenants on u.GroupTenantId equals gt.Id into u_gt
                          from gt in u_gt.DefaultIfEmpty()
                          where u.Id == id && u.DeleteFlag != true && gt.DeleteFlag != true
                          select new ApplicationUserDto()
                          {
                              Id = u.Id,
                              Avatar = u.Avatar,
                              LastName = u.LastName ?? string.Empty,
                              FirstName = u.FirstName ?? string.Empty,
                              Email = u.Email ?? string.Empty,
                              Phone = u.Phone ?? string.Empty,
                              UserName = u.UserName ?? string.Empty,
                              FullName = u.FullName ?? string.Empty,
                              Code = u.Code,
                              DateOfBirth = u.DateOfBirth,
                              Review = string.Empty,
                              Notes = string.Empty,
                              IsAdmin = u.IsAdmin,
                              GroupTenantId = u.GroupTenantId,
                              Color1 = gt.Color1 ?? string.Empty,
                              Color2 = gt.Color2 ?? string.Empty,
                              ApplicationUserStatus = aus != null ? new ApplicationUserStatusDto
                              {
                                  Id = aus.Id,
                                  Code = aus.Code ?? "",
                                  Name = aus.Name ?? ""
                              } : null
                          }
                        ).AsNoTracking().FirstOrDefaultAsync();

        if (user != null)
        {
            user.ApplicationRoles = await (from ur in _context.UserRoles
                                           join r in _context.Roles on ur.RoleId equals r.Id
                                           where ur.UserId == id
                                           select new ApplicationRoleDto()
                                           {
                                               Id = r.Id,
                                               Name = r.Name ?? string.Empty,
                                               DisplayName = r.DisplayName ?? string.Empty,
                                               Description = r.Description ?? string.Empty,
                                               RolePositionId = r.RolePositionId ?? string.Empty
                                           }).AsNoTracking().ToListAsync();
        }

        return Result<ApplicationUserDto>.Success(user);
    }

    public async Task<List<Guid>> GetUserIdByRolePositionAndPermission(string position, string menu, string feature)
    {
        var roles = await (from detail in _context.ApplicationRoleDetails
                           join m_menu in _context.Menus on detail.MenuId equals m_menu.Id
                           join m_feature in _context.Features on detail.FeatureId equals m_feature.Id
                           where detail.DeleteFlag != true && m_menu.Code == menu && m_feature.Id == feature
                           select detail.ApplicationRoleId).Distinct().ToListAsync();

        var lstRole = await _roleManager.Roles.Where(r => r.RolePositionId == position && roles.Contains(r.Id)).ToListAsync();

        var listRoleId = lstRole.Select(x => x.Id).ToList();
        var query = await _context.UserRoles.Where(x => listRoleId.Contains(x.RoleId)).ToListAsync();

        var userIds = lstRole.SelectMany(r => _userManager.GetUsersInRoleAsync(r.Name ?? "").Result)
                             .Select(u =>  u.Id)
                             .Distinct()
                             .ToList();

        return userIds;

    }

    public async Task<List<UserRoleDto>> GetUserRoleByRolePositionAndPermission(string position, string menu, string feature)
    {
        var roles = await (from detail in _context.ApplicationRoleDetails
                           join m_menu in _context.Menus on detail.MenuId equals m_menu.Id
                           join m_feature in _context.Features on detail.FeatureId equals m_feature.Id
                           where detail.DeleteFlag != true && m_menu.Code == menu && m_feature.Id == feature
                           select detail.ApplicationRoleId).Distinct().ToListAsync();

        var lstRole = await _roleManager.Roles.Where(r => r.RolePositionId == position && roles.Contains(r.Id)).ToListAsync();
        var listRoleId = lstRole.Select(x => x.Id).ToList();
        var result = await _context.UserRoles.Where(x => listRoleId.Contains(x.RoleId)).Select(x=> new UserRoleDto() { RoleId=x.RoleId, UserId=x.UserId }).ToListAsync();
        return result;

    }

    public async Task<ApplicationUser> FindAsync(Guid? id)
    {
        if (id == null || id == Guid.Empty) throw new ApplicationException($"Không tìm thấy dữ liệu");
        var user = await _context.ApplicationUsers.FirstOrDefaultAsync(s => s.Id == id && s.DeleteFlag != true);
        if (user == null) throw new ApplicationException($"Không tìm thấy người dùng với Id: {id}");
        return user;
    }

    public async Task<bool> CheckIsAdmin(Guid id)
    {
        var isAdmin = (await _tenantContext.Users.Where(s => s.Id == id).Select(s => s.IsAdmin).FirstOrDefaultAsync()) ?? false;
        return isAdmin;
    }

    public async Task<Result<ApplicationUserDto>> GetProfileAdmin(string email)
    {
        var user = await (from u in _tenantContext.UserAdmins
                          where u.Email == email
                          select new ApplicationUserDto()
                          {
                              Avatar = string.Empty,
                              LastName = string.Empty,
                              FirstName = string.Empty,
                              Email = u.Email ?? string.Empty,
                              Phone = string.Empty,
                              UserName = u.UserName ?? string.Empty,
                              FullName = u.FullName ?? string.Empty,
                              Code = string.Empty,
                              DateOfBirth = DateTime.MinValue,
                              Review = string.Empty,
                              Notes = string.Empty,
                              IsAdmin = false,
                              ApplicationUserStatus = null
                          }
                        ).AsNoTracking().FirstOrDefaultAsync();

        if (user != null)
        {
            List<ApplicationRoleDto> listApplicationRoleDto = new List<ApplicationRoleDto>
            {
                new ApplicationRoleDto
                {
                    Id = Guid.Empty,
                    Name = string.Empty,
                    DisplayName = string.Empty,
                    Description = string.Empty,
                    RolePositionId = "ADMIN SAAS"
                }
            };

            user.ApplicationRoles = listApplicationRoleDto;
        }

        return Result<ApplicationUserDto>.Success(user);
    }

    public async Task<List<ApplicationRoleDto>> GetApplicationRolesByUserIdAsync(Guid userId)
    {
        var user = await _context.ApplicationUsers.Where(x => x.DeleteFlag != true && x.Id == userId)
                                                  .FirstOrDefaultAsync();

        if (user == null)
        {
            throw new ApplicationException("Không tìm thấy user");
        }

        var query = from ur in _context.UserRoles
                    join r in _context.Roles on ur.RoleId equals r.Id into ur_r
                    from r in ur_r.DefaultIfEmpty()
                    where ur.UserId == userId && (r != null && r.DeleteFlag != true)
                    select new { ur, r };

        var result = await query
            .Select(x => new ApplicationRoleDto()
            {
                Id = x.r.Id,
                Name = x.r.Name ?? string.Empty,
                DisplayName = x.r.DisplayName ?? string.Empty,
                RolePositionId = x.r.RolePositionId ?? string.Empty,
                ApplicationUserId = x.ur.UserId
            })
            .ToListAsync();

        return result;
    }

    public async Task<string> GetAvatar(Guid userId)
    {
        var con = _context.GetConnectString();
        var user = await _tenantContext.Users.Where(x => x.Id == userId)
                                             .FirstOrDefaultAsync();

        if (user == null)
        {
            throw new ApplicationException("Không tìm thấy user");
        }

        var avatar = user.Avatar;

        return avatar;
    }

    public async Task<Result<bool>> Logout()
    {
        await _signInManager.SignOutAsync();

        return Result<bool>.Success(true);
    }
}