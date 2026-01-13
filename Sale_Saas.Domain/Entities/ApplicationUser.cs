using Microsoft.AspNetCore.Identity;

namespace Sale_Saas.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
	public ApplicationUser()
	{		
        CreatedDate = DateTime.Now;
        LastModifiedDate = DateTime.Now;
		SMS = false;
        DeleteFlag = false;
    }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Avatar { set; get; }
    public string? Address { get; set; }
    public Guid? ApplicationUserStatusId { set; get; }
    public ApplicationUserStatus? ApplicationUserStatus { set; get; }
    public ICollection<Relationship>? Relationships { get; set; }
    public ICollection<EmployeeSalary>? EmployeeSalaries { get; set; }
    public ICollection<EmployeeSalaryDetail>? EmployeeSalaryDetails { get; set; }
    public ICollection<OpportunityHistory>? OpportunityHistories { get; set; }
    public ICollection<Project>? Projects { get; set; }
    public ICollection<Goal>? Goals { get; set; }
    public ICollection<Opportunity>? Opportunities { get; set; }
    public ICollection<Benefit>? Benefits { get; set; }
	public ICollection<BenefitHistory>? BenefitHistories { get; set; }
    public ICollection<ApplicationUserSyllabus>? ApplicationUserSyllabus { get; set; }
    public ICollection<LoveUnits>? LoveUnits { get; set; }
    public ICollection<Result> Results { get; set; }
    public ICollection<UserLessonProgress> UserLessonProgresses { get; set; }
    public Guid? CreatedApplicationUserId { set; get; }
    public Guid? LastModifiedApplicationUserId { set; get; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastModifiedDate { get; set; }	
	public bool? SMS { set; get; }
	public string? Notes { set; get; }
    public bool? DeleteFlag { set; get; }
    public string Code { get; set; }
    public string? FullName { get; set; }
    public string? CurrentPosition { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Review { get; set; }

}

//public class ApplicationUserTenant
//{
//    public string CodeTenant { get; set; }
//    public ApplicationUser Users { get; set; }
//}