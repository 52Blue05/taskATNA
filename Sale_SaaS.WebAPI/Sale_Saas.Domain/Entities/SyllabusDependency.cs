namespace Sale_Saas.Domain.Entities;

public class SyllabusDependency : BaseAuditableEntity
{
    public Guid? SyllabusId { get; set; }
    public Syllabus? Syllabus { get; set; }
    public Guid? PrerequisiteSyllabusId { get; set; }
    public Syllabus? PrerequisiteSyllabus { get; set; }
}
