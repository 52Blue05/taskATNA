using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sale_Saas.Domain.Entities.Tenant;

public class OrderDetail : BaseAuditableEntityTenant
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public Guid Id { get; set; }

	[ForeignKey("OrderId")]
	public Order? Order { get; set; }
	public Guid? OrderId { get; set; }
	public string? ModuleTenantId { get; set; }
	public string? Name { get; set; }
	public string? NameEn { get; set; }
	public DateTime StartTime { get; set; }
	public DateTime EndTime { get; set; }
}
