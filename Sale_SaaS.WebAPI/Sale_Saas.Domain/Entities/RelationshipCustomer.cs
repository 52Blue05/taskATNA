namespace Sale_Saas.Domain.Entities
{
     public class RelationshipCustomer : BaseAuditableEntity
     {
          public string? Name { get; set; }

          public ICollection<Relationship> Relationships { get; set; }
     }
}
