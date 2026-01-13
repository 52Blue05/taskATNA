using Sale_Saas.Application.Features.CriteriaFeature.Dto;
using Sale_Saas.Application.Features.GoalStatusFeature.Dto;
using Sale_Saas.Application.Models.Identity;

namespace Sale_Saas.Application.Features.GoalFeature.Dto
{
    public class GoalDto : BaseEntityDto
    {
        public string CriteriaType { get; set; } = "";
        public string CriteriaName { get; set; } = "";
        public decimal TargetKPI { get; set; }
        public decimal? ActualKPI { get; set; }
        public string Review { get; set; } = "";
        public decimal TargetPoint { get; set; }
        public decimal? ActualPoint { get; set; }
        public string Calculate { get; set; } = "";
        public decimal SuggestTargetKPI { get; set; }
        public decimal SuggestTargetPoint { get; set; }
        public DateTime SuggestEndTime { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Guid? ApplicationRoleId { get; set; }
        public ApplicationRoleDto? ApplicationRole { get; set; }
        public GoalStatusDto GoalStatus { get; set; } = new GoalStatusDto();
        public UserBasicInfoDto UserSuggest { get; set; } = new UserBasicInfoDto();
        public CriteriaDto? Criteria { get; set; }
        public DateTime? SuggestStartTime { get; set; }
        public Guid? BenefitId { get; set; }
        public List<CustomerByGoal>? Customers { get; set; }
        public int? TotalCustomer { get; set; }
        public DateTime? CreatedDate { get; set; }
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Goal, GoalDto>()
                    .ForMember(dest => dest.SuggestEndTime, opt => opt.MapFrom(src => src.SuggestEndTime != null ? src.SuggestEndTime : DateTime.Now))
                    .ForMember(dest => dest.GoalStatus, opt => opt.MapFrom(src => src.GoalStatus))
                    .ForMember(dest => dest.UserSuggest, opt => opt.MapFrom(src => src.UserSuggest))
                    .ForMember(dest => dest.ApplicationRole, opt => opt.MapFrom(src => src.ApplicationRole))
                    .ForMember(dest => dest.Criteria, opt => opt.MapFrom(src => src.Criteria))
                    .ForMember(dest => dest.CriteriaName, opt => opt.MapFrom(src => src.Criteria != null ? src.Criteria.Name : ""))
                    .ForMember(dest => dest.CriteriaType, opt => opt.MapFrom(src => src.Criteria != null ? src.Criteria.Code : ""));
            }
        }
    }

    public class GoalUpdateResult : BaseEntityDto
    {
        public decimal ActualPoint { get; set; }
        public decimal ActualKPI { get; set; }
    }

    public class CriteriaType
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
