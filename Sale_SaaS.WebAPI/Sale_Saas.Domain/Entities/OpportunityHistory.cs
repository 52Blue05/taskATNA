namespace Sale_Saas.Domain.Entities
{
    public class OpportunityHistory : BaseAuditableEntity
    {
        public string? Goal { get; set; }
        public string? Activity { get; set; }
        public DateTime? Time { get; set; }
        public string? Result { get; set; }
        public Guid? OpportunityId { get; set; }
        public Opportunity? Opportunity { get; set; }
        public Guid? ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
    }
}
