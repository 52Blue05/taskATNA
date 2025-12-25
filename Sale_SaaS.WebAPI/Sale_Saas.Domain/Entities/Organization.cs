using System.ComponentModel.DataAnnotations;

namespace Sale_Saas.Domain.Entities
{
    public class Organization : BaseAuditableEntity
    {
        [MaxLength(50)]
        public string? Code { get; set; }
        [MaxLength(255)]
        public string? Name { get; set; }
        public string? Notes { get; set; }
        
    }
}
