using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Sale_Saas.Application.Common.Interfaces;
using Sale_Saas.Application.Common.Mappings;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.ApplicationUserStatusFeature.Dto;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.Identity;
using Sale_Saas.Application.Models.Tenant;
using Sale_Saas.Application.Utilities;
using Sale_Saas.Domain.Entities.Tenant;
using Sale_Saas.Domain.Enums;
using Sale_Saas.Infrastructure.Services.TenantService.DTOs;
using Sale_Saas.Infrastructure.Services.TenantService.Utils;
using System.Data;
using System.Globalization;

namespace Sale_Saas.Infrastructure.Services.TenantService
{
    public class UserService : IUserService
    {
        private readonly CancellationToken _cancellationToken = new CancellationToken();
        private readonly IApplicationUserService _userService;
        private readonly TenantDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IInternalService _internalService;
        private readonly IMapper _mapper;
        private readonly IApplicationDbContext _apcontext;
        private readonly IFileStorageService _fileStorageService;
        private readonly IOtpSendService _otpSendService;

        public UserService(
            TenantDbContext context,
            IConfiguration configuration,
            IInternalService internalService,
            IApplicationUserService userService,
            IOtpSendService otpSendService,
            IMapper mapper, IApplicationDbContext apContext, IFileStorageService fileStorageService)
        {
            _context = context;
            _configuration = configuration;
            _internalService = internalService;
            _userService = userService;
            _mapper = mapper;
            _apcontext = apContext;
            _fileStorageService = fileStorageService;
            _otpSendService = otpSendService;
        }

        public Result<List<UserTenant>> CreateUser(CreateUser request, bool isCreateTenant = false)
        {
            Guid id = Guid.NewGuid();
            var checkExisted = _context.Users.Where(x => x.UserName == request.UserName && x.DeleteFlag != true).FirstOrDefault();
            if (checkExisted != null)
            {
                if (isCreateTenant)
                {
                    if (checkExisted.Password != FunctionUtils.GetMd5HashSalt(request.Password))
                        return Result<List<UserTenant>>.Failure("Mật khẩu không đúng.");
                }
                else
                    return Result<List<UserTenant>>.Failure("Tài khoản đã tồn tại.");
            }
            if (string.IsNullOrEmpty(request.Code))
            {
                request.Code = StringHelper.GenerateCode();
            }
            else
            {
                User user = new()
                {
                    Id = id,
                    Code = request.Code,
                    FirstName = request.FirstName,
                    UserName = request.UserName,
                    LastName = request.LastName,
                    FullName = StringHelper.GetFullName(request.FirstName, request.LastName),
                    Email = request.Email,
                    Phone = request.Phone,
                    Address = "",
                    DateOfBirth = DateTime.Now,
                    Password = FunctionUtils.GetMd5HashSalt(request.Password),
                    IsAdmin = request.IsAdmin,
                };
                var newData = _context.Users.Add(user);
            }
            var listUserTenant = new List<UserTenant>();
            foreach (var tenant in request.TenantRoles)
            {
                var item = new UserTenant()
                {
                    UserName = request.UserName,
                    TenantId = tenant.TenantId,
                    ApplicationUserId = id
                };
                listUserTenant.Add(item);
            }
            if (listUserTenant.Any()) _context.UserTenants.AddRange(listUserTenant);
            _context.SaveChanges();
            return Result<List<UserTenant>>.Success(listUserTenant);
        }

        public async Task<Result<UserLoginDto>> LoginUser(string userName, string password, Guid? groupTenantId = null, string deviceId = "")
        {
            GroupTenant? checkPlanService = null;
            User? existed = null;

            if (groupTenantId != null)
            {
                checkPlanService = await _context.GroupTenants.Where(gt => gt.Id == groupTenantId && gt.DeleteFlag != true).AsNoTracking().FirstOrDefaultAsync();
                if (checkPlanService == null)
                    throw new ApplicationException("Công ty không tồn tại. Vui lòng liên hệ quản trị viên");
                if (checkPlanService != null)
                    existed = await _context.Users.Include(s => s.ApplicationUserStatus)
                                  .Where(x => x.Email == userName && x.GroupTenantId == checkPlanService.Id && x.DeleteFlag != true).AsNoTracking().FirstOrDefaultAsync();
            }
            if (existed == null)
            {
                existed = await _context.Users.Include(s => s.ApplicationUserStatus)
                                  .Where(x => x.Email == userName && x.DeleteFlag != true).FirstOrDefaultAsync();
                if (existed != null)
                {
                    checkPlanService = await _context.GroupTenants.Where(gt => gt.Id == existed.GroupTenantId && gt.DeleteFlag != true).AsNoTracking().FirstOrDefaultAsync();
                    if (checkPlanService == null)
                        throw new ApplicationException("Công ty không tồn tại. Vui lòng liên hệ quản trị viên");
                }
            }
            if (existed != null)
            {
                if (checkPlanService == null)
                    checkPlanService = await _context.GroupTenants.Where(gt => gt.Id == existed.GroupTenantId && gt.DeleteFlag != true).AsNoTracking().FirstOrDefaultAsync();
                if (checkPlanService == null)
                    throw new ApplicationException("Công ty không tồn tại. Vui lòng liên hệ quản trị viên");
                if (checkPlanService.ExpiryTime < DateTime.Now)
                {
                    throw new ApplicationException("Gói dịch vụ của công ty đã hết hạn. Vui lòng liên hệ quản trị viên");
                }
                if (existed.ApplicationUserStatus == null || existed.ApplicationUserStatus.Code == UserStatusEnum.UNACTIVE.ToString())
                {
                    throw new ApplicationException("Tài khoản chưa được kích hoạt, vui lòng liên hệ quản trị viên");
                }
                if (FunctionUtils.GetMd5HashSalt(password) == existed.Password)
                {
                    var listTenant = ListUserTenant(existed.Id);
                    if (!string.IsNullOrEmpty(deviceId))
                    {
                        existed.DeviceID = deviceId;
                        _context.SaveChanges();
                    }
                    return Result<UserLoginDto>.Success(new UserLoginDto() { Id = existed.Id, GroupTenantId = existed.GroupTenantId, UserName = userName, IsAdmin = existed.IsAdmin, UserTenantDtos = listTenant });
                }
                else
                    throw new ApplicationException("Mật khẩu không đúng");
            }
            else
                throw new ApplicationException($"Tài khoản không tồn tại.");
        }

        public async Task<User> GetUserByEmail(string email, Guid? groupTenantId = null, string deviceId = "", bool isLogout = false)
        {
            var queryUser = _context.Users.Where(x => x.Email == email && x.DeleteFlag != true);

            if (!string.IsNullOrEmpty(deviceId))
                queryUser = queryUser.Where(x => x.DeviceID == deviceId);
            else if (groupTenantId != null)
            {
                queryUser = queryUser.Where(x => x.GroupTenantId == groupTenantId);
            }

            var user = await queryUser.FirstOrDefaultAsync();

            if (isLogout == true)
            {
                user.DeviceID = "";
                _context.SaveChanges();
            }
            return user;
        }
        public async Task<Result<UserDto>> GetUserById(Guid userId)
        {
            var user = await _context.Users.Where(x => x.Id == userId && x.DeleteFlag != true)
                                           .AsNoTracking()
                                           .FirstOrDefaultAsync();

            if (user == null)
            {
                throw new ApplicationException("User không tồn tại");
            }

            var userDto = new UserDto()
            {
                Id = user.Id,
                UserName = user.UserName,
                Code = user.Code,
                FullName = user.FullName
            };

            return Result<UserDto>.Success(userDto);
        }

        public User GetUserByRefreshToken(string refreshToken)
        {
            var user = _context.Users.Where(x => x.RefreshToken == refreshToken && x.DeleteFlag != true).AsNoTracking().FirstOrDefault();
            return user;
        }
        public User GetUser(Guid id)
        {
            var user = _context.Users.Where(x => x.Id == id && x.DeleteFlag != true).AsNoTracking().FirstOrDefault();
            return user;
        }

        public User GetUserAdminByUserName(string userName)
        {
            var user = _context.Users.Where(x => x.UserName == userName && x.IsAdmin == true && x.DeleteFlag != true).AsNoTracking().FirstOrDefault();
            return user;
        }
        public PaginatedList<User> GetListUser(GetListWithPaginationQueryRequest request)
        {
            var query = _context.Users.Where(x => !string.IsNullOrEmpty(x.UserName) && x.DeleteFlag != true);
            if (!string.IsNullOrEmpty(request.TextSearch))
                query = query.Where(x => x.FirstName == request.TextSearch || x.UserName.Contains(request.TextSearch)
                || x.FirstName.Contains(request.TextSearch) || x.LastName.Contains(request.TextSearch));
            return (query.OrderBy(x => x.FirstName).PaginatedListNoAsync(request.PageIndex, request.PageSize));
        }

        public UserTenant GetUserTenant(string userName, string tenantId)
        {
            var user = _context.UserTenants.Where(x => x.UserName == userName && x.TenantId == tenantId && x.DeleteFlag != true).FirstOrDefault();
            return user;
        }
        public List<UserTenantDto> ListUserTenant(Guid id, bool cnStr = true)
        {
            var users = _context.UserTenants.Where(x => x.ApplicationUserId == id && x.DeleteFlag != true && x.Tenant != null && x.Tenant.DeleteFlag != true)
                .Include(x => x.Tenant).AsNoTracking()
                .OrderBy(s => s.Tenant!.CreatedDate)
                .Select(x => new UserTenantDto()
                {
                    TenantId = x.TenantId,
                    TenantName = x.Tenant!.Name,
                    Logo = x.Tenant.Logo,
                    ApplicationUserId = x.ApplicationUserId,
                    ConnectString = cnStr ? x.Tenant.ConnectionString : ""
                }).ToList();
            if (cnStr == false)
            {
                /*var ids = users.Select(s => s.TenantId).ToList();
				var count = _context.UserTenants.*/
                foreach (var item in users)
                {
                    item.Count = _context.UserTenants.Where(s => s.TenantId == item.TenantId && s.DeleteFlag != true).Count();
                }
            }
            return users;
        }

        public async Task<List<UserTenantMobileDto>> ListUserTenantByMobile(Guid id, bool cnStr = true)
        {
            var users = await _context.UserTenants.Where(x => x.ApplicationUserId == id && x.DeleteFlag != true && x.Tenant != null && x.Tenant.DeleteFlag != true)
                                                  .Include(x => x.Tenant).AsNoTracking()
                                                  .OrderBy(s => s.Tenant!.CreatedDate)
                                                  .Select(x => new UserTenantMobileDto()
                                                  {
                                                      TenantId = x.TenantId,
                                                      TenantName = x.Tenant!.Name,
                                                      Logo = x.Tenant.Logo,
                                                      ApplicationUserId = x.ApplicationUserId,
                                                      ConnectString = x.Tenant.ConnectionString
                                                  })
                                                  .ToListAsync();

            if (cnStr == false)
            {
                /*var ids = users.Select(s => s.TenantId).ToList();
				var count = _context.UserTenants.*/
                foreach (var item in users)
                {
                    item.Count = _context.UserTenants.Where(s => s.TenantId == item.TenantId && s.DeleteFlag != true).Count();
                    //item.Roles = _a
                    _userService.SetConnectDB(item.ConnectString);
                    item.Roles = await _userService.GetApplicationRolesByUserIdAsync(id);
                    _userService.ClearConnectDB();
                }
            }
            return users;
        }


        public List<AllUserTenantDto> ListAllUserTenant(Guid id, int? PageIndex = null, int? PageSize = null)
        {
            var listTenant = _context.UserTenants.Where(x => x.ApplicationUserId == id && x.DeleteFlag != true).AsNoTracking().Select(x => x.TenantId).ToList();
            if (listTenant.Any())
            {
                var userTenantQuery = _context.UserTenants.Where(x => listTenant.Contains(x.TenantId) && x.DeleteFlag != true).OrderByDescending(s => s.CreatedDate).AsNoTracking();

                var distinctUserNames = userTenantQuery
                    .GroupBy(x => x.UserName)
                    .Select(g => g.FirstOrDefault().UserName);

                if (PageIndex != null && PageSize != null)
                {
                    int skip = (PageIndex.Value - 1) * PageSize.Value;
                    int take = PageSize.Value;

                    distinctUserNames = distinctUserNames.Skip(skip).Take(take);
                }

                var userNamesList = distinctUserNames.ToList();

                var ids = userTenantQuery
                    .Where(x => userNamesList.Contains(x.UserName) && x.DeleteFlag != true)
                    .Select(x => x.ApplicationUserId)
                    .ToList();

                var users = _context.UserTenants.Where(x => listTenant.Contains(x.TenantId) && ids.Contains(x.ApplicationUserId) && x.DeleteFlag != true).Include(x => x.Tenant).AsNoTracking().
                    Select(x => new AllUserTenantDto()
                    {
                        UserName = x.UserName,
                        TenantId = x.TenantId,
                        TenantName = x.Tenant.Name,
                        ApplicationUserId = x.ApplicationUserId,
                        ConnectString = x.Tenant.ConnectionString
                    }).ToList();
                return users;
            }
            return null;
        }

        public bool UpdateRefreshTokenUser(Guid? userId, string? refreshToken)
        {
            var user = _context.Users.Where(x => x.Id == userId && x.DeleteFlag != true).FirstOrDefault();
            if (user != null)
            {
                user.RefreshToken = refreshToken;
                _context.SaveChanges();
                return true;
            }
            return false;
        }
        public bool UpdateUserTenant(string userName, string tenantId, Guid userId)
        {
            var user = _context.UserTenants.Where(x => x.UserName == userName && x.TenantId == tenantId && x.DeleteFlag != true).FirstOrDefault();
            if (user != null)
            {
                user.ApplicationUserId = userId;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public async Task<Guid> GetApplicationUserIdByTenant(string username, string tenantId)
        {
            var id = await _context.UserTenants.Where(s => s.UserName == username && s.TenantId == tenantId && s.DeleteFlag != true)
                                   .Select(s => s.ApplicationUserId).FirstOrDefaultAsync();
            return id ?? Guid.Empty;
        }

        public async Task<Result<PaginatedList<ApplicationUserWithTenantDto>>> GetListWithPaginationQuery(GetListApplicationUserWithPaginationQueryRequest request)
        {
            var query = (from user in _context.Users
                         join status in _context.UserStatus on user.ApplicationUserStatusId equals status.Id
                         join userTenant in _context.UserTenants on user.UserName equals userTenant.UserName into userTenants
                         where user.DeleteFlag != true && user.IsAdmin == false
                         orderby user.CreatedDate descending
                         select new ApplicationUserWithTenantDto
                         {
                             Id = user.Id,
                             UserName = user.UserName ?? "",
                             FullName = user.FullName ?? "",
                             Code = user.Code ?? "",
                             Phone = user.Phone ?? "",
                             Email = user.Email ?? "",
                             DateOfBirth = user.DateOfBirth ?? null,
                             Address = user.Address ?? "",
                             FirstName = user.FirstName ?? "",
                             LastName = user.LastName ?? "",
                             GroupTenantId = user.GroupTenantId,
                             ApplicationUserStatus = new ApplicationUserStatusDto
                             {
                                 Id = status.Id,
                                 Code = status.Code ?? "",
                                 Name = status.Name ?? ""
                             },
                             Tenants = userTenants.Where(s => s.DeleteFlag != true).Select(s => new TenantDto
                             {
                                 Id = s.TenantId
                             }).ToList()
                         }).AsNoTracking();

            if (!string.IsNullOrEmpty(request.TextSearch))
            {
                query = query.Where(s => s.UserName.Contains(request.TextSearch) ||
                                         s.FullName.Contains(request.TextSearch) ||
                                         s.Code.Contains(request.TextSearch));
            }

            if (request.GroupTenantId.HasValue && request.GroupTenantId != Guid.Empty)
            {
                query = query.Where(s => s.GroupTenantId == request.GroupTenantId);
            }

            if (!string.IsNullOrEmpty(request.Email))
                query = query.Where(x => x.Email.Contains(request.Email.Trim()));
            if (!string.IsNullOrEmpty(request.Code))
                query = query.Where(x => x.Code.Contains(request.Code.Trim()));
            if (!string.IsNullOrEmpty(request.FullName))
                query = query.Where(x => x.FullName.Contains(request.FullName.Trim()));
            if (!string.IsNullOrEmpty(request.Phone))
                query = query.Where(x => x.Phone.Contains(request.Phone.Trim()));
            if (!string.IsNullOrEmpty(request.DateOfBirth))
            {
                DateTime.TryParseExact(request.DateOfBirth.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDateOfBirth);
                query = query.Where(x => x.DateOfBirth.HasValue && x.DateOfBirth.Value.Date == parsedDateOfBirth.Date);
            }
            if (!string.IsNullOrEmpty(request.Address))
                query = query.Where(x => x.Address.Contains(request.Address.Trim()));

            if (request.StatusId.HasValue && request.StatusId.Value != Guid.Empty)
            {
                query = query.Where(x => x.ApplicationUserStatus.Id == request.StatusId.Value);
            }

            int totalRecords = await query.CountAsync();

            if (!string.IsNullOrEmpty(request.OrderCol))
            {
                if (string.IsNullOrEmpty(request.OrderDir)) request.OrderDir = "ASC";
                query = FunctionUtilServices.OrderByDynamic(query, request.OrderCol, request.OrderDir.ToUpper() == "DESC" ? false : true);
            }

            query = query.Skip((request.PageIndex - 1) * request.PageSize)
                                .Take(request.PageSize);

            var data = await query.AsNoTracking().ToListAsync();

            var tenantIds = data.SelectMany(user => user.Tenants.Select(tenant => tenant.Id)).ToList();

            var tenants = await (from t1 in _context.Tenants where tenantIds.Contains(t1.Id) && t1.DeleteFlag != true select t1).AsNoTracking().ToListAsync();
            foreach (var item in data)
            {
                var ids = item.Tenants.Select(t => t.Id);
                item.Tenants = tenants.Where(s => ids.Contains(s.Id) && s.DeleteFlag != true)
                                      .Select(s => new TenantDto
                                      {
                                          Id = s.Id,
                                          Name = s.Name ?? "",
                                          Logo = s.Logo ?? "",
                                          ThemeColor = s.ThemeColor ?? ""
                                      }).ToList();
            }

            return Result<PaginatedList<ApplicationUserWithTenantDto>>.Success(new PaginatedList<ApplicationUserWithTenantDto>(data, totalRecords, request.PageIndex, request.PageSize));
        }

        public async Task<Result<ApplicationUserWithTenantDto>> UpdateStatus(UpdateStatusRequest request)
        {
            var status = await _context.UserStatus.FirstOrDefaultAsync(s => s.Code == request.Status && s.DeleteFlag != true);
            if (status == null) throw new ApplicationException($"Không tìm thấy dữ liệu với Id: {request.Status}");
            var user = await _context.Users.FirstOrDefaultAsync(s => s.Id == request.Id && s.DeleteFlag != true);
            if (user == null) throw new ApplicationException($"Không tìm thấy dữ liệu với Id: {request.Id}");
            user.ApplicationUserStatus = status;
            user.ApplicationUserStatusId = status.Id;

            var getConnecttionStr = (from userTenant in _context.UserTenants
                                     join tenant in _context.Tenants on userTenant.TenantId equals tenant.Id
                                     where userTenant.ApplicationUserId == user.Id && userTenant.DeleteFlag != true && tenant.DeleteFlag != true
                                     select new
                                     {
                                         Id = userTenant.ApplicationUserId,
                                         connectStr = tenant.ConnectionString
                                     }).ToList();

            _context.Update(user);
            await _context.SaveChangesAsync();
            var result = await GetById(user.Id);
            if (getConnecttionStr.Count <= 0)
                return Result<ApplicationUserWithTenantDto>.Success(result.Data);

            foreach (var conStr in getConnecttionStr)
            {
                _userService.SetConnectDB(conStr.connectStr);

                var applicationUser = await _apcontext.ApplicationUsers.Where(u => u.Id == conStr.Id).AsNoTracking().FirstOrDefaultAsync();

                if (applicationUser == null)
                    throw new Exception("Không tìm thấy người dùng");

                var applicationUserStatus = await _apcontext.ApplicationUserStatuses.Where(s => s.Code == status.Code).AsNoTracking().FirstOrDefaultAsync();

                if (applicationUserStatus == null)
                    throw new Exception($"Không tìm thấy trạng thái {status.Code}");

                applicationUser.ApplicationUserStatusId = applicationUserStatus.Id;

                _apcontext.ApplicationUsers.Update(applicationUser);
                await _apcontext.SaveChangesAsync(new CancellationToken());

                _userService.ClearConnectDB();

                #region oldcode

                //            using (NpgsqlConnection con = new NpgsqlConnection(conStr.connectStr))
                //{
                //	con.Open();

                //	using (NpgsqlCommand selectCmd = new NpgsqlCommand("SELECT * FROM \"ApplicationUsers\" WHERE \"Id\" = @Id", con))
                //	{
                //		selectCmd.Parameters.AddWithValue("Id", conStr.Id);
                //		int temp = 0;
                //		using (NpgsqlDataReader dr = selectCmd.ExecuteReader(CommandBehavior.SingleResult))
                //		{
                //			while (dr.Read())
                //			{
                //				temp = 1;
                //			}
                //			dr.Close();
                //		}

                //		if (temp == 1)
                //		{
                //			Guid IdStatus = Guid.Empty;

                //			using (NpgsqlCommand selectUserStatus = new NpgsqlCommand("SELECT * FROM \"ApplicationUserStatuses\" WHERE \"Code\" = @Code", con))
                //			{
                //				selectUserStatus.Parameters.AddWithValue("Code", status.Code);

                //				using (NpgsqlDataReader selectUserStatusReader = selectUserStatus.ExecuteReader(CommandBehavior.SingleResult))
                //				{
                //					while (selectUserStatusReader.Read())
                //					{
                //						IdStatus = selectUserStatusReader.GetGuid(selectUserStatusReader.GetOrdinal("Id"));
                //					}
                //					selectUserStatusReader.Close();
                //				}
                //			}

                //			using (NpgsqlCommand updateCmd = new NpgsqlCommand("UPDATE \"ApplicationUsers\" SET \"ApplicationUserStatusId\" = @ApplicationUserStatusId WHERE \"Id\" = @Id", con))
                //			{
                //				updateCmd.Parameters.AddWithValue("ApplicationUserStatusId", IdStatus);
                //				updateCmd.Parameters.AddWithValue("Id", conStr.Id);

                //				updateCmd.ExecuteNonQuery();
                //			}
                //		}
                //	}
                //	con.Close();
                //}
                #endregion
            }
            return Result<ApplicationUserWithTenantDto>.Success(result.Data);
        }

        public async Task<Result<ApplicationUserWithTenantDto>> GetById(Guid id)
        {
            var data = await (from user in _context.Users
                              join status in _context.UserStatus on user.ApplicationUserStatusId equals status.Id
                              join userTenant in _context.UserTenants on user.UserName equals userTenant.UserName into userTenants
                              where user.Id == id && user.DeleteFlag != true
                              select new ApplicationUserWithTenantDto
                              {
                                  Id = user.Id,
                                  UserName = user.UserName ?? "",
                                  FullName = user.FullName ?? "",
                                  Code = user.Code ?? "",
                                  Phone = user.Phone ?? "",
                                  Email = user.Email ?? "",
                                  DateOfBirth = user.DateOfBirth ?? null,
                                  Address = user.Address ?? "",
                                  FirstName = user.FirstName ?? "",
                                  LastName = user.LastName ?? "",
                                  ApplicationUserStatus = new ApplicationUserStatusDto
                                  {
                                      Id = status.Id,
                                      Code = status.Code ?? "",
                                      Name = status.Name ?? ""
                                  },
                                  Tenants = userTenants.Where(s => s.DeleteFlag != true).Select(s => new TenantDto
                                  {
                                      Id = s.TenantId
                                  }).ToList()
                              }).AsNoTracking().FirstOrDefaultAsync();
            if (data == null) throw new ApplicationException();

            var tenantIds = data.Tenants.Select(s => s.Id).ToList();
            var tenants =
            data.Tenants = await (from s in _context.Tenants
                                  where tenantIds.Contains(s.Id) && s.DeleteFlag != true
                                  select new TenantDto
                                  {
                                      Id = s.Id,
                                      Name = s.Name ?? "",
                                      Logo = s.Logo ?? "",
                                      ThemeColor = s.ThemeColor ?? ""
                                  }).AsNoTracking().ToListAsync();

            return Result<ApplicationUserWithTenantDto>.Success(data);
        }

        public async Task<Result<Guid>> RegisterUser(UserMainTenantDto obj, UserStatus? status = null, Guid? groupTenant = null, Guid? userId = null, bool? isHasOtp = false, string? otpCode = "", string? deviceId = "")
        {
            if (isHasOtp != null && isHasOtp == true)
            {
                var otpSend = await _otpSendService.GetOtpByEmail(obj.Email, "", otpCode);

                if (otpSend == null)
                    throw new ApplicationException("Mã OTP không tồn tại.");

                if (Math.Abs((otpSend.DateInput - DateTime.Now).Minutes) > 5)
                    throw new ApplicationException("OTP hết hạn.");
            }

            status = status ?? await _context.UserStatus.FirstOrDefaultAsync(s => s.Code == UserStatusEnum.ACTIVE.ToString());

            var getGroupTenant = await _context.GroupTenants.FirstOrDefaultAsync(gt => gt.Id == groupTenant);
            int countLimitUser = await _context.Users.CountAsync(u => u.GroupTenantId == groupTenant && u.DeleteFlag != true);
            if (countLimitUser == getGroupTenant.LimitUser)
                throw new ApplicationException("Số lượng tài khoản trong công ty của bạn đã đạt đến giới hạn. Vui lòng liên hệ với quản trị viên để nâng cấp gói");
            if (string.IsNullOrEmpty(obj.Password))
            {
                throw new ApplicationException("Vui lòng điền mật khẩu");
            }

            var exist = await _context.Users.Where(s => s.Email.ToLower() == obj.Email.ToLower() && s.GroupTenantId == getGroupTenant.Id && s.DeleteFlag != true)
                                            .FirstOrDefaultAsync();
            if (exist != null) throw new ApplicationException("Email đã được sử dụng");

            exist = await _context.Users.Where(s => s.Phone == obj.Phone && s.GroupTenantId == getGroupTenant.Id && s.DeleteFlag != true)
                                        .FirstOrDefaultAsync();
            if (exist != null) throw new ApplicationException("Số điện thoại đã được sử dụng");

            exist = await _context.Users.Where(s => s.Code == obj.Code && s.GroupTenantId == getGroupTenant.Id && s.DeleteFlag != true)
                                        .FirstOrDefaultAsync();
            if (exist != null) throw new ApplicationException("Mã nhân viên đã được sử dụng");

            var user = new User
            {
                Id = Guid.NewGuid(),
                //Code = StringHelper.GenerateCode(),
                Code = obj.Code ?? "",
                Address = obj.Address ?? "",
                Email = obj.Email ?? "",
                Phone = obj.Phone ?? "",
                Password = FunctionUtils.GetMd5HashSalt(obj.Password),
                UserName = obj.Email ?? "",
                DateOfBirth = obj.DateOfBirth ?? null,
                FirstName = obj.FirstName ?? "",
                LastName = obj.LastName ?? "",
                FullName = obj.FullName ?? "",
                IsAdmin = false,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
                ApplicationUserStatus = status,
                GroupTenantId = groupTenant,
                CreatedApplicationUserId = userId,
                DeviceID = deviceId,
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Result<Guid>.Success(user.Id);
        }

        public async Task<Result<List<ApplicationUserWithTenantDto>>> AddUser(List<AddOrUpdateRequest> request, Guid? groupTenant = null, Guid? userId = null)
        {
            List<ApplicationUserWithTenantDto> updatedSuccess = new List<ApplicationUserWithTenantDto>();
            var status = await _context.UserStatus.FirstOrDefaultAsync(s => s.Code == UserStatusEnum.ACTIVE.ToString());
            foreach (AddOrUpdateRequest addOrUpdateRequest in request)
            {
                if (addOrUpdateRequest.Data == null)
                {
                    throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
                }
                var obj = (UserMainTenantDto)_internalService.MapValueToObject(new UserMainTenantDto(), addOrUpdateRequest.Data, new UserMainTenantDto());
                var newUserId = await RegisterUser(obj, status, groupTenant, userId);
                var tmp = await GetById(newUserId.Data);
                updatedSuccess.Add(tmp.Data);
            }
            return Result<List<ApplicationUserWithTenantDto>>.Success(updatedSuccess);
        }

        public async Task<Result<List<ApplicationUserWithTenantDto>>> UpdateUser(List<AddOrUpdateRequest> request, Guid? userId = null)
        {
            List<ApplicationUserWithTenantDto> updatedSuccess = new List<ApplicationUserWithTenantDto>();
            var status = await _context.UserStatus.FirstOrDefaultAsync(s => s.Code == UserStatusEnum.ACTIVE.ToString());
            foreach (AddOrUpdateRequest addOrUpdateRequest in request)
            {
                if (addOrUpdateRequest.Data == null)
                {
                    throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
                }
                var user = await _context.Users.FirstOrDefaultAsync(s => s.Id == addOrUpdateRequest.Id && s.DeleteFlag != true);
                if (user == null) throw new ApplicationException("Không tìm thấy");

                var obj = (UserMainTenantDto)_internalService.MapValueToObject(new UserMainTenantDto(), addOrUpdateRequest.Data, new UserMainTenantDto());
                if (string.IsNullOrEmpty(obj.Email))
                {
                    throw new ApplicationException("Email không thể rỗng");
                }
                if (string.IsNullOrEmpty(obj.Phone))
                {
                    throw new ApplicationException("Số điện thoại không thể rỗng");
                }

                user.Email = obj.Email;
                user.Phone = obj.Phone;
                user.Code = obj.Code ?? "";
                user.FirstName = obj.FirstName ?? "";
                user.LastName = obj.LastName ?? "";
                user.DateOfBirth = obj.DateOfBirth;
                user.UserName = obj.Email ?? user.UserName;
                user.FullName = obj.FullName ?? "";
                user.Address = obj.Address ?? "";
                user.LastModifiedApplicationUserId = userId;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                var tmp = await GetById(user.Id);
                updatedSuccess.Add(tmp.Data);

                var getConnecttionStr = (from userTenant in _context.UserTenants
                                         join tenant in _context.Tenants on userTenant.TenantId equals tenant.Id
                                         where userTenant.ApplicationUserId == user.Id && userTenant.DeleteFlag != true && tenant.DeleteFlag != true
                                         select new
                                         {
                                             Id = userTenant.ApplicationUserId,
                                             connectStr = tenant.ConnectionString
                                         }).ToList();
                if (getConnecttionStr.Count <= 0)
                    return Result<List<ApplicationUserWithTenantDto>>.Success(updatedSuccess);

                foreach (var conStr in getConnecttionStr)
                {
                    _userService.SetConnectDB(conStr.connectStr);

                    var applicationUser = await _apcontext.ApplicationUsers.Where(au => au.Id == conStr.Id).AsNoTracking().FirstOrDefaultAsync();

                    if (applicationUser == null)
                    {
                        continue;
                    }

                    applicationUser.Email = obj.Email;
                    applicationUser.PhoneNumber = obj.Phone;
                    applicationUser.Code = obj.Code ?? "";
                    applicationUser.FirstName = obj.FirstName ?? "";
                    applicationUser.LastName = obj.LastName ?? "";
                    applicationUser.DateOfBirth = obj.DateOfBirth;
                    applicationUser.UserName = obj.Email ?? user.UserName;
                    applicationUser.FullName = obj.FullName ?? "";
                    applicationUser.Address = obj.Address ?? "";
                    applicationUser.LastModifiedApplicationUserId = userId;

                    _apcontext.ApplicationUsers.Update(applicationUser);
                    await _apcontext.SaveChangesAsync(_cancellationToken);

                    _userService.ClearConnectDB();

                    #region oldcode 
                    //               using (NpgsqlConnection con = new NpgsqlConnection(conStr.connectStr))
                    //{
                    //	con.Open();

                    //	using (NpgsqlCommand selectCmd = new NpgsqlCommand("SELECT * FROM \"ApplicationUsers\" WHERE \"Id\" = @Id", con))
                    //	{
                    //		selectCmd.Parameters.AddWithValue("Id", conStr.Id);
                    //		int temp = 0;
                    //		using (NpgsqlDataReader dr = selectCmd.ExecuteReader(CommandBehavior.SingleResult))
                    //		{
                    //			while (dr.Read())
                    //			{
                    //				temp = 1;
                    //			}
                    //			dr.Close();
                    //		}

                    //		if (temp == 1)
                    //		{
                    //			using (NpgsqlCommand updateCmd = new NpgsqlCommand("UPDATE \"ApplicationUsers\" SET \"Email\" = @Email, \"PhoneNumber\" = @Phone, \"FirstName\" = @FirstName, \"LastName\" = @LastName, \"DateOfBirth\" = @DateOfBirth, \"UserName\" = @UserName, \"FullName\" = @FullName, \"Address\" = @Address WHERE \"Id\" = @Id", con))
                    //			{
                    //				updateCmd.Parameters.AddWithValue("Email", user.Email);
                    //				updateCmd.Parameters.AddWithValue("Phone", user.Phone);
                    //				updateCmd.Parameters.AddWithValue("FirstName", user.FirstName);
                    //				updateCmd.Parameters.AddWithValue("LastName", user.LastName);
                    //				updateCmd.Parameters.AddWithValue("DateOfBirth", user.DateOfBirth ?? null);
                    //				updateCmd.Parameters.AddWithValue("UserName", user.UserName);
                    //				updateCmd.Parameters.AddWithValue("FullName", user.FullName);
                    //				updateCmd.Parameters.AddWithValue("Address", user.Address);
                    //				updateCmd.Parameters.AddWithValue("Id", user.Id);

                    //				updateCmd.ExecuteNonQuery();
                    //			}
                    //		}
                    //	}
                    //	con.Close();
                    //}
                    #endregion

                }
            }
            return Result<List<ApplicationUserWithTenantDto>>.Success(updatedSuccess);
        }

        public async Task<Result<List<ApplicationUserWithTenantDto>>> GetListWithFilterQuery(FilterQueryRequest request)
        {
            var query = (from user in _context.Users
                         join status in _context.UserStatus on user.ApplicationUserStatusId equals status.Id
                         join userTenant in _context.UserTenants on user.UserName equals userTenant.UserName into userTenants
                         where user.DeleteFlag != true
                         orderby user.CreatedDate descending
                         select new ApplicationUserWithTenantDto
                         {
                             Id = user.Id,
                             UserName = user.UserName ?? "",
                             FullName = user.FullName ?? "",
                             Code = user.Code ?? "",
                             Phone = user.Phone ?? "",
                             Email = user.Email ?? "",
                             DateOfBirth = user.DateOfBirth ?? null,
                             Address = "",
                             FirstName = user.FirstName ?? "",
                             LastName = user.LastName ?? "",
                             GroupTenantId = user.GroupTenantId,
                             ApplicationUserStatus = new ApplicationUserStatusDto
                             {
                                 Id = status.Id,
                                 Code = status.Code ?? "",
                                 Name = status.Name ?? ""
                             },
                             Tenants = userTenants.Where(s => s.DeleteFlag != true).Select(s => new TenantDto
                             {
                                 Id = s.TenantId
                             }).ToList()
                         }).AsNoTracking();

            if (!string.IsNullOrEmpty(request.TextSearch))
            {
                query = query.Where(s => s.UserName.Contains(request.TextSearch) ||
                                         s.FullName.Contains(request.TextSearch) ||
                                         s.Code.Contains(request.TextSearch));
            }

            if (request.GroupTenantId.HasValue && request.GroupTenantId != Guid.Empty)
            {
                query = query.Where(s => s.GroupTenantId == request.GroupTenantId);
            }

            if (request.Skip != null)
            {
                query = query.Skip(request.Skip.Value);
            }

            if (request.TotalRecord != null)
            {
                query = query.Take(request.TotalRecord.Value);
            }

            var data = await query.AsNoTracking().ToListAsync();

            var tenantIds = data.SelectMany(user => user.Tenants.Select(tenant => tenant.Id)).ToList();

            var tenants = await (from t1 in _context.Tenants where tenantIds.Contains(t1.Id) select t1).AsNoTracking().ToListAsync();
            foreach (var item in data)
            {
                var ids = item.Tenants.Select(t => t.Id);
                item.Tenants = tenants.Where(s => ids.Contains(s.Id) && s.DeleteFlag != true)
                                      .Select(s => new TenantDto
                                      {
                                          Id = s.Id,
                                          Name = s.Name ?? "",
                                          Logo = s.Logo ?? "",
                                          ThemeColor = s.ThemeColor ?? ""
                                      }).ToList();
            }

            return Result<List<ApplicationUserWithTenantDto>>.Success(data);
        }

        public async Task<Result<string>> DeleteByIds(DeleteRequest request)
        {
            string result = string.Empty;

            if (request.Ids == null) throw new ApplicationException("Không tìm thấy tham số Id.");
            List<Guid> ids = request.Ids.Select(m => Guid.Parse(m)).ToList();
            var query = await _context.Users.Where(m => ids.Contains(m.Id) && m.DeleteFlag != true).ToListAsync();
            if (query == null || query.Count == 0) throw new ApplicationException($"Không tìm thấy trong dữ liệu có Id: {string.Join(";", request.Ids)}");

            foreach (var item in query)
            {
                item.DeleteFlag = true;
                item.LastModifiedDate = DateTime.Now;
                item.LastModifiedApplicationUserId = request.ApplicationUserId;

                var getConnecttionStr = (from userTenant in _context.UserTenants
                                         join tenant in _context.Tenants on userTenant.TenantId equals tenant.Id
                                         where userTenant.ApplicationUserId == item.Id && userTenant.DeleteFlag != true && tenant.DeleteFlag != true
                                         select new
                                         {
                                             Id = userTenant.ApplicationUserId,
                                             TenantId = tenant.Id,
                                             connectStr = tenant.ConnectionString
                                         }).ToList();
                if(getConnecttionStr.Any()) throw new ApplicationException("Tài khoản đã có trong tổ chức!");

                _context.Users.UpdateRange(query);
                await _context.SaveChangesAsync();
                if (getConnecttionStr.Count <= 0)
                    return Result<string>.Success(string.Empty);

                foreach (var conStr in getConnecttionStr)
                {
                    #region old-code
                    /*using (NpgsqlConnection con = new NpgsqlConnection(conStr.connectStr))
					{
						con.Open();

						using (NpgsqlCommand selectCmd = new NpgsqlCommand("SELECT * FROM \"ApplicationUsers\" WHERE \"Id\" = @Id", con))
						{
							selectCmd.Parameters.AddWithValue("Id", conStr.Id);
							int temp = 0;
							using (NpgsqlDataReader dr = selectCmd.ExecuteReader(CommandBehavior.SingleResult))
							{
								while (dr.Read())
								{
									temp = 1;
								}
								dr.Close();
							}

							if (temp == 1)
							{
								using (NpgsqlCommand updateCmd = new NpgsqlCommand("UPDATE \"ApplicationUsers\" SET \"DeleteFlag\" = @DeleteFlag, \"LastModifiedDate\" = @LastModifiedDate, \"LastModifiedApplicationUserId\" = @LastModifiedApplicationUserId WHERE \"Id\" = @Id", con))
								{
									updateCmd.Parameters.AddWithValue("DeleteFlag", true);
									updateCmd.Parameters.AddWithValue("LastModifiedDate", DateTime.Now);
									updateCmd.Parameters.AddWithValue("LastModifiedApplicationUserId", request.ApplicationUserId);
									updateCmd.Parameters.AddWithValue("Id", conStr.Id);

									updateCmd.ExecuteNonQuery();
								}
							}
						}
						con.Close();
					}*/
                    #endregion
                    _userService.SetConnectDB(conStr.connectStr);
                    await _userService.DeleteByIds(new DeleteRequest
                    {
                        Ids = new List<string> { conStr.Id.ToString() ?? "" },
                        ApplicationUserId = request.ApplicationUserId
                    });
                }
                await RemoveAllUserTenant(item.Id);
            }

            return Result<string>.Success(string.Empty);
        }

        private async Task<bool> RemoveAllUserTenant(Guid Id)
        {
            var result = false;
            var tenants = await _context.UserTenants.Where(s => s.ApplicationUserId == Id).ToListAsync();
            if (tenants.Any())
            {
                foreach (var tenant in tenants)
                {
                    tenant.DeleteFlag = true;
                }
                _context.UpdateRange(tenants);
                result = (await _context.SaveChangesAsync()) > 0;
            }
            return result;
        }

        public async Task<Result<ApplicationUserWithTenantDto>> AddToTenant(AddUserToTenantDto request, Guid? userId = null)
        {
            var user = await _context.Users.FirstOrDefaultAsync(s => s.DeleteFlag != true && s.Id == request.Id);
            if (user == null) throw new ApplicationException($"Không tìm thấy tài khoản với Id: {request.Id}");

            var exist = await _context.UserTenants
                             .FirstOrDefaultAsync(s => s.DeleteFlag != true &&
                              s.ApplicationUserId == request.Id &&
                              s.TenantId == request.TenantId);

            if (exist != null) throw new ApplicationException($"Tài khoản đã tồn tại trong tổ chức");

            var userDto = _mapper.Map<ApplicationUserDto>(user);
            var result = await _userService.AddUserWithManyRole(userDto, request.ApplicationRoleIds);

            if (!result.Succeeded || result.Data == null) throw new ApplicationException($"Thêm tài khoản thất bại");
            var tenant = new UserTenant
            {
                UserName = user.UserName,
                TenantId = request.TenantId,
                ApplicationUserId = user.Id,
                CreatedApplicationUserId = userId
            };
            _context.UserTenants.Add(tenant);
            await _context.SaveChangesAsync();

            var data = _mapper.Map<ApplicationUserWithTenantDto>(result.Data);

            if (data?.ApplicationRoles != null)
            {
                var checkAdminRole = data.ApplicationRoles.FirstOrDefault(item => item.RolePositionId == "ADMIN");
                if (checkAdminRole != null)
                {
                    data.ApplicationRoles.Remove(checkAdminRole);
                }
            }

            return Result<ApplicationUserWithTenantDto>.Success(data);
        }

        public async Task<Result<List<ApplicationUserWithTenantDto>>> GetRecommendUserQuery(FilterQueryRequest request)
        {
            var exists = await _context.UserTenants
                         .Where(s => s.DeleteFlag != true && s.TenantId == request.TenantId)
                         .Select(s => s.ApplicationUserId)
                         .ToListAsync();

            var query = _context.Users.Where(s => !exists.Contains(s.Id) && s.DeleteFlag != true)
                                .Select(s => new ApplicationUserWithTenantDto
                                {
                                    Id = s.Id,
                                    Code = s.Code,
                                    Email = s.Email ?? "",
                                    FirstName = s.FirstName ?? "",
                                    LastName = s.LastName ?? "",
                                    FullName = s.FullName ?? "",
                                    GroupTenantId = s.GroupTenantId
                                }).AsNoTracking();

            if (!string.IsNullOrEmpty(request.TextSearch))
            {
                query = query.Where(s => (s.Email != null && s.Email.Contains(request.TextSearch)) ||
                                         (s.FullName != null && s.FullName.Contains(request.TextSearch)) ||
                                         (s.Code != null && s.Code.Contains(request.TextSearch)));
            }

            if (request.GroupTenantId.HasValue && request.GroupTenantId != Guid.Empty)
            {
                query = query.Where(s => s.GroupTenantId != null && s.GroupTenantId == request.GroupTenantId);
            }

            if (request.Skip != null)
            {
                query = query.Skip(request.Skip.Value);
            }

            if (request.TotalRecord != null)
            {
                query = query.Take(request.TotalRecord.Value);
            }

            var data = await query.AsNoTracking().ToListAsync();

            return Result<List<ApplicationUserWithTenantDto>>.Success(data);
        }

        public async Task<Result<string>> DeleteFromTenantByIds(DeleteRequest request)
        {
            string result = string.Empty;

            if (request.Ids == null) throw new ApplicationException("Không tìm thấy tham số Id.");
            List<Guid> ids = request.Ids.Select(m => Guid.Parse(m)).ToList();

            var users = await _context.UserTenants
                              .Where(s => s.DeleteFlag != true && s.TenantId == request.TenantId &&
                                          s.ApplicationUserId != null && ids.Contains((Guid)s.ApplicationUserId))
                              .ToListAsync();

            foreach (var user in users)
            {
                user.DeleteFlag = true;
            }
            _context.UserTenants.RemoveRange(users);
            await _context.SaveChangesAsync();

            return Result<string>.Success(result);
        }

        public async Task<Result<bool>> UpdatePassword(UpdatePasswordRequest request, bool checkCurrentPassword = true)
        {
            if (request.Data == null)
            {
                throw new ApplicationException($"Không có dữ liệu gửi đến máy chủ.");
            }
            var obj = (UpdatePasswordDto)_internalService.MapValueToObject(new UpdatePasswordDto(), request.Data, new UpdatePasswordDto());

            if (obj.NewPassword != obj.ConfirmPassword)
                throw new ApplicationException("Mật khẩu xác nhận không trùng khớp");

            var user = await _context.Users.FirstOrDefaultAsync(s => s.Id == obj.Id);
            if (user == null) throw new ApplicationException("Không tìm thấy tài khoản");
            //if (checkCurrentPassword)
            //{
            //	var valid = FunctionUtils.GetMd5HashSalt(obj.CurrentPassword ?? "") == user.Password;
            //	if (!valid) throw new ApplicationException("Mật khẩu hiện tại không chính xác");
            //}


            user.Password = FunctionUtils.GetMd5HashSalt(obj.NewPassword);
            user.LastModifiedDate = DateTime.Now;
            user.LastModifiedApplicationUserId = request.id;
            _context.Users.Update(user);
            int row = await _context.SaveChangesAsync();
            return Result<bool>.Success(row > 0);
        }

        public async Task<Result<bool>> UpdatePasswordByMobile(UpdatePasswordMobileRequest request, Guid userId, bool checkCurrentPassword = true)
        {
            if (request.NewPassword != request.ConfirmPassword)
            {
                throw new ApplicationException("Mật khẩu xác nhận không trùng khớp");
            }

            var user = await _context.Users.FirstOrDefaultAsync(s => s.Id == userId);
            if (user == null)
            {
                throw new ApplicationException("Không tìm thấy tài khoản");
            }

            if (checkCurrentPassword)
            {
                var isValid = FunctionUtils.GetMd5HashSalt(request.CurrentPassword ?? "") == user.Password;

                if (!isValid)
                {
                    throw new ApplicationException("Mật khẩu cũ không đúng");
                }
            }

            user.Password = FunctionUtils.GetMd5HashSalt(request.NewPassword);
            user.LastModifiedDate = DateTime.Now;
            user.LastModifiedApplicationUserId = userId;

            _context.Users.Update(user);
            int row = await _context.SaveChangesAsync();

            return Result<bool>.Success(row > 0);
        }

        public async Task<Result<bool>> UpdatePasswordByOtpMobile(UpdatePasswordByOtpMobileRequest request, Guid userId, bool checkCurrentPassword = true)
        {
            if (request.NewPassword != request.ConfirmPassword)
            {
                throw new ApplicationException("Mật khẩu xác nhận không trùng khớp");
            }

            var user = await GetUserByEmail(request.Email, request.GroupTenantId, request.DeviceID ?? "");

            if (user == null)
            {
                throw new ApplicationException("Không tìm thấy tài khoản");
            }

            if (checkCurrentPassword)
            {
                var isValid = FunctionUtils.GetMd5HashSalt(request.CurrentPassword ?? "") == user.Password;

                if (!isValid)
                {
                    throw new ApplicationException("Mật khẩu cũ không đúng");
                }
            }

            user.Password = FunctionUtils.GetMd5HashSalt(request.NewPassword);
            user.LastModifiedDate = DateTime.Now;
            user.LastModifiedApplicationUserId = userId;

            _context.Users.Update(user);
            int row = await _context.SaveChangesAsync();

            return Result<bool>.Success(row > 0);
        }

        public async Task<bool> CheckUserExisted(string tenantId)
        {
            var user = await _context.UserTenants.Where(x => x.TenantId == tenantId).AsNoTracking().FirstOrDefaultAsync();
            return user != null ? true : false;
        }

        public async Task<string> UpdatePasswordGen(User user)
        {
            var newPass = FunctionUtils.GenString();
            user.Password = FunctionUtils.GetMd5HashSalt(newPass);
            user.LastModifiedDate = DateTime.Now;
            user.LastModifiedApplicationUserId = user.Id;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return newPass;
        }

        public async Task<Result<string>> UpdateAvatar(User user, IFormFile avatar)
        {
            var response = await _fileStorageService.UploadFileAsync(new List<IFormFile>() { avatar }, "sass", true);
            if (!response.Any())
            {
                throw new Exception("Thay đổi avatar không thành công");
            }

            user.Avatar = response[0].ServerPath;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Result<string>.Success("Thay đổi avatar thành công");

        }

        public async Task<Result<User>> UpdateProfile(UpdateProfileRequest request)
        {
            var user = await _context.Users.Where(u => u.Id == request.ApplicationUserId && u.DeleteFlag != true).AsNoTracking().FirstOrDefaultAsync();

            if (user == null)
            {
                throw new Exception("Không tìm thấy người dùng");
            }

            if (request.Avatar != null)
            {
                var response = await _fileStorageService.UploadFileAsync(new List<IFormFile>() { request.Avatar }, "sass", true);
                if (!response.Any())
                {
                    throw new Exception("Thay đổi avatar không thành công");
                }

                user.Avatar = response[0].ServerPath;
            }

            if (request.FullName != null)
            {
                user.FullName = request.FullName;
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            var getConnecttionStr = (from userTenant in _context.UserTenants
                                     join tenant in _context.Tenants on userTenant.TenantId equals tenant.Id
                                     where userTenant.ApplicationUserId == user.Id && userTenant.DeleteFlag != true && tenant.DeleteFlag != true
                                     select new
                                     {
                                         Id = userTenant.ApplicationUserId,
                                         connectStr = tenant.ConnectionString
                                     }).ToList();
            if (getConnecttionStr.Count <= 0)
                return Result<User>.Success(user);

            foreach (var item in getConnecttionStr)
            {
                _userService.SetConnectDB(item.connectStr);

                var applicationUser = await _apcontext.ApplicationUsers.Where(au => au.Id == item.Id).AsNoTracking().FirstOrDefaultAsync();

                if (applicationUser == null)
                {
                    continue;
                }

                applicationUser.FullName = request.FullName;

                _apcontext.ApplicationUsers.Update(applicationUser);
                await _apcontext.SaveChangesAsync(_cancellationToken);

                _userService.ClearConnectDB();
            }

            return Result<User>.Success(user);
        }

        public async Task<Result<List<UserStatus>>> GetAllStatus()
        {
            var statuses = await _context.UserStatus.Where(x => x.DeleteFlag != true).ToListAsync();

            return Result<List<UserStatus>>.Success(statuses);
        }

        public async Task<Result<string>> UpdateAvatarByMobile(Guid userId, IFormFile avatar)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                throw new ApplicationException("Không tìm thấy user");
            }

            var response = await _fileStorageService.UploadFileAsync(new List<IFormFile>() { avatar }, "sass", true);

            if (!response.Any())
            {
                throw new Exception("Thay đổi avatar không thành công");
            }

            user.Avatar = response[0].ServerPath;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Result<string>.Success("Thay đổi avatar thành công");
        }

        public async Task<Result<string>> MobileDelete(Guid userId)
        {
            string result = string.Empty;
            var item = await _context.Users.Where(m => m.Id== userId && m.DeleteFlag != true).FirstOrDefaultAsync();
            if (item == null ) throw new ApplicationException($"Không tìm thấy trong dữ liệu có Id: {userId}");
            item.DeleteFlag = true;
            item.LastModifiedDate = DateTime.Now;
            item.LastModifiedApplicationUserId = userId;         
            await _context.SaveChangesAsync();
            return Result<string>.Success(string.Empty);
        }
    }
}
