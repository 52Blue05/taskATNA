
namespace Sale_Saas.Application.Models.Identity;

public class ApplicationRoleDetailRequest
{
    public Guid? MenuId { set; get; }
    public Guid? ApplicationRoleId { set; get; }        
    public int? Permission { set; get; }
    public Guid? CreatedApplicationUserId { set; get; }
    public Guid? LastModifiedApplicationUserId { set; get; }
}