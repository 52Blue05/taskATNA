namespace Sale_Saas.Domain.Entities
{
    public class RelationshipHistory : BaseAuditableEntity
    {
        public Guid? RelationshipId { get; set; }
        public Relationship? Relationship { get; set; }
        public Guid? ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
        public Guid? PreviousLevelId { get; set; }
        public RelationshipLevel? PreviousLevel { get; set; }
        public Guid? UpdatedLevelId { get; set; }
        public RelationshipLevel? UpdatedLevel { get; set; }
    }
}
