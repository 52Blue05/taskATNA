namespace Sale_Saas.Domain.Entities
{
     public class GainsSchool : BaseAuditableEntity
     {
          public string? Name { get; set; }
          public int? Year { get; set; }
          public Guid? GainsId { get; set; }
          public Gains? Gains { get; set; }
     }
}
