
namespace Sale_Saas.Domain.Entities
{
    public class ApplicationRoleDetail : BaseAuditableEntity
    {
        public Guid? ApplicationRoleId {  get; set; }
        public ApplicationRole? ApplicationRole { set; get; }
        public Guid? MenuId { get; set; }
        public Menu? Menu { set; get; }
        public int? Permission { set; get; }
		public string? FeatureId { get; set; }
		public Feature? Feature { set; get; }
	}
}
