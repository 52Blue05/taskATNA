namespace Sale_Saas.Application.Features.BenefitFeature.Requests;

public class BenefitMobileAddV3Request
{
    public Guid ApplicationUserId { get; set; }
    public decimal? MonthlySalary { get; set; }
    public decimal? TargetSalary { get; set; }
    public decimal? TotalSalary { get; set; }
    public decimal? EstimateBenefit { get; set; }
    public string RolePositionId { get; set; }
    public Guid? ApplicationRoleId { get; set; }
    public ICollection<TargetFluctuationAddV3Request>? TargetFluctuations { get; set; } = new List<TargetFluctuationAddV3Request>();

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<TargetFluctuationAddV3Request, TargetFluctuation>();
            CreateMap<BenefitMobileAddV3Request, Benefit>()
                .ForMember(dest => dest.TargetFluctuations, opt => opt.MapFrom(src => src.TargetFluctuations));
        }
    }
}

public class TargetFluctuationAddV3Request
{
    public int? TargetYear { get; set; }
    public string? TypeMoney { get; set; }
    public decimal? TargetSalary { get; set; }
    public decimal? CompletionPercent { get; set; }
    public Guid? BenefitId { get; set; }
    public Guid? GoalId { get; set; }
}
