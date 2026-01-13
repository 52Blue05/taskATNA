using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Sale_Saas.Domain.Entities.Tenant;

public class PlanService : BaseAuditableEntityTenant
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public Guid Id { get; set; }
	public string? Name { get; set; }
	public decimal Price { get; set; }
	public int Time { get; set; }
    public int? LimitChild { get; set; }
    public int? LimitUser { get; set; }
    public ICollection<PlanServiceModuleTenant>? PlanServiceModuleTenants { set; get; }
	public ICollection<Order>? Orders { set; get; }
}
