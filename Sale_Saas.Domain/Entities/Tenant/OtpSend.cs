using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sale_Saas.Domain.Entities.Tenant;

public class OtpSend : BaseAuditableEntityTenant
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public Guid Id { get; set; }
    public string PhoneNo { get; set; }
    public string? DeviceId { get; set; }
    public string Otp { get; set; }
    public string Content { get; set; }
    public DateTime DateInput { get; set; }
    public string? ResultContent { get; set; }
    public bool ConfirmFlag { get; set; }
}
