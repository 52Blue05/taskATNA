namespace Sale_Saas.Application.Models.Identity;

public class ChangePasswordRequest
{
    public Guid Id { set; get; }
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
    public Guid LastModifiedApplicationUserId { set; get; }
}
