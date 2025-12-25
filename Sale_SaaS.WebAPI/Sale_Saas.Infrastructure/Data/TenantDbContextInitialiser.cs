using Microsoft.Extensions.Logging;
using Sale_Saas.Domain.Entities.Tenant;

namespace Sale_Saas.Infrastructure.Data;

public interface ITenantDbContextInitialiser
{
    Task SeedAsync();
}
public class TenantDbContextInitialiser : ITenantDbContextInitialiser
{
    private readonly ILogger<TenantDbContextInitialiser> _logger;
    private readonly TenantDbContext _context;
    public TenantDbContextInitialiser(ILogger<TenantDbContextInitialiser> logger,
                                            TenantDbContext context
                                            )
    {
        _logger = logger;
        _context = context;     

    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
    public async Task TrySeedAsync()
    {
        #region Module
        if (!_context.ModuleTenants.Any())
        {
            _context.ModuleTenants.AddRange(new List<ModuleTenant>(){
                new ModuleTenant() {  Code = "QLTK", Name = "Quản lý người dùng", NameEn="User management",  Icon = "tenicon.png" },            
                new ModuleTenant() { Code = "DM", Name = "Danh mục", NameEn = "Categories", Icon = "tenicon.png" },
                new ModuleTenant() { Code = "DM_KH", Name = "Khách hàng", NameEn = "Customers", Icon = "tenicon.png" },
                new ModuleTenant() { Code = "DM_NS", Name = "Nhân sự", NameEn = "Human resources", Icon = "tenicon.png" },
                new ModuleTenant() { Code = "DM_NCC", Name = "Nhà cung cấp", NameEn = "Suppliers", Icon = "tenicon.png" },
                new ModuleTenant() { Code = "DM_MDV", Name = "Mảng dịch vụ", NameEn = "Service segment", Icon = "tenicon.png" },
                new ModuleTenant() { Code = "DM_MDQH", Name = "Mức độ quan hệ", NameEn = "Relationship level", Icon = "tenicon.png" },
                new ModuleTenant() { Code = "DM_GAINS", Name = "Câu hỏi bảng GAINS", NameEn = "GAINS board questions", Icon = "tenicon.png" },
                new ModuleTenant() { Code = "Sale", Name = "Sales", NameEn = "Sales", Icon = "tenicon.png" },              
                new ModuleTenant() { Code = "NS", Name = "Nhân sự", NameEn = "Human resources", Icon = "tenicon.png" },
               }); 
            await _context.SaveChangesAsync();
        }

		#endregion UserStatus
		if (!_context.UserStatus.Any())
		{
			_context.UserStatus.AddRange(new List<UserStatus>(){
				new UserStatus() { Code = "ACTIVE", Name = "Đang hoạt động"},
				new UserStatus() { Code = "UNACTIVE", Name = "Bị khóa"},
			   });
			await _context.SaveChangesAsync();
		}

	}
}