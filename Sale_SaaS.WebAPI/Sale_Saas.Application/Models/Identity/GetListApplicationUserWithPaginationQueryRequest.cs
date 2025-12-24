namespace Sale_Saas.Application.Models.Identity;

public class GetListApplicationUserWithPaginationQueryRequest : GetListWithPaginationQueryRequest
{
    public string? ApplicationUserId { set; get; }
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName { get; set; }
    public string? Email { set; get; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? DateOfBirth { get; set; }
    public string? RoleName { get; set; }
}