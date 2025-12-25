namespace Sale_Saas.Domain.Entities
{
    public class BenefitStatus : BaseAuditableEntity
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public ICollection<Benefit>? Benefits { set; get; }
		public ICollection<BenefitHistory>? BenefitHistoriesUpdated { set; get; }
		public ICollection<BenefitHistory>? BenefitHistoriesPrevious { set; get; }
	}
}
