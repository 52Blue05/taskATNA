namespace Sale_Saas.Application.Models.Goal
{
    public class UpdateStatusGoalRequest : UpdateStatusRequest
    {
        public string? Criteria { get; set; }
        public decimal? TargetKPI { get; set; } = 0;
        public decimal? TargetPoint { get; set; } = 0;
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Calculate { get; set; }
        public string? SuggestStartTime { get; set; }
        public string? SuggestEndTime { get; set; }
        public decimal? SuggestTargetKPI { get; set; } = 0;
        public decimal? SuggestTargetPoint { get; set; } = 0;
    }

    public class UpdateRequestGoal
    {
        public Guid Id { set; get; }
        public string? SuggestStartTime { get; set; }
        public string? SuggestEndTime { get; set; }
        public decimal? SuggestTargetKPI { get; set; } = 0;
        public decimal? SuggestTargetPoint { get; set; } = 0;
    }
}
