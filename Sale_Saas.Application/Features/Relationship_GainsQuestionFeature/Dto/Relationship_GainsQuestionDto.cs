namespace Sale_Saas.Application.Features.Relationship_GainsQuestionFeature.Dto
{
    public class Relationship_GainsQuestionDto : BaseEntityDto
    {
        public Guid? RelationshipId { get; set; }
        public Guid? GainsQuestionId { get; set; }
        public string Question { get; set; } = "";
        public bool Answer { get; set; } = false;
        public string AnswerDetail { get; set; }
        public DateTime? CreatedDate { get; set; }
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Relationship_GainsQuestion, Relationship_GainsQuestionDto>()
                    .ForMember(dest => dest.Question, opt => opt.MapFrom(src => src.GainsQuestion != null ? src.GainsQuestion.Content ?? "" : ""));
            }
        }
    }
}
