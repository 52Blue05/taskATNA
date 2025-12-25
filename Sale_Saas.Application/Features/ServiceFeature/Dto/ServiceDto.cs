namespace Sale_Saas.Application.Features.ServiceFeature.Dto
{
    public class ServiceDto : BaseEntityDto
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string ShortName { get; set; } = "";
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Service, ServiceDto>();
            }
        }
    }
}
