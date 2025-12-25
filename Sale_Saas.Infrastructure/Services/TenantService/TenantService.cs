using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Utilities;
using Sale_Saas.Domain.Constants.API;
using Sale_Saas.Domain.Entities.Tenant;
using Sale_Saas.Infrastructure.Services.TenantService.DTOs;
using System.Data;


namespace Sale_Saas.Infrastructure.Services.TenantService
{
    public class TenantService : ITenantService
    {

        private readonly TenantDbContext _context; // database context
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly IApplicationDbContextInitialiser _initialiserService;
        private readonly IApplicationUserService _userService;
        private readonly ApplicationDbContext _applicationDbContext;
        public TenantService(TenantDbContext context, IConfiguration configuration, IServiceProvider serviceProvider,
                                IApplicationDbContextInitialiser initialiserService, IApplicationUserService userService,
                                ApplicationDbContext applicationDbContext)
        {
            _context = context;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _initialiserService = initialiserService;
            _userService = userService;
            _applicationDbContext = applicationDbContext;
        }

        public async Task<Tenant> CreateTenant(CreateTenantRequest request, bool isAdminCreated = false)
        {
            List<string> listMenuActive = null;
            if (request.GroupTenantId.HasValue && request.GroupTenantId != Guid.Empty)
            {
                var groupTenant = await _context.GroupTenants.FindAsync(request.GroupTenantId);
                if (groupTenant == null) throw new ApplicationException("Không tìm thấy công ty");

                int countLimitChild = await _context.Tenants.CountAsync(t => t.GroupTenantId == groupTenant.Id && t.DeleteFlag != true);
                if (countLimitChild == groupTenant.LimitChild)
                    throw new ApplicationException("Số lượng tổ chức của bạn đã đạt đến giới hạn. Vui lòng liên hệ với quản trị viên để nâng cấp gói");
                var query = from order in _context.Orders
                            join orderDetail in _context.OrderDetails
                            on order.Id equals orderDetail.OrderId
                            where order.IsActived == true && order.GroupTenantId == request.GroupTenantId
                            select new OrderDetail
                            {
                                ModuleTenantId = orderDetail.ModuleTenantId
                            };
                listMenuActive = await query.Select(x => x.ModuleTenantId).ToListAsync();
            }
            string newConnectionString = null;
            if (request.Isolated == true)
            {
                newConnectionString = await InnitTenantDatabase(request.Id, request.ConnectionString, listMenuActive);
            }
            Tenant tenant = new() // create a new tenant entity
            {
                Id = request.Id,
                Name = request.Name,
                Logo = string.IsNullOrEmpty(request.Logo) ? APISystemInfoConstant.Default_Logo : request.Logo,
                ThemeColor = request.ThemeColor,
                DeleteFlag = false,
                ConnectionString = newConnectionString,
                IsAdminCreated = isAdminCreated,
                GroupTenantId = request.GroupTenantId
            };

            _context.Add(tenant);
            await _context.SaveChangesAsync();

            return tenant;
        }

        public async Task<string> InnitTenantDatabase(string id, string sConnect, List<string> listMenuActive = null)
        {
            string newConnectionString = null;
            // generate a connection string for new tenant database
            newConnectionString = sConnect;
            if (string.IsNullOrEmpty(sConnect))
            {
                string dbName = "postgres-" + id;
                string defaultConnectionString = _configuration.GetConnectionString("Sale_SaasDbConnectionCore");
                newConnectionString = defaultConnectionString.Replace("Database=postgres-core", "Database=" + dbName);
            }
            // create a new tenant database and bring current with any pending migrations from ApplicationDbContext
            try
            {
                //using IServiceScope scopeApplication = _serviceProvider.CreateScope();
                //ApplicationDbContext dbContext = scopeApplication.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                //dbContext.Database.SetConnectionString(newConnectionString);
                //if (dbContext.Database.GetPendingMigrations().Any())
                //{
                //    Console.ForegroundColor = ConsoleColor.Blue;
                //    Console.WriteLine($"Applying ApplicationDB Migrations for New '{id}' tenant.");
                //    Console.ResetColor();
                //    dbContext.Database.Migrate();

                //    await _initialiserService.SeedAsync(newConnectionString);

                //}
                using (var scopeApplication = _serviceProvider.CreateScope())
                {
                    ApplicationDbContext dbContext = scopeApplication.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    dbContext.Database.SetConnectionString(newConnectionString);
                    if (dbContext.Database.GetPendingMigrations().Any())
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine($"Applying ApplicationDB Migrations for New '{id}' tenant.");
                        Console.ResetColor();
                        dbContext.Database.Migrate();

                        await _initialiserService.SeedAsync(newConnectionString, listMenuActive);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return newConnectionString;
        }

        public async Task<Tenant> UpdateTenant(CreateTenantRequest request)
        {
            var tenant = await _context.Tenants.Where(s => s.DeleteFlag != true && s.Id == request.Id).FirstOrDefaultAsync();
            if (tenant == null) throw new ApplicationException($"Không tìm thấy dữ liệu với Id: {request.Id}");
            tenant.Name = request.Name ?? tenant.Name;
            tenant.Logo = request.Logo ?? tenant.Logo;
            tenant.ThemeColor = request.ThemeColor;
            _context.Tenants.Update(tenant);
            await _context.SaveChangesAsync();
            tenant.ConnectionString = "";
            return tenant;
        }

        public Tenant GetTenantInfoByTenantId(string tenantId)
        {
            return _context.Tenants.Where(x => x.Id == tenantId && x.DeleteFlag != true).AsNoTracking().FirstOrDefault();
        }

        public async Task<Tenant> GetTenantInfoByTenantName(string tenantname, Guid groupTenantId)
        {
            return await _context.Tenants.Where(x => x.Name.Contains(tenantname) && x.GroupTenantId == groupTenantId && x.DeleteFlag != true).AsNoTracking().FirstOrDefaultAsync();
        }
        public List<Tenant> GetListTenant(bool cnStr = true)
        {
            var tenants = _context.Tenants
                                  .Where(s => s.DeleteFlag != true)
                                  .OrderByDescending(x => x.CreatedDate).AsNoTracking()
                                  .Select(s => new Tenant
                                  {
                                      Id = s.Id,
                                      Name = s.Name,
                                      Host = cnStr ? s.Host : "",
                                      SubDomain = cnStr ? s.SubDomain : "",
                                      Logo = s.Logo,
                                      ThemeColor = s.Logo,
                                      ConnectionString = cnStr ? s.ConnectionString : "",
                                  }).ToList();
            return tenants;
        }

        public string GetTenantIdBySubDomain(string subDomain)
        {
            var target = _context.Tenants.Where(x => x.SubDomain.Contains(subDomain) && x.DeleteFlag != true).AsNoTracking().FirstOrDefault();
            return target.Id;
        }

        public string GetTenantNameByConStr(string connectStr)
        {
            var target = _context.Tenants.Where(x => x.ConnectionString.Contains(connectStr) && x.DeleteFlag != true).AsNoTracking().FirstOrDefault();
            return target.Name;
        }

        public async Task<string> DeleteByIds(DeleteRequest request)
        {
            if (request.Ids == null) ExceptionHelper.RequestEmpty(request.Locale);

            List<string> ids = request.Ids!.ToList();
            var query = await _context.Tenants.Where(m => ids.Contains(m.Id) && m.DeleteFlag != true).ToListAsync();
            if (query == null || query.Count == 0) ExceptionHelper.NotFound(ids, request.Locale);

            var userTenants = await _context.UserTenants.Where(ut => ids.Contains(ut.TenantId) && ut.DeleteFlag != true).AsNoTracking().ToListAsync();
            if (userTenants.Any()) throw new Exception("Tổ chức đang có thông tin tài khoản!");

            foreach (var item in query)
            {
                item.DeleteFlag = true;
                item.LastModifiedDate = DateTime.Now;
                item.LastModifiedApplicationUserId = request.ApplicationUserId;
                using (NpgsqlConnection con = new NpgsqlConnection(item.ConnectionString))
                {
                    con.Open();

                    using (NpgsqlCommand updateCmd = new NpgsqlCommand("UPDATE \"ApplicationUsers\" SET \"DeleteFlag\" = @DeleteFlag, \"LastModifiedDate\" = @LastModifiedDate, \"LastModifiedApplicationUserId\" = @LastModifiedApplicationUserId ", con))
                    {
                        updateCmd.Parameters.AddWithValue("DeleteFlag", true);
                        updateCmd.Parameters.AddWithValue("LastModifiedDate", DateTime.Now);
                        updateCmd.Parameters.AddWithValue("LastModifiedApplicationUserId", request.ApplicationUserId);
                        updateCmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
            }

            foreach (var item in userTenants)
            {
                item.DeleteFlag = true;
                item.LastModifiedDate = DateTime.Now;
                item.LastModifiedApplicationUserId = request.ApplicationUserId;
            }
            await _context.SaveChangesAsync();
            return "Xóa tenant thành công";
        }

        public async Task<List<string>> GetListTenantByListIds(string Ids)
        {
            List<string> listConnStr = new List<string>();
            List<string> listTenantIds = new List<string>(Ids.Split(','));

            if (listConnStr.Count < 0)
                throw new Exception("Mã tổ chức không thể bỏ trống. Vui lòng nhập mã tổ chức trước khi tiến hành import file");

            foreach (string id in listTenantIds)
            {
                string idTenant = id.Trim();
                var connStr = await _context.Tenants.Where(t => t.Id == idTenant).Select(t => t.ConnectionString).FirstOrDefaultAsync();
                if (connStr == null)
                    throw new Exception($"Mã tổ chức {id} không được tìm thấy nên không thể import file này. Vui lòng liên hệ với quản trị viên");
                listConnStr.Add(connStr);
            }

            return listConnStr;
        }

        public async Task<string> GetTenantById(string id)
        {
            string idTenantNormalized = id.Trim();
            var connStr = await _context.Tenants.Where(t => t.Id == idTenantNormalized
                                                         && t.DeleteFlag != true)
                                                .Select(t => t.ConnectionString)
                                                .FirstOrDefaultAsync();

            return connStr;
        }

        public List<Tenant> GetListTenantByGroup(Guid groupTenant)
        {
            var tenants = _context.Tenants
                                  .Where(s => s.DeleteFlag != true && s.GroupTenantId == groupTenant)
                                  .OrderByDescending(x => x.CreatedDate).AsNoTracking()
                                  .Select(s => new Tenant
                                  {
                                      Id = s.Id,
                                      Name = s.Name,
                                      Host = "",
                                      SubDomain = "",
                                      Logo = s.Logo,
                                      ThemeColor = s.Logo,
                                      ConnectionString = "",
                                  }).ToList();
            return tenants;
        }

        public async Task<Result<List<Tenant>>> GetListTenantWithDeleted()
        {
            var listTenants = await (from groupTenant in _context.GroupTenants
                                     join tenant in _context.Tenants on groupTenant.Id equals tenant.GroupTenantId
                                     where groupTenant.DeleteFlag == true && tenant.DeleteFlag == true
                                     select new Tenant
                                     {
                                         Id = tenant.Id,
                                         Name = tenant.Name,
                                         Host = tenant.Host,
                                         SubDomain = tenant.SubDomain,
                                         Logo = tenant.Logo,
                                         ThemeColor = tenant.ThemeColor,
                                         ConnectionString = tenant.ConnectionString,
                                         DeleteFlag = tenant.DeleteFlag,
                                     }).AsNoTracking().ToListAsync();

            return Result<List<Tenant>>.Success(listTenants);
        }

        public async Task<Result<string>> DropTenant(DropDbRequest request)
        {
            if (request.Password != "avv@123")
                throw new Exception("Vui lòng nhập đúng mật khẩu để xóa");
            if (!string.IsNullOrEmpty(request.TenantId))
            {
                var getTenant = await _context.Tenants.Where(t => t.Id == request.TenantId).FirstOrDefaultAsync();

                if (getTenant == null)
                    throw new Exception("Tenant không tồn tại");

                _userService.SetConnectDB(getTenant.ConnectionString);
                _userService.DropDB();
                _context.Tenants.Remove(getTenant);
                await _context.SaveChangesAsync();
            }
            if (request.ListTenantId != null)
            {
                //remove  noti
                var listNoti = await _context.Notifications.Where(t => request.ListTenantId.Contains(t.TenantId)).ToListAsync();
                if (listNoti.Count > 0)
                {
                    _context.Notifications.RemoveRange(listNoti);
                    await _context.SaveChangesAsync();
                }
                var listGroupTenantId = new List<Guid>();
                var listTenant = await _context.Tenants.Where(t => request.ListTenantId.Contains(t.Id)).ToListAsync();
                for (int i = 0; i < listTenant.Count; i++)
                {
                    _userService.SetConnectDB(listTenant[i].ConnectionString);
                    _userService.DropDB();
                    _context.Tenants.Remove(listTenant[i]);
                    listGroupTenantId.Add(listTenant[i].GroupTenantId.Value);
                }
                ////remove group tenant
                //if (listGroupTenantId.Count > 0)
                //{
                //    var groupTenants = await _context.GroupTenants.Where(t => listGroupTenantId.Contains(t.Id)).ToListAsync();
                //    if(groupTenants.Count > 0)
                //        _context.GroupTenants.RemoveRange(groupTenants);
                //}
                await _context.SaveChangesAsync();
            }
            return Result<string>.Success("Xóa tenant thành công");
        }

        public List<Tenant> GetListTenantByAdmin(string tenantId)
        {
            var getTenant = _context.Tenants.Where(t => t.Id == tenantId).AsNoTracking().FirstOrDefault();

            var listTenants = (from groupTenant in _context.GroupTenants
                               join tenant in _context.Tenants on groupTenant.Id equals tenant.GroupTenantId
                               where groupTenant.DeleteFlag != true && tenant.DeleteFlag != true && groupTenant.Id == getTenant.GroupTenantId
                               select new Tenant
                               {
                                   Id = tenant.Id,
                                   Name = tenant.Name,
                                   Host = tenant.Host,
                                   SubDomain = tenant.SubDomain,
                                   Logo = tenant.Logo,
                                   ThemeColor = tenant.ThemeColor,
                                   ConnectionString = tenant.ConnectionString,
                                   DeleteFlag = tenant.DeleteFlag,
                               }).AsNoTracking().ToList();

            return listTenants;
        }

        public async Task<bool> UpdateRemoveFeatureMenu()
        {
            var listTenant = await _context.Tenants.AsNoTracking().ToListAsync();
            foreach (var tenant in listTenant)
            {
                _applicationDbContext.SetConnectString(tenant.ConnectionString);

                try
                {
                    await _initialiserService.RemoveRolePositionFeatureMenu(tenant.ConnectionString);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                _applicationDbContext.ClearChangeTracker();
            }
            return true;
        }

        public async Task<Result<bool>> UpdateRemoveFeatureMenuByTenant(string tenantId)
        {
            var tenant = await _context.Tenants.Where(s => s.Id == tenantId && s.DeleteFlag != true)
                                               .AsNoTracking()
                                               .FirstOrDefaultAsync();

            if (tenant == null)
            {
                throw new ApplicationException("Tổ chức không tồn tại");
            }

            try
            {
                await _initialiserService.RemoveRolePositionFeatureMenu(tenant.ConnectionString ?? "");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return Result<bool>.Success(true);
        }
    }
}
