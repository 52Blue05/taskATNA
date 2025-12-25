namespace Sale_Saas.Application.Models.Identity;

public class ResetPasswordRequest
{
	public Guid Id { set; get; }
	public string NewPassword { get; set; }
	public Guid LastModifiedApplicationUserId { set; get; }
}
