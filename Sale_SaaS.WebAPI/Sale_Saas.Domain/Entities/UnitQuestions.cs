
using System.ComponentModel.DataAnnotations.Schema;

namespace Sale_Saas.Domain.Entities
{
    public class UnitQuestions : BaseAuditableEntity
    {
        public int? MaxScore { get; set; }
        public int? MinScore { get; set; }
        public int?  MaxNumberQuestion { get; set; }
        public int? MinNumberQuestion { get; set; }
        public int? Time {  get; set; }

        public Guid UnitId { get; set; }
        [ForeignKey("UnitId")]
        public Units? Units { get; set; }

        public ICollection<Question>? Questions { get; set; }
    }
}
