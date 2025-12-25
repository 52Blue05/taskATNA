
namespace Sale_Saas.Domain.Entities
{
    public class Syllabus : BaseAuditableEntity
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool? isStart { get; set; }
        public bool? isObligatory { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set;}

        public ICollection<ApplicationUserSyllabus>? ApplicationUserSyllabus { get; set; }
        public ICollection<SyllabusUnits>? SyllabusUnits { get; set; }
    }
}
