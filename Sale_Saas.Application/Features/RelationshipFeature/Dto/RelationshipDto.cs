using Sale_Saas.Application.Features.RelationshipCustomerFeature.Dto;
using Sale_Saas.Application.Features.RelationshipLevelFeature.Dto;
using Sale_Saas.Application.Features.RelationshipStatusFeature.Dto;
using Sale_Saas.Application.Models.Identity;

namespace Sale_Saas.Application.Features.RelationshipFeature.Dto
{
    public class RelationshipDto : BaseEntityDto
    {
        public string CustomerName { get; set; } = "";
        public string Position { get; set; } = "";
        public string Reason { get; set; } = "";
        public decimal Point { get; set; } = 0;
        public decimal? ActualPoint { get; set; }
        public string? Customer { get; set; } = "";
        public string CurrentRelationshipLevel { get; set; } = "";
        public string TargetRelationshipLevel { get; set; } = "";
        public string YearToDateLevel { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public RelationshipStatusDto RelationshipStatus { get; set; } = new RelationshipStatusDto();
        public UserBasicInfoDto ApplicationUser { get; set; } = new UserBasicInfoDto();
        public Guid? GoalId { get; set; }
        public string? Avatar { get; set; }
        public string? WorkPlace { get; set; }
        public DateTime? CompletionDate { get; set; }
        public Guid? GainsId { get; set; }
        public string? ApplicationRoleName { get; set; }
        public Guid? ApplicationRoleId { get; set; }
        /*public RelationshipLevelDto CurrentRelationshipLevel { get; set; } = new RelationshipLevelDto();
        public RelationshipLevelDto TargetRelationshipLevel { get; set; } = new RelationshipLevelDto();*/
        public RelationshipLevelDto? YearToDate { get; set; } = new RelationshipLevelDto();
        public RelationshipCustomerDto? RelationshipCustomer { get; set; }

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Relationship, RelationshipDto>()
                    .ForMember(dest => dest.Customer, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Fullname ?? "" : ""))
                    .ForMember(dest => dest.ApplicationRoleName, opt => opt.MapFrom(src => src.ApplicationRole != null ? src.ApplicationRole.DisplayName ?? "" : ""))
                    .ForMember(dest => dest.CurrentRelationshipLevel, opt => opt.MapFrom(src => src.CurrentRelationship != null ? src.CurrentRelationship.Code ?? "" : ""))
                    .ForMember(dest => dest.TargetRelationshipLevel, opt => opt.MapFrom(src => src.TargetRelationship != null ? src.TargetRelationship.Code ?? "" : ""))
                    .ForMember(dest => dest.YearToDateLevel, opt => opt.MapFrom(src => src.YearToDate != null ? src.YearToDate.Code ?? "" : ""))
                    .ForMember(dest => dest.YearToDate, opt => opt.MapFrom(src => src.YearToDate))
                    .ForMember(dest => dest.RelationshipStatus, opt => opt.MapFrom(src => src.RelationshipStatus))
                    .ForMember(dest => dest.RelationshipCustomer, opt => opt.MapFrom(src => src.RelationshipCustomer))
                    .ForMember(dest => dest.ApplicationUser, opt => opt.MapFrom(src => src.ApplicationUser));
            }
        }
    }

    public class RelationshipUpdateResult : BaseEntityDto
    {
        public decimal? ActualPoint { get; set; }
        public Guid? YearToDateId { get; set; }
    }
}
