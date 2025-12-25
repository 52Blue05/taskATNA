namespace Sale_Saas.Application.Features.LessionFeature.Requests;

public class LessonUpdateSortOrderRequest
{
    public Guid UnitId { get; set; }
    public Guid LessonId { get; set; }
    public int SortOrder { get; set; }
}
