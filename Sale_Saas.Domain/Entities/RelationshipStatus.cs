namespace Sale_Saas.Domain.Entities
{
     public class RelationshipStatus : BaseAuditableEntity
     {
          public string? Code { get; set; }
          public string? Name { get; set; }
          public ICollection<Relationship>? Relationships { get; set; }
     }
}
