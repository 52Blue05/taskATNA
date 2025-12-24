namespace Sale_Saas.Application.Features.TargetFluctuationFeature.Dto
{
    public class TargetFluctuationDto : BaseEntityDto
    {
        public int TargetYear { get; set; }
        public string? TypeMoney { get; set; }
        public decimal TargetSalary { get; set; }
        public decimal CompletionPercent { get; set; }
        public string? CriteriaName { get; set; }
        public Guid? BenefitId { get; set; }
        public Guid? GoalId { get; set; }
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<TargetFluctuationDto, TargetFluctuation>().ReverseMap();
            }
        }
    }
}
