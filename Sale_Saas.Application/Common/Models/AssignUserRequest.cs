namespace Sale_Saas.Application.Common.Models
{
    public class AssignUserRequest
    {
        public Guid? Id { get; set; }
        public Guid? ApplicationUserId { get; set; }
        public Guid? CreatedApplicationUserId { set; get; }
        public Guid? LastModifiedApplicationUserId { set; get; }
        public DateTime? OpportunityStartDate { get; set; }
        public DateTime? OpportunityEndDate { get; set; }
        public Guid? ApplicationRoleId { get; set; }
    }
}
