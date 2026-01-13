namespace Sale_Saas.Application.Features.GainsQuestionFeature.Dto
{
    public class GainsQuestionDto : BaseEntityDto
    {
        public int Code { get; set; }
        public string Content { get; set; } = "";
        public string Description { get; set; } = "";
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<GainsQuestion, GainsQuestionDto>();
            }
        }
    }
}
