namespace Sale_Saas.Application.Features.RelationshipCustomerFeature.Dto
{
     public class RelationshipCustomerDto : BaseEntityDto
     {
          public string? Name { get; set; }

          private class Mapping : Profile
          {
               public Mapping()
               {
                    CreateMap<RelationshipCustomer, RelationshipCustomerDto>().ReverseMap();
               }
          }
     }
}
