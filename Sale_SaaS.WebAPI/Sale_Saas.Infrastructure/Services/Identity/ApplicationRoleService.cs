using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Org.BouncyCastle.Crypto;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.Identity;
using Sale_Saas.Application.Utilities;
using Sale_Saas.Domain.Entities;
using Sale_Saas.Domain.Entities.Tenant;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Infrastructure.Services.Identity;

public class ApplicationRoleService : IApplicationRoleService
{
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IInternalService _internalService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IMapper _mapper;
    public ApplicationRoleService(ApplicationDbContext context,
                                    RoleManager<ApplicationRole> roleManager,
                                    UserManager<ApplicationUser> userManager,
                                    IInternalService internalService,
                                    IMapper mapper)
    {
        _context = context;
        _roleManager = roleManager;
        _internalService = internalService;
        _userManager = userManager;
        _mapper = mapper;
        //_applicationUserService = applicationUserService;
    }
    public async Task<Result<List<ApplicationRoleDto>>> GetAllQuery(GetAllQueryRequest request, Guid userId)
    {
        var query = from r in _context.ApplicationRoles
                    where r.DeleteFlag != true && r.IsModified != false
                    select new { r };

        if(userId != default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new ApplicationException("Không có dữ liệu người dùng");
            }
            var names = (List<string>)await _userManager.GetRolesAsync(user);
            var roles = await _context.ApplicationRoles.Where(s => s.Name != null && names.Contains(s.Name) && !s.DeleteFlag)
                                                       .Select(s => s.Id).ToListAsync();

            query = from r in _context.ApplicationRoles
                    where r.DeleteFlag != true && r.IsModified != false && !roles.Contains(r.Id)
                    select new { r };
        }

        if (!string.IsNullOrEmpty(request.TextSearch))
        {
            query = query.Where(x => (!string.IsNullOrEmpty(x.r.Name) &&
                                       x.r.Name.Contains(request.TextSearch)) ||
                                       (!string.IsNullOrEmpty(x.r.DisplayName) &&
                                       x.r.DisplayName.Contains(request.TextSearch)));
        }

        return Result<List<ApplicationRoleDto>>.Success(await query.Select(x => new ApplicationRoleDto()
        {
            Id = x.r.Id,
            Name = x.r.Name ?? string.Empty,
            DisplayName = x.r.DisplayName ?? string.Empty,
            Description = x.r.Description ?? string.Empty,
            RolePositionId = x.r.RolePositionId ?? ""
        }).AsNoTracking().ToListAsync());
    }

    public async Task<Result<List<ApplicationRoleDto>>> AddOrUpdateAsync(List<AddOrUpdateRequest> request)
    {
        ApplicationRole? obj = null;
        List<ApplicationRoleDto> updatedSuccess = new List<ApplicationRoleDto>();

        foreach (AddOrUpdateRequest addOrUpdateRequest in request)
        {
            ApplicationRole tmp = new ApplicationRole();
            if (addOrUpdateRequest.Id == null)
            {
                obj = new ApplicationRole()
                {
                    CreatedApplicationUserId = addOrUpdateRequest.CreatedApplicationUserId
                };
            }
            else
            {
                obj = await _roleManager.FindByIdAsync(addOrUpdateRequest.Id.Value.ToString());

                if (obj == null)
                    throw new ApplicationException($"Không tìm thấy AppRole có id: {addOrUpdateRequest.Id.Value}");
                PropertiesExtension.Copy(obj, tmp);
            }

            if (addOrUpdateRequest.Data != null)
            {
                obj = (ApplicationRole)_internalService.MapValueToObject(new ApplicationRole(), addOrUpdateRequest.Data, obj);
            }

            obj.LastModifiedDate = DateTime.Now;
            obj.LastModifiedApplicationUserId = addOrUpdateRequest.LastModifiedApplicationUserId;
            if (obj.DisplayName == null) throw new ApplicationException("Vui lỏng nhập tên hiển thị");
            if (obj.Name == null) obj.Name = obj.DisplayName;
            if (obj.DisplayName.Length > 250) throw new ApplicationException("Chỉ có thể đặt tên hiển thị 250 kí tự");
            obj.Name = StringHelper.BoDauVaKhoangTrang(obj.Name);

            if (obj.RolePositionId != null)
            {
                var duplicate = await _context.ApplicationRoles.Where(s => s.Name == obj.Name && s.Id != obj.Id).CountAsync();
                if (duplicate > 0)
                {
                    throw new ApplicationException("Tên chức vụ đã được sử dụng");
                }
                var position = await _context.RolePositions.FirstOrDefaultAsync(s => s.Id == obj.RolePositionId);
                if (position == null) throw new ApplicationException("Chức vụ không hợp lệ");
                obj.RolePositionId = position.Id;
                obj.RolePosition = position;
            }

            if (addOrUpdateRequest.Id == null)
            {
                var duplicate = await _context.ApplicationRoles.Where(s => s.Name == obj.Name).CountAsync();
                if (duplicate > 0)
                {
                    throw new ApplicationException("Tên chức vụ đã được sử dụng");
                }
                obj.IsModified = true;
                var result = await _roleManager.CreateAsync(obj);
                if (!result.Succeeded)
                {
                    throw new ApplicationException("Tạo Role không thành công. " + string.Join(". ", result.Errors.Select(m => m.Description).ToList()));
                }
            }
            else
            {
                obj.IsModified = tmp.IsModified;
                var result = await _roleManager.UpdateAsync(obj);
                if (!result.Succeeded)
                {
                    throw new ApplicationException("Cập nhật Role không thành công. " + string.Join(". ", result.Errors.Select(m => m.Description).ToList()));
                }
            }

            updatedSuccess.Add(_mapper.Map<ApplicationRoleDto>(obj));
        }

        await _context.SaveChangesAsync();
        return Result<List<ApplicationRoleDto>>.Success(updatedSuccess);
    }
    public async Task<Result<string>> DeleteByIds(DeleteRequest request)
    {
        if (request.Ids == null) throw new ApplicationException("Không tìm thấy tham số Id.");
        List<Guid> ids = (request.Ids.Select(m => Guid.Parse(m)).ToList());
        var applicationRoles = await _context.ApplicationRoles.Where(m => ids.Contains(m.Id) && m.IsModified != false).ToListAsync();
        if (applicationRoles == null) throw new ApplicationException($"Không tìm thấy trong dữ liệu có Id: {string.Join(";", request.Ids)}");

        var userRoles = await _context.UserRoles.Where(us => ids.Contains(us.RoleId)).ToListAsync();
        if (userRoles.Any()) throw new ApplicationException("Chức vụ này đã được gán cho tài khoản user");

        var salekits = await _context.ApplicationRole_SaleKits.Where(s => s.ApplicationRoleId != null && ids.Contains((Guid)s.ApplicationRoleId)).ToListAsync();
        if (salekits.Any()) throw new ApplicationException("Chức vụ này đã được gán trong phân quyền sale kit");

        var employeeSalaries = await _context.EmployeeSalarys.Where(es => es.RoleId != null && ids.Contains((Guid)es.RoleId)).ToListAsync();
        if (employeeSalaries.Any()) throw new ApplicationException("Chức vụ này đã có trong thông tin thu nhập");

        var benefits = await _context.Benefits.Where(b => b.ApplicationRoleId != null && ids.Contains((Guid)b.ApplicationRoleId)).ToListAsync();
        if (benefits.Any()) throw new ApplicationException("Chức vụ này đã được gán trong quyền lợi");

        var goals = await _context.Goals.Where(g => g.ApplicationRoleId != null && ids.Contains((Guid)g.ApplicationRoleId)).ToListAsync();
        if (goals.Any()) throw new ApplicationException("Chức vụ này đã được gán trong mục tiêu");

        var relationships = await _context.Relationships.Where(r => r.ApplicationRoleId != null && ids.Contains((Guid)r.ApplicationRoleId)).ToListAsync();
        if (relationships.Any()) throw new ApplicationException("Chức vụ này đã được gán trong mối quan hệ");

        var opportunities = await _context.Opportunities.Where(s => s.ApplicationRoleId != null && ids.Contains((Guid)s.ApplicationRoleId)).ToListAsync();
        if (opportunities.Any()) throw new ApplicationException("Chức vụ này đã được gán trong cơ hội");

        var queryApplicationRoleDetails = _context.ApplicationRoleDetails.Where(m => m.ApplicationRoleId != null && ids.Contains(m.ApplicationRoleId.Value)).ToList();
        if (queryApplicationRoleDetails.Any())
            _context.ApplicationRoleDetails.RemoveRange(queryApplicationRoleDetails);
        _context.ApplicationRoles.RemoveRange(applicationRoles);

        await _context.SaveChangesAsync();

        return Result<string>.Success(string.Empty);
    }
    public async Task<Result<PaginatedList<ApplicationRoleDto>>> GetListWithPaginationQuery(GetListRoleWithPaginationQueryRequest request)
    {
        var query = (from x in _context.ApplicationRoles
                     where x.DeleteFlag != true && x.IsModified != false
                     select new ApplicationRoleDto
                     {
                         Id = x.Id,
                         Name = x.Name ?? string.Empty,
                         DisplayName = x.DisplayName ?? string.Empty,
                         Description = x.Description ?? string.Empty,
                         RolePositionId = x.RolePositionId ?? ""
                     }).AsNoTracking();

        if (!string.IsNullOrEmpty(request.TextSearch))
        {
            query = query.Where(x => x.Name.Contains(request.TextSearch) ||
                                     x.DisplayName.Contains(request.TextSearch) ||
                                     x.Description.Contains(request.TextSearch));
        }

        if (!string.IsNullOrEmpty(request.RoleName))
        {
            query = query.Where(x => x.DisplayName.Contains(request.RoleName.Trim()));
        }

        int totalRecords = await query.CountAsync();

        if (!string.IsNullOrEmpty(request.OrderCol))
        {
            if (string.IsNullOrEmpty(request.OrderDir)) request.OrderDir = "ASC";
            query = FunctionUtilServices.OrderByDynamic(query, request.OrderCol, request.OrderDir.ToUpper() == "DESC" ? false : true);
        }

        query = query.Skip((request.PageIndex - 1) * request.PageSize)
                     .Take(request.PageSize);

        var data = await query.ToListAsync();

        return Result<PaginatedList<ApplicationRoleDto>>.Success(new PaginatedList<ApplicationRoleDto>(data, totalRecords, request.PageIndex, request.PageSize));
    }
    public async Task<Result<ApplicationRoleDto>> GetById(Guid id)
    {
        return Result<ApplicationRoleDto>.Success(await (from ar in _context.ApplicationRoles
                                                         where ar.DeleteFlag != true && ar.Id == id
                                                         select new ApplicationRoleDto()
                                                         {
                                                             Id = ar.Id,
                                                             Name = ar.Name ?? string.Empty,
                                                             DisplayName = ar.DisplayName ?? string.Empty,
                                                             Description = ar.Description ?? string.Empty,
                                                             RolePositionId = ar.RolePositionId ?? ""
                                                         })
                                                          .AsNoTracking()
                                                          .FirstOrDefaultAsync());
    }
    public async Task<Result<List<ApplicationRoleDto>>> FilterQuery(FilterQueryRequest request)
    {
        var query = from ar in _context.ApplicationRoles
                    where ar.IsModified != false
                    select new { ar };

        if (!string.IsNullOrEmpty(request.TextSearch))
        {
            query = query.Where(x => !string.IsNullOrEmpty(x.ar.Name)
                                    && x.ar.Name.Contains(request.TextSearch));
        }

        if (!string.IsNullOrEmpty(request.Name))
        {
            query = query.Where(x => !string.IsNullOrEmpty(x.ar.Name)
                                        && x.ar.Name.Contains(request.Name));
        }

        if (request.TotalRecord != null)
        {
            query = query.Take(request.TotalRecord.Value);
        }

        var data = await query.Select(x => new ApplicationRoleDto()
        {
            Id = x.ar.Id,
            Name = x.ar.Name ?? string.Empty
        }).AsNoTracking().ToListAsync();

        return Result<List<ApplicationRoleDto>>.Success(data);
    }

    /*public async Task<List<string>> GetListRoleByUserId(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return new List<string>();
        }
        return (List<string>)await _userManager.GetRolesAsync(user);
    }*/


    public async Task<string> GetRoleById(Guid id)
    {
        var role = await _context.ApplicationRoles.FirstOrDefaultAsync(x => x.Id == id && x.DeleteFlag != true);

        if (role == null)
        {
            throw new ApplicationException("Không tìm thấy role");
        }

        return role.RolePositionId ?? "";
    }

    public async Task<List<ApplicationRole>> GetListRoleByUserId(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return new List<ApplicationRole>();
        }
        var names = (List<string>)await _userManager.GetRolesAsync(user);
        var roles = await _context.ApplicationRoles
                                  .Where(s => s.Name != null && names.Contains(s.Name) && !s.DeleteFlag)
                                  .ToListAsync();
        return roles;
    }

    public async Task<Result<List<RoleDto>>> GetListRoleByUserIdHasResult(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new ApplicationException("Không có dữ liệu người dùng");
        }
        var names = (List<string>)await _userManager.GetRolesAsync(user);
        var roles = await _context.ApplicationRoles
                                  .Where(s => s.Name != null && names.Contains(s.Name) && !s.DeleteFlag)
                                  .Select(s => new RoleDto()
                                  {
                                      Id = s.Id,
                                      DisplayName = s.DisplayName ?? "",
                                      Name = s.Name ?? "",
                                      RolePositionId = s.RolePositionId ?? ""
                                  })
                                  .ToListAsync();
        return Result<List<RoleDto>>.Success(roles);
    }

    public async Task<List<string>> GetListRoleStringByUserId(Guid userId, string connectionString)
    {
        //_applicationUserService.SetConnectDB(connectionString);

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new ApplicationException("Không tìm thấy user");
        }
        var names = (List<string>)await _userManager.GetRolesAsync(user);

        var roles = await _context.ApplicationRoles
                                  .Where(s => s.Name != null && names.Contains(s.Name) && !s.DeleteFlag)
                                  .ToListAsync();

        //_applicationUserService.ClearConnectDB();

        if (roles.Count <= 0 || roles == null)
        {
            return new List<string>();
        }

        return roles.Select(x => x.Name).ToList();
    }

    public async Task<List<string>> GetListRoleStringByUserIdAndRolePositionId(Guid userId, string rolePositionId, string connectionString)
    {
        //_applicationUserService.SetConnectDB(connectionString);

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new ApplicationException("Không tìm thấy user");
        }
        var names = (List<string>)await _userManager.GetRolesAsync(user);

        var roles = await _context.ApplicationRoles
                                  .Where(s => s.Name != null && names.Contains(s.Name) && s.RolePositionId == rolePositionId && !s.DeleteFlag)
                                  .ToListAsync();

        //_applicationUserService.ClearConnectDB();

        if (roles.Count <= 0 || roles == null)
        {
            return new List<string>();
        }

        return roles.Select(x => x.Name).ToList();
    }

    public async Task<Result<Guid>> GetIdByRoleName(string roleName)
    {
        var roleId = await _context.Roles.Where(r => r.Name == roleName).Select(r => r.Id).FirstOrDefaultAsync();
        if (roleId == null)
        {
            return Result<Guid>.Failure("Không tìm thấy tên của vai trò");
        }

        return Result<Guid>.Success(roleId);
    }

    public async Task<Result<ApplicationRole>> GetByRoleName(string roleName)
    {
        var role = await _context.Roles.Where(r => r.Name == roleName).FirstOrDefaultAsync();
        if (role == null)
        {
            return Result<ApplicationRole>.Failure("Không tìm thấy tên của vai trò");
        }

        return Result<ApplicationRole>.Success(role);
    }

    public async Task<string> GetCurrentRoleOfUser(Guid userId)
    {
        var listRolesOfUser = await GetListRoleByUserId(userId);

        var listRolesStringOfUser = listRolesOfUser.Select(x => x.RolePositionId).ToList();

        if (listRolesStringOfUser.Contains(RolePositionEnum.MANAGER.ToString()))
        {
            return RolePositionEnum.MANAGER.ToString();
        }

        if (listRolesStringOfUser.Contains(RolePositionEnum.EMPLOYEE.ToString()))
        {
            return RolePositionEnum.EMPLOYEE.ToString();
        }

        return "";
    }

    public async Task<Result<List<RoleDto>>> GetApplicationRoleByRolePositionId(string rolePositionId)
    {
        var roles = await _context.ApplicationRoles.Where(x => x.DeleteFlag != true && x.RolePositionId == rolePositionId)
                                                   .ToListAsync();

        if (roles != null && roles.Count > 0)
        {
            var result = roles
                            .Select(x => new RoleDto()
                            {
                                Id = x.Id,
                                DisplayName = x.DisplayName ?? "",
                                RolePositionId = x.RolePositionId ?? "",
                                Name = x.Name ?? ""
                            })
                            .ToList();

            return Result<List<RoleDto>>.Success(result);
        }

        return Result<List<RoleDto>>.Success(new List<RoleDto>());
    }

    public async Task<Result<List<RoleDto>>> GetListRoleSaleKitByRolePositionId(string rolePositionId)
    {
        var roles = await _context.ApplicationRoles.Where(x => x.DeleteFlag != true && x.RolePositionId != rolePositionId
                                                                                     && x.RolePositionId != RolePositionEnum.ADMIN.ToString())
                                                   .ToListAsync();

        if (roles != null && roles.Count > 0)
        {
            var result = roles
                            .Select(x => new RoleDto()
                            {
                                Id = x.Id,
                                DisplayName = x.DisplayName ?? "",
                                RolePositionId = x.RolePositionId ?? "",
                                Name = x.Name ?? ""
                            })
                            .ToList();

            return Result<List<RoleDto>>.Success(result);
        }

        return Result<List<RoleDto>>.Success(new List<RoleDto>());
    }

    public async Task<Result<List<RoleDto>>> GetListRoleByUser(Guid userId, string rolePositionId)
    {
        if (userId == Guid.Empty || string.IsNullOrEmpty(rolePositionId))
        {
            throw new ApplicationException("Id không được phép null");
        }

        var listRoleOfUser = await GetListRoleByUserId(userId);

        var result = listRoleOfUser.Where(x => x.RolePositionId == rolePositionId
                                                              && x.DeleteFlag != true)
                                                     .Select(x => new RoleDto()
                                                     {
                                                         Id = x.Id,
                                                         DisplayName = x.DisplayName ?? "",
                                                         Name = x.Name ?? "",
                                                         RolePositionId = x.RolePositionId ?? ""
                                                     })
                                                     .ToList();
        return Result<List<RoleDto>>.Success(result);
    }
}