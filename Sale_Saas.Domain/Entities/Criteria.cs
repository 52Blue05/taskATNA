namespace Sale_Saas.Domain.Entities;

public class Criteria : BaseAuditableEntity
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public ICollection<Goal>? Goals { get; set; }
}
