
using System.ComponentModel.DataAnnotations.Schema;

namespace Sale_Saas.Domain.Entities
{
    public class Result : BaseAuditableEntity
    {
        public float? Score { get; set; }
        public int? CorrectQuestion { get; set; }
        public int? ToTalQuestion { get; set; }
        public bool? isOldResult { get; set; }
        public bool? isQualified { get; set; }
        public int? TimeCompletion { get; set; }

        public Guid ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser? ApplicationUser { get; set; }

        public Guid UnitId { get; set; }
        [ForeignKey("UnitId")]
        public Units? Units { get; set; }
    }
}
