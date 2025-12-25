namespace Sale_Saas.Application.Features.EmployeeSalaryFeature.Dto;

public class IncomeOfRoleByMonthDto
{
    public Guid? RoleId { get; set; }
    public string? RoleName { get; set; }
    public int? Month { get; set; }
    public decimal? IncomeByRole { get; set; }
    public decimal? IncomeOther { get; set; }
    public decimal? IncomeReceived { get; set; }
    public string? TenantId { get; set; }
    public string? TenantName { get; set; }
    public DateTime? CreatedDate { get; set; }
}
