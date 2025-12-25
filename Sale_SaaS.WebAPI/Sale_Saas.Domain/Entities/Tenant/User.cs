using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Sale_Saas.Domain.Entities.Tenant
{
    public class User : BaseAuditableEntityTenant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
		public string Code { get; set; }
		[Required, MaxLength(250)]
        public string UserName { get; set; }
        [Required, MaxLength(250)]
        public string Password { get; set; }
      
        [MaxLength(250)]
        public string? FirstName { get; set; }
        [MaxLength(250)]
        public string? LastName { get; set; }
		[MaxLength(250)]
		public string? FullName { get; set; }
		[MaxLength(250)]
        public string? Email { set; get; }
		[MaxLength(250)]
		public string? Address { set; get; }
		[MaxLength(250)]
        public string? Phone { get; set; }
		public DateTime? DateOfBirth { get; set; }
        public Guid? ApplicationUserStatusId { get; set; }
		public UserStatus? ApplicationUserStatus { get; set; }
		public bool? IsAdmin { get; set; }

		[ForeignKey("GroupTenantId")]
		public GroupTenant? GroupTenant { get; set; }
		public Guid? GroupTenantId { get; set; }
        public string? DeviceID { get; set; }
        public string? Avatar { get; set; }
        public string? RefreshToken { get; set; }
    }
}
