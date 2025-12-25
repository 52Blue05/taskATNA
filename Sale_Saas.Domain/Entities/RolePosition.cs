using System.ComponentModel.DataAnnotations;

namespace Sale_Saas.Domain.Entities
{
    public class RolePosition
    {
        [Key]
        public string Id { set; get; }
        public string? Name { get; set; } = "";
        public int? Level { get; set; }
        public bool DeleteFlag { set; get; } = false;
        public bool IsModified { get; set; } = true;
        public DateTime CreatedDate { get; set; } = System.DateTime.Now;
        public DateTime LastModifiedDate { get; set; } = System.DateTime.Now;
        public Guid? CreatedApplicationUserId { set; get; }
        public Guid? LastModifiedApplicationUserId { set; get; }
        public ICollection<ApplicationRole>? ApplicationRoles { get; set; }
		public ICollection<RolePositionFeatureMenu>? RolePositionFeatureMenus { get; set; }
	}
}
