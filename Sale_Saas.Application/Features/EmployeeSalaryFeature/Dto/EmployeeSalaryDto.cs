using Sale_Saas.Application.Models.Identity;

namespace Sale_Saas.Application.Features.EmployeeSalaryFeature.Dto
{
    public class EmployeeSalaryDto : BaseEntityDto
    {
        public Guid UserId { get; set; }
        public string EmployeeCode { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        public decimal? IncomeBeforeTax { get; set; }
        public decimal? IncomeNonTax { get; set; }
        public decimal? Dependent { get; set; }
        public decimal? Insurance { get; set; }
        public decimal? IncomeTax { get; set; }
        public decimal? PersonalIncomeTax { get; set; }
        public decimal? IncomeRecevied { get; set; }
        public decimal? IncomeOther { get; set; }
        public Guid? RoleId { get; set; }
        public string? Note { get; set; }
        public UserBasicInfoDto ApplicationUser { get; set; } = new UserBasicInfoDto();
        public List<ApplicationRoleDto> Roles { get; set; } = new List<ApplicationRoleDto>();

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<EmployeeSalary, EmployeeSalaryDto>();
            }
        }
    }

    public class EmployeeSalaryReportDto : BaseEntityDto
    {
        public decimal? IncomeBeforeTax { get; set; }
        public decimal? IncomeNonTax { get; set; }
        public decimal? Dependent { get; set; }
        public decimal? Insurance { get; set; }
        public decimal? IncomeTax { get; set; }
        public decimal? PersonalIncomeTax { get; set; }
        public decimal? IncomeRecevied { get; set; }
        public UserBasicInfoDto ApplicationUser { get; set; } = new UserBasicInfoDto();
        public List<ApplicationRoleDto> Roles { get; set; } = new List<ApplicationRoleDto>();

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<EmployeeSalary, EmployeeSalaryDto>();
            }
        }
    }

    public class EmployeeSalaryAllRoleReportDto
    {
        public int? Month { get; set; }
        public int? Year { get; set; }
        public decimal? IncomeOther { get; set; }
        public decimal? TotalIncome { get; set; }
        public List<TenantNameIncomRole> TenantNameIncomRoles { get; set; } = new List<TenantNameIncomRole>();

    }
    public class TenantNameIncomRole
    {
        public string? TenantName { get; set; }
        public List<IncomeRoleDto> IncomeRoles { get; set; } = new List<IncomeRoleDto>();
    }
    public class EmployeeSalaryRoleReportDto
    {
        public int? Month { get; set; }
        public int? Year { get; set; }
        public decimal? IncomeOther { get; set; }
        public decimal? TotalIncome { get; set; }
        //public UserBasicInfoDto ApplicationUser { get; set; } = new UserBasicInfoDto();
        public List<IncomeRoleDto> IncomeRoles { get; set; } = new List<IncomeRoleDto>();

    }

    public class EmployeeSalaryAllAdminRoleReportDto
    {
        public string? UserName { get; set; }
        public UserBasicInfoDto ApplicationUser { get; set; } = new UserBasicInfoDto();
        public decimal? IncomeOther { get; set; }
        public decimal? TotalIncome { get; set; }
        public List<TenantNameIncomRole> TenantNameIncomRoles { get; set; } = new List<TenantNameIncomRole>();


    }
    public class EmployeeSalaryAdminRoleReportDto
    {
        public UserBasicInfoDto ApplicationUser { get; set; } = new UserBasicInfoDto();
        public decimal? IncomeOther { get; set; }
        public decimal? TotalIncome { get; set; }
        //public UserBasicInfoDto ApplicationUser { get; set; } = new UserBasicInfoDto();
        public List<IncomeRoleDto> IncomeRoles { get; set; } = new List<IncomeRoleDto>();

    }

    public class EmployeeSalaryUserReportDto
    {
        public decimal? IncomeBeforeTax { get; set; }
        public decimal? IncomeNonTax { get; set; }
        public decimal? Dependent { get; set; }
        public decimal? Insurance { get; set; }
        public decimal? IncomeTax { get; set; }
        public decimal? PersonalIncomeTax { get; set; }
        public decimal? IncomeRecevied { get; set; }
        public string? UserName { get; set; }
        public decimal? IncomeByRole { get; set; }
        public decimal? IncomeOther { get; set; }
        public TenantNameRole? TenantNameRoles { get; set; }
        public UserBasicInfoDto ApplicationUser { get; set; } = new UserBasicInfoDto();

    }

    public class EmployeeSalaryUserReportWithAllTenantDto
    {
        public decimal? IncomeBeforeTax { get; set; }
        public decimal? IncomeNonTax { get; set; }
        public decimal? Dependent { get; set; }
        public decimal? Insurance { get; set; }
        public decimal? IncomeTax { get; set; }
        public decimal? PersonalIncomeTax { get; set; }
        public decimal? IncomeRecevied { get; set; }
        public string? UserName { get; set; }
        public decimal? IncomeByRole { get; set; }
        public decimal? IncomeOther { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        public DateTime? CreatedDate { get; set; }
        public List<TenantNameRole> TenantNameRoles { get; set; } = new List<TenantNameRole>();
        public UserBasicInfoDto ApplicationUser { get; set; } = new UserBasicInfoDto();

    }

    public class TenantNameRole
    {
        public string? TenantName { get; set; }
        public List<string>? RoleNames { get; set; }
    }

    public class RoleBasic
    {
        public Guid Id { get; set; }
        public string? DisplayName { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

    public class EmployeeInfo
    {
        public string? UserName { get; set; }
        public List<TenantNameRoleInfo>? TenantNameRoleInfos { get; set; } = new List<TenantNameRoleInfo>();
        public UserBasicInfoDto ApplicationUser { get; set; } = new UserBasicInfoDto();
    }
    public class TenantNameRoleInfo
    {
        public string? TenantName { get; set; }
        public List<RoleBasic>? RoleInfos { get; set; } = new List<RoleBasic>();
    }

    public class TenantNameRoles
    {
        public string? TenantName { get; set; }
        public Guid Id { get; set; }
        public string? DisplayName { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
