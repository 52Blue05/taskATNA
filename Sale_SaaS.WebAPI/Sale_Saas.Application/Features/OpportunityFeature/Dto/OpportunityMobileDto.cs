using Sale_Saas.Application.Features.CustomerFeature.Dto;
using Sale_Saas.Application.Features.OpportunityStatusFeature.Dto;
using Sale_Saas.Application.Models.Identity;

namespace Sale_Saas.Application.Features.OpportunityFeature.Dto;

public class OpportunityMobileDto : BaseEntityDto
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
    public string Strategy { get; set; } = "";
    public DateTime LastTimeInteract { get; set; }
    public DateTime CreatedDate { get; set; }
    public Guid? CreatedApplicationUserId { set; get; }
    public Guid? LastModifiedApplicationUserId { set; get; }
    public string WinningOppotunity { get; set; } = "";
    public string Reason { get; set; } = "";
    public DateTime OpportunityStartDate { get; set; }
    public DateTime OpportunityEndDate { get; set; }
    public string TypeMoney { get; set; } = "";
    public decimal TotalMoney { get; set; }
    public decimal CurrencyConversion { get; set; }
    public Guid? ApplicationRoleId { get; set; }
    public UserBasicInfoDto ApplicationUser { get; set; } = new UserBasicInfoDto();
    public OpportunityStatusDto OpportunityStatus { get; set; } = new OpportunityStatusDto();
    public CustomerDto Customer { get; set; } = new CustomerDto();

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Opportunity, OpportunityMobileDto>()
                .ForMember(dest => dest.OpportunityStatus, opt => opt.MapFrom(src => src.OpportunityStatus))
                .ForMember(dest => dest.ApplicationUser, opt => opt.MapFrom(src => src.ApplicationUser))
                .ForMember(dest => dest.Customer, opt => opt.MapFrom(src => src.Customer));
        }
    }
}
