using Sale_Saas.Application.Features.CustomerFeature.Dto;
using Sale_Saas.Application.Features.OpportunityStatusFeature.Dto;
using Sale_Saas.Application.Models.Identity;

namespace Sale_Saas.Application.Features.OpportunityFeature.Dto
{
    public class OpportunityDto : BaseEntityDto
    {
        public string CustomerName { get; set; } = "";
        public string Accountable { get; set; } = "";
        public string TechnicalLead { get; set; } = "";
        public string Need { get; set; } = "";
        public string Beneficiary { get; set; } = "";
        public DateTime EstimatedTime { get; set; }
        public decimal Budget { get; set; }
        public decimal EstimatedMoney { get; set; }
        public decimal CommissionMoney { get; set; }
        public string Opponent1 { get; set; } = "";
        public string Opponent1Attribute { get; set; } = "";
        public string Opponent1Strength { get; set; } = "";
        public string Opponent1Weakness { get; set; } = "";
        public string Opponent2 { get; set; } = "";
        public string Opponent2Attribute { get; set; } = "";
        public string Opponent2Strength { get; set; } = "";
        public string Opponent2Weakness { get; set; } = "";
        public string Opponent3 { get; set; } = "";
        public string Opponent3Attribute { get; set; } = "";
        public string Opponent3Strength { get; set; } = "";
        public string Opponent3Weakness { get; set; } = "";
        public string Strategy { get; set; } = "";
        public DateTime? LastTimeInteract { get; set; }
        public string WinningOppotunity { get; set; } = "";
        public string Reason { get; set; } = "";
        public string? TypeMoney { get; set; }               // Loại tiền
        public decimal? CurrencyConversion { get; set; }     // Quy đổi tiền tệ
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public Guid? CreatedApplicationUserId { set; get; }
        public Guid? LastModifiedApplicationUserId { set; get; }
        public DateTime? OpportunityStartDate { get; set; }
        public DateTime? OpportunityEndDate { get; set; }
        public UserBasicInfoDto ApplicationUser { get; set; } = new UserBasicInfoDto();
        public OpportunityStatusDto OpportunityStatus { get; set; } = new OpportunityStatusDto();
        public CustomerDto Customer { get; set; } = new CustomerDto();
        public List<OpportunityOpponentDto>? OpportunityOpponents { get; set; }
        public Guid? ApplicationRoleId { get; set; }
        public string? ApplicationRoleName { get; set; }
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Opportunity, OpportunityDto>()
                    .ForMember(dest => dest.OpportunityStatus, opt => opt.MapFrom(src => src.OpportunityStatus))
                    .ForMember(dest => dest.ApplicationRoleName, opt => opt.MapFrom(src => src.ApplicationRole != null ? src.ApplicationRole.DisplayName ?? "" : ""))
                    .ForMember(dest => dest.ApplicationUser, opt => opt.MapFrom(src => src.ApplicationUser))
                    .ForMember(dest => dest.Customer, opt => opt.MapFrom(src => src.Customer))
                    .ForMember(dest => dest.OpportunityOpponents, opt => opt.MapFrom(src => src.OpportunityOpponents));

                /* CreateMap<Opportunity, OpportunityDto>()
                     .ForMember(dest => dest.OpportunityStatus, opt => opt.MapFrom(src => src.OpportunityStatus ?? PropertiesExtension.GetEmpty(new OpportunityStatus()) ))
                     .ForMember(dest => dest.ApplicationUser, opt => opt.MapFrom(src => src.ApplicationUser ?? PropertiesExtension.GetEmpty(new ApplicationUser()) ));*/

            }
        }
    }
}
