using Sale_Saas.Application.Features.RelationshipLevelFeature.Dto;

namespace Sale_Saas.Application.Features.RelationshipHistoryFeature.Dto;

public class RelationshipHistoryMobileDto : BaseEntityDto
{
    public string ApplicationUser { get; set; } = "";
    public RelationshipLevelMobileDto PreviousLevel { get; set; }
    public RelationshipLevelMobileDto UpdatedLevel { get; set; }
    public DateTime CreatedDate { get; set; }
    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<RelationshipHistory, RelationshipHistoryMobileDto>()
                 .ForMember(dest => dest.ApplicationUser, opt => opt.MapFrom(src => src.ApplicationUser != null ? src.ApplicationUser.FullName ?? "" : ""))
                 .ForMember(dest => dest.PreviousLevel, opt => opt.MapFrom(src => src.PreviousLevel))
                 .ForMember(dest => dest.UpdatedLevel, opt => opt.MapFrom(src => src.UpdatedLevel));
        }
    }
}
