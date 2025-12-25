namespace Sale_Saas.Domain.Entities
{
    public class TargetFluctuation : BaseAuditableEntity
    {
        public int? TargetYear { get; set; }
        public string? TypeMoney { get; set; }
        public decimal? TargetSalary { get; set; }
        public decimal? CompletionPercent { get; set; }
        public Guid? BenefitId { get; set; }
        public Benefit? Benefit { get; set; }
        public Guid? GoalId { get; set; }
        public Goal? Goal { get; set; }
    }
}
