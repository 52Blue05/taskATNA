namespace Sale_Saas.Domain.Entities
{
    public class Supplier : BaseAuditableEntity
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Review { get; set; }
    }
}
