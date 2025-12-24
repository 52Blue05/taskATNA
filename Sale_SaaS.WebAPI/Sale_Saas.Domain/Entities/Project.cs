namespace Sale_Saas.Domain.Entities
{
    public class Project : BaseAuditableEntity
    {
        public string? Code { get; set; }
        public string? Name { get; set; }    
        public string? Result { get; set; }
        public string? Type { get; set; }
        public int? Point { get; set; }
        public string? Note { get; set; }
        public string? Service { get; set; }
        public Guid? ProjectStatusId { get; set; }
        public ProjectStatus? ProjectStatus { get; set; }
        public Guid? ContractId { get; set; }
        public Contract? Contract { get; set; }
        public Guid? ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
    }
}
