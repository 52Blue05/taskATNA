namespace Sale_Saas.Application.Features.UnitFeature.Dto;

public class MobileUnitResultDto : BaseEntityDto
{
    public int? TimeCompletion { get; set; }
    public bool? isQualified { get; set; }
    public bool? isOldResult { get; set; }
    public int? CorrectQuestion { get; set; }
    public int? TotalQuestion { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? Order { get; set; }
}
