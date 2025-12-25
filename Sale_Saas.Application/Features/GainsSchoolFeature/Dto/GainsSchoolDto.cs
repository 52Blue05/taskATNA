namespace Sale_Saas.Application.Features.GainsSchoolFeature.Dto
{
     public class GainsSchoolDto : BaseEntityDto
     {
          public string? Name { get; set; }
          public int? Year { get; set; }
          private class Mapping : Profile
          {
               public Mapping()
               {
                    CreateMap<GainsSchool, GainsSchoolDto>();
               }
          }
     }
}
