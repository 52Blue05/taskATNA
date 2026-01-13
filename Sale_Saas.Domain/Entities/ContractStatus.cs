namespace Sale_Saas.Domain.Entities
{
    public class ContractStatus : BaseAuditableEntity
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public ICollection<Contract>? Contracts { set; get; }
    }
}
