using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sale_Saas.Domain.Entities.Tenant;

public class Order : BaseAuditableEntityTenant
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public Guid Id { get; set; }

	[ForeignKey("GroupTenantId")]
	public GroupTenant? GroupTenant { get; set; }
	public Guid? GroupTenantId { get; set; }

	[ForeignKey("PlanServiceId")]
	public PlanService? PlanService { get; set; }
	public Guid? PlanServiceId { get; set; }

	public decimal Price { get; set; }
	public int Time { get; set; }
	public bool? IsActived { get; set; }

	public ICollection<OrderDetail>? OrderDetails { set; get; }
}
