namespace Sale_Saas.Domain.Entities
{
    public class Goal : BaseAuditableEntity
    {
        public string? CriteriaType { get; set; }
        public string? CriteriaName { get; set; }
        public decimal? TargetKPI { get; set; }
        public decimal? ActualKPI { get; set; }
        public string? Review { get; set; }
        public decimal? TargetPoint { get; set; }
        public decimal? ActualPoint { get; set; }
        public string? Calculate { get; set; }
        public decimal? SuggestTargetKPI { get; set; }
        public decimal? SuggestTargetPoint { get; set; }
        public bool? SendExpiredNotification { get; set; } = false;
        public DateTime? SuggestEndTime { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? RolePosition { get; set; }
        public Guid? ApplicationRoleId { get; set; }
        public ApplicationRole? ApplicationRole { get; set; }
        public Guid? GoalStatusId { get; set; }
        public GoalStatus? GoalStatus { get; set; }
        public Guid? UserSuggestId { get; set; }
        public ApplicationUser? UserSuggest { get; set; }
        public Guid? BenefitId { get; set; }
        public DateTime? SuggestStartTime { get; set; }
        public ICollection<TargetFluctuation>? TargetFluctuations { get; set; }
        public Guid? CriteriaId { get; set; }
        public Criteria? Criteria { get; set; }
    }
}
