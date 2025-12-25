namespace Sale_Saas.Application.Features.UnitFeature.Dto;

public class CreateOrUpdateUnitPrerequisiteRequest
{
    public Guid? UnitId { get; set; }
    public Guid? PrerequisiteUnitId { get; set; }
    public Guid? SyllabusId { get; set; }
}
