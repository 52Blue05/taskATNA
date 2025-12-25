
using System.ComponentModel.DataAnnotations.Schema;

namespace Sale_Saas.Domain.Entities
{
    public class ApplicationUserSyllabus : BaseAuditableEntity
    {
        public Guid ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser? ApplicationUser { get; set; }
       
        public Guid SyllabusId { get; set; }
        [ForeignKey("SyllabusId")]
        public Syllabus? Syllabus { get; set; }
      
    }
}
