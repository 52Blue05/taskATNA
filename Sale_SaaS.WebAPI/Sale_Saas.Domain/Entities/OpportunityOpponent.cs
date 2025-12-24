namespace Sale_Saas.Domain.Entities;

public class OpportunityOpponent : BaseAuditableEntity
{
    public string? Name { get; set; }
    public string? Strength { get; set; }
    public string? Weakness { get; set; }
    public Guid? OpportunityId { get; set; }
    public Opportunity? Opportunity { get; set; }
}
