using Sale_Saas.Application.Models.Identity;

namespace Sale_Saas.Application.Features.BenefitFeature.Dto;

public class EmployeeOfBenefitDto : UserBasicInfoDto
{
    public List<ApplicationRoleDto> Roles { get; set; } = new List<ApplicationRoleDto>();
}
