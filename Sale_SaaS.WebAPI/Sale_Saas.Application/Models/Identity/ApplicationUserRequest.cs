namespace Sale_Saas.Application.Models.Identity;

public class ApplicationUserRequest
{
    public string? Id { set; get; }
    public Guid? CreatedApplicationApplicationUserId { set; get; }
    public Guid? LastModifiedApplicationUserId { set; get; }
    public string? Code { set; get; }
    public Guid? ApplicationUserStatusId { set; get; }
    public string? Address { set; get; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Dob { get; set; }
    public string? Email { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public List<string>? ApplicationRoleNames { set; get; }
    public IFormFile? Avatar { set; get; }
    public string? Description { set; get; }
    public string? ConnectString { set; get; }
}

public class ApplicationUserAdminRequest
{
    public string? ConnectString { set; get; }
    public string UserName { set; get; }
    public Guid? LastModifiedApplicationUserId { set; get; }
}
