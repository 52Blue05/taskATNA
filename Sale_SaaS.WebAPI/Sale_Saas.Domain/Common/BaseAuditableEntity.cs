
namespace Sale_Saas.Domain.Common
{
    public abstract class BaseAuditableEntity: BaseEntity
    {
        public BaseAuditableEntity()
        {
            CreatedDate = System.DateTime.Now;
            LastModifiedDate = System.DateTime.Now;
        }
        public DateTime CreatedDate { get; set; }
		public DateTime LastModifiedDate { get; set; }
        public Guid? CreatedApplicationUserId { set; get; }
        public Guid? LastModifiedApplicationUserId { set; get; }
    }

    public abstract class BaseAuditableEntityTenant 
    {
        public BaseAuditableEntityTenant()
        {
            CreatedDate = System.DateTime.Now;
            LastModifiedDate = System.DateTime.Now;
            DeleteFlag = false;
        }
		public bool? DeleteFlag { set; get; }
		public DateTime? CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public Guid? CreatedApplicationUserId { set; get; }
        public Guid? LastModifiedApplicationUserId { set; get; }
    }
}
