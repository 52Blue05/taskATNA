using System.ComponentModel.DataAnnotations.Schema;

namespace Sale_Saas.Domain.Entities.Tenant
{
	public class Notification : BaseAuditableEntity
	{
		public string? Title { get; set; }
		public string? Message { get; set; }
		public bool? SeenFlag { get; set; }
		public bool? ReadFlag { get; set; }
		public string? Type { get; set; }
		public string? ActionId { get; set; }	
		public DateTime? SeenDate { get; set; }
		public DateTime? ReadDate { get; set; }
		public string? Navigate { get; set; }

		[ForeignKey("ApplicationUserId")]
		public User? ApplicationUser { get; set; }
		public Guid? ApplicationUserId { get; set; }

		[ForeignKey("TenantId")]
		public Tenant? Tenant { get; set; }
		public string? TenantId { get; set; }
	}
}
