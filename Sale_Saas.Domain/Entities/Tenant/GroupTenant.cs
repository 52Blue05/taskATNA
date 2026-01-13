using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Sale_Saas.Domain.Entities.Tenant;

public class GroupTenant : BaseAuditableEntityTenant
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public Guid Id { get; set; }
	public string? Code { get; set; }
	public string? Name { get; set; }
	public string? Logo { get; set; }
    public string? Domain { get; set; }
    public string? Color1 { get; set; }
    public string? Color2 { get; set; }
    public int? LimitChild { get; set; }
    public int? LimitUser { get; set; }
	public DateTime? ExpiryTime { get; set; }
    public ICollection<User>? ApplicationUsers { set; get; }
	public ICollection<Tenant>? Tenants { set; get; }
	public ICollection<Order>? Orders { set; get; }

}
