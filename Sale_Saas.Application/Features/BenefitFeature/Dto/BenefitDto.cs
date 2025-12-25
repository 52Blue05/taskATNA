using Sale_Saas.Application.Features.BenefitStatusFeature.Dto;
using Sale_Saas.Application.Features.TargetFluctuationFeature.Dto;
using Sale_Saas.Application.Models.Identity;

namespace Sale_Saas.Application.Features.BenefitFeature.Dto
{
    public class BenefitDto : BaseEntityDto
    {
        public Guid? ApplicationRoleId { get; set; }
        public string? RolePositionId { get; set; }
        public decimal TotalBenefit { get; set; }
        public decimal PointKPI { get; set; }
        public decimal MonthlySalary { get; set; }
        public decimal TargetSalary { get; set; }
        public decimal TotalSalary { get; set; }

        public decimal SuggestMonthlySalary { get; set; }
        public decimal SuggestTargetSalary { get; set; }
        public decimal SuggestTotalSalary { get; set; }
        public decimal EstimateBenefit { get; set; }
        public DateTime CreatedDate { get; set; }

        public UserBasicInfoDto ApplicationUser { get; set; } = new UserBasicInfoDto();
        public BenefitStatusDto BenefitStatus { get; set; } = new BenefitStatusDto();
        public ApplicationRoleDto? ApplicationRole { get; set; } = new ApplicationRoleDto();
        public List<TargetFluctuationDto>? TargetFluctuations { get; set; } = new List<TargetFluctuationDto>();

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Benefit, BenefitDto>()
                    .ForMember(dest => dest.ApplicationUser, opt => opt.MapFrom(src => src.ApplicationUser))
                    .ForMember(dest => dest.BenefitStatus, opt => opt.MapFrom(src => src.BenefitStatus))
                    .ForMember(dest => dest.ApplicationRole, opt => opt.MapFrom(src => src.ApplicationRole))
                    .ForMember(dest => dest.TargetFluctuations, opt => opt.MapFrom(src => src.TargetFluctuations));
            }
        }
    }

    public class BenefitUpdateTotalBenefit : BaseEntityDto
    {
        public decimal? TotalBenefit { get; set; }
    }

    //public class BenefitMobileDto : BaseEntityDto
    //{
    //    public decimal TotalBenefit { get; set; }
    //    public decimal PointKPI { get; set; }
    //    public decimal MonthlySalary { get; set; }
    //    public decimal TargetSalary { get; set; }
    //    public decimal TotalSalary { get; set; }

    //    public decimal SuggestMonthlySalary { get; set; }
    //    public decimal SuggestTargetSalary { get; set; }
    //    public decimal SuggestTotalSalary { get; set; }
    //    public DateTime CreatedDate { get; set; }
    //    public BenefitStatusDto BenefitStatus { get; set; } = new BenefitStatusDto();

    //}

}
