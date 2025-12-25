
using System.ComponentModel.DataAnnotations.Schema;

namespace Sale_Saas.Domain.Entities
{
    public class SyllabusUnits : BaseAuditableEntity
    {
        public Guid SyllabusId { get; set; }
        [ForeignKey("SyllabusId")]
        public Syllabus? Syllabus { get; set; }

        public Guid UnitId { get; set; }
        [ForeignKey("UnitId")]
        public Units? Unit { get; set; }

        public int? SortOrder { get; set; }
    }
}
