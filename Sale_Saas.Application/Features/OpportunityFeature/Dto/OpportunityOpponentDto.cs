namespace Sale_Saas.Application.Features.OpportunityFeature.Dto;

public class OpportunityOpponentDto : BaseEntityDto
{
    public string? Name { get; set; }
    public string? Strength { get; set; }
    public string? Weakness { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<OpportunityOpponent, OpportunityOpponentDto>().ReverseMap();
        }
    }
}
