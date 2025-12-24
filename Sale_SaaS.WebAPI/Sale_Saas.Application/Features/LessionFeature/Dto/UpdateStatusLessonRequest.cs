namespace Sale_Saas.Application.Features.LessionFeature.Dto;

public class UpdateStatusLessonRequest
{
    public Guid LessonId { get; set; }
    public bool? IsDone { get; set; }
}
