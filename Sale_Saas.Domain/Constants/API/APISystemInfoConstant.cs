
namespace Sale_Saas.Domain.Constants.API;

public class APISystemInfoConstant
{
	public int NamBatDauSuDungPhanMem {  get; set; }    
    public const string Session_Token = nameof(Session_Token);
    public const string NA = "N/A";
    public const string OrderDir_ASC = "asc";
    public const string OrderDir_DESC = "desc";
    public const string Default_Logo = "https://test01-api.atnavn.com/api/Storage/sass/tenant-logo.png";

	public string? NoData { set; get; }
}