namespace Sale_Saas.Domain.Entities
{
    public class Relationship_GainsQuestion : BaseAuditableEntity
    {
        public Guid? RelationshipId { get; set; }
        public Relationship? Relationship { get; set; }
        public Guid? GainsQuestionId { get; set; }
        public GainsQuestion? GainsQuestion { get; set; }
        public bool Answer { get; set; }
        public string? AnswerDetail { get; set; }
    }
}
