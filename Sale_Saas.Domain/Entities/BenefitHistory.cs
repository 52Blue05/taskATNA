namespace Sale_Saas.Domain.Entities
{
	public class BenefitHistory : BaseAuditableEntity
	{
		public Guid? BenefitId { get; set; }
		public Benefit? Benefit { get; set; }
		public Guid? ApplicationUserId { get; set; }
		public ApplicationUser? ApplicationUser { get; set; }
		public Guid? PreviousStatusId { get; set; }
		public BenefitStatus? PreviousStatus { get; set; }
		public Guid? UpdatedStatusId { get; set; }
		public BenefitStatus? UpdatedStatus { get; set; }
	}
}
