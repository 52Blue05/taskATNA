namespace Sale_Saas.Domain.Entities
{
    public class OpportunityStatus : BaseAuditableEntity
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public ICollection<Opportunity>? Opportunities { set; get; }
    }
}
