using Sale_Saas.Application.Features.UnitFeature.Dto;

namespace Sale_Saas.Application.Features.SyllabusFeature.Dto;

public class SyllabusOfUserDto : BaseEntityDto
{
    public string SyllabusName { get; set; } = "";
    public List<UnitResultDto> UnitResults { get; set; } = new List<UnitResultDto>();
}
