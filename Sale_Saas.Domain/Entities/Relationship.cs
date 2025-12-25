namespace Sale_Saas.Domain.Entities
{
    public class Relationship : BaseAuditableEntity
    {
        public string? CustomerName { get; set; }
        public string? Position { get; set; }
        public string? Reason { get; set; }
        public decimal? Point { get; set; }
        public decimal? ActualPoint { get; set; }
        public Guid? ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
        public Guid? CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public Guid? CurrentRelationshipId { get; set; }
        public RelationshipLevel? CurrentRelationship { get; set; } // Original Relationship Level
        public Guid? TargetRelationshipId { get; set; } // Target Relationship Level
        public RelationshipLevel? TargetRelationship { get; set; }
        public Guid? YearToDateId { get; set; } // Current Relationship Level of Customer
        public RelationshipLevel? YearToDate { get; set; }
        public Guid? GainsId { get; set; }
        public Gains? Gains { get; set; }
        public DateTime? CompletionDate { get; set; }
        public Guid? RelationshipStatusId { get; set; }
        public RelationshipStatus? RelationshipStatus { get; set; }
        public string? RolePosition { get; set; }
        public ICollection<Relationship_GainsQuestion>? Relationship_GainsQuestion { get; set; }
        public ICollection<RelationshipHistory>? RelationshipHistories { get; set; }
        public Guid? GoalId { get; set; }
        public string? Avatar { get; set; }
        public string? WorkPlace { get; set; }

        public Guid? RelationshipCustomerId { get; set; }
        public RelationshipCustomer? RelationshipCustomer { get; set; }
        public Guid? ApplicationRoleId { get; set; }
        public ApplicationRole? ApplicationRole { get; set; }
    }
}
