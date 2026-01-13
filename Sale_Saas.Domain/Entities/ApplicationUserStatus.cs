using System.ComponentModel.DataAnnotations;

namespace Sale_Saas.Domain.Entities;

public class ApplicationUserStatus : BaseAuditableEntity
{
    public string? Code {  get; set; }
    public string? Name {  get; set; }
    public ICollection<ApplicationUser>? ApplicationUsers { set; get; }
}
