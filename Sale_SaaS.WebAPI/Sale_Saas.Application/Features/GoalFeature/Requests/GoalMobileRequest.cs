namespace Sale_Saas.Application.Features.GoalFeature.Requests;

public class GoalMobileRequest
{
    public Guid? Id { set; get; }
    public Guid CriteriaId { get; set; }
    public decimal TargetKPI { get; set; }
    public decimal TargetPoint { get; set; }
    public decimal? ActualPoint { get; set; }
    public string? Calculate { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public string? ApplicationRoleId { get; set; }
    public List<CustomerByGoal>? Customers { get; set; }
}
