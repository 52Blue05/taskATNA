namespace Sale_Saas.Application.Models.Identity;

public class LoginDto
{
    public string? Token { set; get; }
    public DateTime ValidTo { set; get; }
    public List<string>? TenantIds { set; get; }
	public Guid? GroupTenantId { set; get; }
	public bool? IsAdmin { set; get; }
    public bool? IsSaaS { set; get;}
    public string? AccessToken { set; get; }
    public string? RefreshToken { set; get; }
}

