namespace Sale_Saas.Domain.Entities
{
    public class GoalStatus : BaseAuditableEntity
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public ICollection<Goal>? Goals { set; get; }
    }
}
