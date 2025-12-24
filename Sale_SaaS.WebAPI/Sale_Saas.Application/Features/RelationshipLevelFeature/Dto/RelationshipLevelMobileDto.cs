namespace Sale_Saas.Application.Features.RelationshipLevelFeature.Dto;

public class RelationshipLevelMobileDto : BaseEntityDto
{
    public string Code { get; set; } = "";
    public string Description { get; set; } = "";

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<RelationshipLevel, RelationshipLevelMobileDto>().ReverseMap();
        }
    }
}
