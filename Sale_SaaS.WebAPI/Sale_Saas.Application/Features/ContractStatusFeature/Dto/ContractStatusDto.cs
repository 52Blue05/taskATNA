namespace Sale_Saas.Application.Features.ContractStatusFeature.Dto
{
    public class ContractStatusDto : BaseEntityDto
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<ContractStatus, ContractStatusDto>()
                    .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code ?? ""))
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name ?? ""));
            }
        }
    }
}
