namespace Sale_Saas.Domain.Entities
{
    public class GainsQuestion : BaseAuditableEntity
    {
        public int? Code { get; set; }
        public string? Content { get; set; }
        public string? Description { get; set; }
        public ICollection<Relationship_GainsQuestion>? Relationship_GainsQuestion { get; set; }
    }
}
