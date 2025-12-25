using Sale_Saas.Application.Common.Models;

namespace Sale_Saas.Infrastructure.Services.TenantService.DTOs;

public class GroupTenantDto : BaseEntityDto
{
	public string Code { get; set; } = "";
	public string Name { get; set; } = "";
	public string Logo { get; set; } = "";
	public string Email { get; set; } = "";
	public string Password { get; set; } = "";
	public string Address { get; set; } = "";
	public string Phone { get; set; } = "";
	public PlanServiceDto? PlanService { get; set; } = new PlanServiceDto();
}

public class GroupTenantOrderPlanserviceDto
{
	public Guid OrderId { get; set; }
	public Guid GroupTenantId { get; set; }
	public Guid PlanServiceId { get; set; }
	public string Name { get; set; } = "";
	public decimal Price { get; set; }
	public int Time { get; set; }
}
public class AddOrUpdateGroupTenantDto : BaseEntityDto
{
	public string? Code { get; set; }
	public string? Name { get; set; }
	public string? Logo { get; set; }
	public string? Email { get; set; }
	public string? Password { get; set; }
	public string? Address { get; set; }
	public string? Phone { get; set; }
	public Guid? PlanServiceId { get; set; }
}

public class GroupTenantHomeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string CompanyName { get; set; }
    public string FullName { get; set; }
    public string Logo { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Address { get; set; }
    public string Phone { get; set; }
	public string PlanService { get; set; }
	public DateTime ExpiryTime { get; set; }
    public string Domain { get; set; }
    public string? Color1 { get; set; }
    public string? Color2 { get; set; }
}

public class GroupTenantExtendPlanServiceDto
{
    public Guid Id { get; set; }
	public string Name { get; set; }
    public DateTime ExpiryTime { get; set; }
    public DateTime ExtraExpiryTime { get; set; }
}
