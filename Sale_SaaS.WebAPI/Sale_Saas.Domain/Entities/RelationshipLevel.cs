namespace Sale_Saas.Domain.Entities
{
    public class RelationshipLevel : BaseAuditableEntity
    {
        public string? Code { get; set; }
        public string? Description { get; set; }
        public string? Review { get; set; }
        public int? PointFrom { get; set; }
        public int? PointTo { get; set; }
        public int? SortOrder { get; set; }
        public ICollection<Relationship>? CurrentRelationships { get; set; }
        public ICollection<Relationship>? TargetRelationships { get; set; }
        public ICollection<Relationship>? YearToDateRelationships { get; set; }
        public ICollection<RelationshipHistory>? RelationshipHistoriesUpdated { get; set; }
        public ICollection<RelationshipHistory>? RelationshipHistoriesPrevious { get; set; }
    }
}
