using System.ComponentModel.DataAnnotations;

namespace Sale_Saas.Domain.Entities
{
    public class Customer : BaseAuditableEntity
    {
        public string? Code { get; set; }
        public string? Fullname { get; set; }
        public ICollection<Contract>? Contracts { get; set; }
        public ICollection<Relationship>? Relationships { get; set; }
    }
}
