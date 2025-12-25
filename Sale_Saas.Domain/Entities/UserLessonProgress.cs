namespace Sale_Saas.Domain.Entities;

public class UserLessonProgress : BaseAuditableEntity
{
    public Guid? ApplicationUserId { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }
    public Guid? LessonId { get; set; }
    public Lessions? Lesson { get; set; }
    public bool? IsDone { get; set; }
}
