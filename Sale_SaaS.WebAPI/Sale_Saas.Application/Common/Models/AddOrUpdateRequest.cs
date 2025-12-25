namespace Sale_Saas.Application.Common.Models;

public class AddOrUpdateRequest
{
    public Guid? Id { set; get; }
    public Dictionary<string, string>? Data { set; get; }
    public Guid? CreatedApplicationUserId { set; get; }
    public Guid? LastModifiedApplicationUserId { set; get; }
    public List<CustomerByGoal>? Customers { get; set; }
}

public class AddOrUpdateMediatrRequest
{
    public List<AddOrUpdateRequest>? List { get; set; }
    public Guid UserId { set; get; }
    public string? TenantId { get; set; }
}


public class GoalRequest
{
    public Guid? Id { set; get; }
    public string? CriteriaType { get; set; }
    public string Criteria { get; set; } = "";
    public decimal TargetKPI { get; set; }
    public decimal TargetPoint { get; set; }
    public decimal? ActualPoint { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public string? ApplicationRoleId { get; set; }
    public List<CustomerByGoal>? Customers { get; set; }
}

public class CustomerByGoal
{
    public Guid? Id { set; get; }
    public string FullName { get; set; }
    public decimal? Point { get; set; }
    public decimal? ActualPoint { get; set; }
}


public class UpdateBenefitIdRequest
{
    public List<Guid>? GoalIds { get; set; }
    public Guid BenefitId { set; get; }
    public Guid UserId { set; get; }
}