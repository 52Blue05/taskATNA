using Sale_Saas.Domain.Common;
namespace Sale_Saas.Application.Features.OrganizationFeature.Dto
{
    public class OrganizationDto : BaseEntity
    {
        public string Code { get; set; }
        public string Name { get; set; }

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Organization, OrganizationDto>();
            }
        }
    }
}
