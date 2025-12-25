namespace Sale_Saas.Application.Features.CustomerFeature.Dto
{
    public class CustomerDto : BaseEntityDto
    {
        public string Code { get; set; } = "";
        public string Fullname { get; set; } = "";

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Customer, CustomerDto>();
            }
        }
    }
}
