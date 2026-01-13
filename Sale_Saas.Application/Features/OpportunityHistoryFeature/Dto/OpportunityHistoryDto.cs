namespace Sale_Saas.Application.Features.OpportunityHistoryFeature.Dto
{
    public class OpportunityHistoryDto : BaseEntityDto
    {
        public string Goal { get; set; } = "";
        public string Activity { get; set; } = "";
        public string ApplicationUser { get; set; } = "";
        public DateTime? Time { get; set; }
        public string Result { get; set; } = "";
        public Guid? OpportunityId { get; set; }

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<OpportunityHistory, OpportunityHistoryDto>()
                    .ForMember(dest => dest.ApplicationUser, opt => opt.MapFrom(src => src.ApplicationUser != null
                    ? src.ApplicationUser.FullName ?? "" : ""));
            }
        }
    }
}
