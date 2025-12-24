using System.ComponentModel.DataAnnotations;

namespace Sale_Saas.Domain.Entities
{
	public class Feature
	{
		public Feature() {
			CreatedDate = System.DateTime.Now;
			LastModifiedDate = System.DateTime.Now;
			Id = "";
			Sort = 0;
		}
		[Key]
		public string Id { get; set; }
		public string? Name { get; set; }
		public bool DeleteFlag { set; get; }
		public int? Sort { get; set; } = 0;
		public DateTime CreatedDate { get; set; }
		public DateTime LastModifiedDate { get; set; }
		public Guid? CreatedApplicationUserId { set; get; }
		public Guid? LastModifiedApplicationUserId { set; get; }
		public ICollection<FeatureMenu>? FeatureMenus { get; set; }
		public ICollection<ApplicationRoleDetail>? RoleDetails { get; set; }
	}
}
