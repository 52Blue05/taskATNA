namespace Sale_Saas.Domain.Entities
{
    public class Contract : BaseAuditableEntity
    {
        public string? Code { get; set; }
        public string? Number { get; set; }
        public string? Name { get; set; }    
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public Guid? ContractStatusId { get; set; }
        public ContractStatus? ContractStatus { get; set; }
        public ICollection<Project>? Projects { set; get; }
    }
}
