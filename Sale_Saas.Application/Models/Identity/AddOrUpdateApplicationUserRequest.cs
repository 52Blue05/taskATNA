namespace Sale_Saas.Application.Models.Identity;

public class AddOrUpdateApplicationUserRequest: AddOrUpdateRequest
{
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? Role { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Code { get; set; }
    public string? Address { get; set; }
    public string? Avatar { set; get; }
    public string? Email { set; get; }
    public string? Phone { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DateOfBirth { get; set; }

    //public IFormFile? Avatar { set; get; }
    //public string? FormUrlEncodedContent_Data { set; get; }
}

public class RegisterRequest
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? Code { get; set; }
    public string? Address { get; set; }
    public string? Avatar { set; get; }
    public string? FullName { set; get; }
    public string? Phone { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? DeviceID { get; set; }
    public Guid? GroupTenantId { get; set; }
    public bool? IsHasOtp { get; set; }
    public string? OtpCode { get; set; }
}