namespace Sale_Saas.Application.Features.BenefitStatusFeature.Dto
{
    public class BenefitStatusDto : BaseEntityDto
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<BenefitStatus, BenefitStatusDto>();
            }
        }
    }
}
