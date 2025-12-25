using Microsoft.Extensions.Configuration;
using Sale_Saas.Application.Common.Mappings;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Domain.Entities.Tenant;
using Sale_Saas.Infrastructure.Services.TenantService.DTOs;

namespace Sale_Saas.Infrastructure.Services.TenantService
{
    public class ModuleTenantService : IModuleTenantService
    {

        private readonly TenantDbContext _context; // database context
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        public ModuleTenantService(TenantDbContext context, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _context = context;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        public CreateModuleTenantDto CreateModuleTenant(CreateModuleTenantDto request)
        {
            var checkExisted = _context.ModuleTenants.Where(x => x.Code == request.Code && x.DeleteFlag != true).FirstOrDefault();
            if (checkExisted != null)
            {
                return null;
            }
            CreateModuleTenantDto ModuleTenant = new()
            {
                Code = request.Code,
                Name = request.Name
            };
            var newData = _context.Add(ModuleTenant);
            _context.SaveChanges();
            return newData.Entity;
        }
        public ModuleTenant GetModuleTenant(string code)
        {
            var moduleTenant = _context.ModuleTenants.Where(x => x.Code == code && x.DeleteFlag != true).FirstOrDefault();
            return moduleTenant;
        }

        public  PaginatedList<ModuleTenant> GetListModuleTenant( GetListWithPaginationQueryRequest request)
        {
            var query = _context.ModuleTenants.Where(x => !string.IsNullOrEmpty(x.Code) && x.DeleteFlag != true);
            if (!string.IsNullOrEmpty(request.TextSearch))
                query = query.Where(x => x.Name.Contains(request.TextSearch) || x.Code.Contains(request.TextSearch) );
            return (query.OrderBy(x => x.CreatedDate).PaginatedListNoAsync(request.PageIndex , request.PageSize));
        }
        public  bool TrySeedAsync()
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
                 _context.SaveChanges();
            }

            #endregion
            return true;
        }

        public async Task<Result<List<ModuleTenantDto>>> GetListAllModuleTenant()
        {
            var listModuleTenant = await _context.ModuleTenants.ToListAsync();

            List<ModuleTenantDto> result = new List<ModuleTenantDto>();

            foreach (var tenant in listModuleTenant)
            {
                ModuleTenantDto moduleTenantDto = new ModuleTenantDto
                {
                    Code = tenant.Code,
                    Name = tenant.Name
                };
                result.Add(moduleTenantDto);
            }

            return Result<List<ModuleTenantDto>>.Success(result);
        }
    }
}
