using Microsoft.AspNetCore.Identity;

namespace Sale_Saas.Domain.Entities;
public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole()
    {
        DeleteFlag = false;
    }
    public string? DisplayName { set; get; }
    public string? Description { set; get; }
    public bool? IsModified { get; set; } = true;
    public ICollection<ApplicationRoleDetail>? RoleDetails { set; get; }
    public ICollection<EmployeeSalary>? EmployeeSalaries { set; get; }
    public ICollection<ApplicationRoleSaleKit>? RoleSaleKits { get; set; }
    public ICollection<Benefit> Benefits { get; set; }
    public ICollection<Goal>? Goals { get; set; }
    public ICollection<Relationship>? Relationships { get; set; }
    public ICollection<Opportunity>? Opportunities { get; set; }
    public System.DateTime CreatedDate { set; get; }
    public System.DateTime LastModifiedDate { set; get; }
    public Guid? CreatedApplicationUserId { set; get; }
    public Guid? LastModifiedApplicationUserId { set; get; }
    public string? RolePositionId { get; set; }
    public RolePosition? RolePosition { get; set; }
    public bool DeleteFlag { set; get; }
}
