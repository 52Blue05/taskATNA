
using System.ComponentModel.DataAnnotations.Schema;

namespace Sale_Saas.Domain.Entities
{
    public class LoveUnits : BaseAuditableEntity
    {
        public Guid ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser? ApplicationUser { get; set; }

        public Guid UnitId { get; set; }
        [ForeignKey("UnitId")]
        public Units? Units { get; set; }
    }
}
