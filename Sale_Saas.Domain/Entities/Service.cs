namespace Sale_Saas.Domain.Entities;

public class Service : BaseAuditableEntity
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? ShortName { get; set; }
}
