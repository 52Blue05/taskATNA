namespace Sale_Saas.Domain.Entities;

public class UnitDependency : BaseAuditableEntity
{
    public Guid? UnitId { get; set; }
    public Units? Unit { get; set; }
    public Guid? PrerequisiteUnitId { get; set; }
    public Units? PrerequisiteUnit { get; set; }
    public Guid? SyllabusId { get; set; }
    public Syllabus? Syllabus { get; set; }
}
