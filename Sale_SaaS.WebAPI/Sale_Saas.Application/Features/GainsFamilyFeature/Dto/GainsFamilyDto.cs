namespace Sale_Saas.Application.Features.GainsFamilyFeature.Dto
{
     public class GainsFamilyDto : BaseEntityDto
     {
          public string? Relationship { get; set; }
          public string? Name { get; set; }
          public int? YearOfBirth { get; set; }
          private class Mapping : Profile
          {
               public Mapping()
               {
                    CreateMap<GainsFamily, GainsFamilyDto>();
               }
          }
     }
}
