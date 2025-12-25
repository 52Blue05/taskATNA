namespace Sale_Saas.Domain.Entities
{
     public class GainsFamily : BaseAuditableEntity
     {
          public string? Relationship { get; set; }
          public string? Name { get; set; }
          public int? YearOfBirth { get; set; }
          public Guid? GainsId { get; set; }
          public Gains? Gains { get; set; }
     }
}
