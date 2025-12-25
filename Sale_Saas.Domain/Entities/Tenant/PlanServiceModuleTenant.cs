using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sale_Saas.Domain.Entities.Tenant;

public class PlanServiceModuleTenant : BaseAuditableEntityTenant
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public Guid Id { get; set; }

	[ForeignKey("PlanServiceId")]
	public PlanService? PlanService { get; set; }
	public Guid? PlanServiceId { get; set; }


	[ForeignKey("ModuleTenantId")]
	public ModuleTenant? ModuleTenant { get; set; }
	public string? ModuleTenantId { get; set; }
}
