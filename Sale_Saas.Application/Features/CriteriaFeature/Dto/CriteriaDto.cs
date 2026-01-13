namespace Sale_Saas.Application.Features.CriteriaFeature.Dto;

public class CriteriaDto : BaseEntityDto
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<CriteriaDto, Criteria>().ReverseMap();
        }
    }
}
