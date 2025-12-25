using Sale_Saas.Application.Features.ContractStatusFeature.Dto;
using Sale_Saas.Application.Features.CustomerFeature.Dto;

namespace Sale_Saas.Application.Features.ContractFeature.Dto
{
    public class ContractDto : BaseEntityDto
    {
        public string Code { get; set; } = "";
        public string Number { get; set; } = "";
        public string Name { get; set; } = "";
        public DateTime StartDate { get; set; } = new DateTime();
        public DateTime EndDate { get; set; } = new DateTime();
        public CustomerDto Customer { get; set; } = new CustomerDto();
        public ContractStatusDto ContractStatus { get; set; } = new ContractStatusDto();
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Contract, ContractDto>()
                    .ForMember(dest => dest.ContractStatus, opt => opt.MapFrom(src => src.ContractStatus != null ? src.ContractStatus : new ContractStatus()));
            }
        }
    }
}
