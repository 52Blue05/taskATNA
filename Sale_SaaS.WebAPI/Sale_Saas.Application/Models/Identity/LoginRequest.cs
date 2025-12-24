namespace Sale_Saas.Application.Models.Identity;

public class LoginRequest
{
    public string UserName { get; set; }
	public string Password { get; set; }
    //public bool RememberMe { get; set; }
    public Guid? GroupTenantId { get; set; }
    public string? DeviceID { get; set; }
}

public class RefreshTokenRequest
{
    //public string Email { get; set; }
    //public string? DeviceID { get; set; }
    //public Guid? GroupTenantId { get; set; }
    public string? RefreshToken { set; get; }
}

public class SendOtpRequest
{
    public string Email { get; set; }
    public string? DeviceID { get; set; }
    public Guid? GroupTenantId { get; set; }
}


public class ResponseSendOtp
{ 
    public Guid IdOtp { get; set; }
}
public class UpdateAvatarRequest 
{
    public string Email { get; set; }
    public string? DeviceID { get; set; }
    public Guid? GroupTenantId { get; set; }
    public IFormFile? Avatar { get; set; }
}

public class ResponseRefreshToken
{
    public string? AccessToken { set; get; }
    public string? RefreshToken { set; get; }
    public DateTime ValidTo { set; get; }
}

public class VerifyOTPRequest
{
    public string OtpCode { get; set; }
    public string Email { get; set; }
    public string? DeviceID { get; set; }
    public Guid? GroupTenantId { get; set; }
}

public class AvatarRequest
{
    public string Email { get; set; }
    public string Avatar { get; set; }
    public string? DeviceID { get; set; }
    public Guid? GroupTenantId { get; set; }
}
