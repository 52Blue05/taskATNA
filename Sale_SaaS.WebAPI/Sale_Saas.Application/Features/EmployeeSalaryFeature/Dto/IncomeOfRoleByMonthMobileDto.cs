namespace Sale_Saas.Application.Features.EmployeeSalaryFeature.Dto;

public class IncomeOfRoleByMonthMobileDto
{
    public int? Month { get; set; }
    public decimal? IncomeOther { get; set; }
    public Guid? RoleId { get; set; }
    public string? RoleName { get; set; }
    public decimal? IncomeByRole { get; set; }
    public string? TenantId { get; set; }
    public string? TenantName { get; set; }
    public DateTime? CreatedDate { get; set; }
}
