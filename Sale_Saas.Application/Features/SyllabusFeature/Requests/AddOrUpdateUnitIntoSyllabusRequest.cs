namespace Sale_Saas.Application.Features.SyllabusFeature.Requests;

public class AddOrUpdateUnitIntoSyllabusRequest
{
    public Guid SyllabusId { get; set; }
    public Guid UnitId { get; set; }
    public int SortOrder { get; set; }
}
