
using Sale_Saas.Application.Features.BenefitStatusFeature.Dto;

namespace Sale_Saas.Application.Features.BenefitHistoryFeature.Dto;

public class BenefitHistoryDto : BaseEntityDto
{
	public string ApplicationUser { get; set; } = "";
	public BenefitStatusDto PreviousStatus { get; set; }
	public BenefitStatusDto UpdatedStatus { get; set; }
	public DateTime CreatedDate { get; set; }
	private class Mapping : Profile
	{
		public Mapping()
		{
			CreateMap<BenefitHistory, BenefitHistoryDto>()
				.ForMember(dest => dest.ApplicationUser, opt => opt.MapFrom(src => src.ApplicationUser != null ? src.ApplicationUser.FullName ?? "" : ""))
				.ForMember(dest => dest.PreviousStatus, opt => opt.MapFrom(src => src.PreviousStatus))
				.ForMember(dest => dest.UpdatedStatus, opt => opt.MapFrom(src => src.UpdatedStatus));
		}
	}
}
