namespace Sale_Saas.Domain.Entities.Tenant
{
	public class UserStatus : BaseAuditableEntity
	{
		public string? Code { get; set; }
		public string? Name { get; set; }
		public ICollection<User>? ApplicationUsers { set; get; }
	}
}
