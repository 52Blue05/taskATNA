namespace Sale_Saas.Application.Features.MailServerInfoFeature.Model;
public class EmailMessage
{
    public List<string> EmailCCs { get; set; }
    public List<string> Files { get; set; }
    public string Filename { get; set; }
    public string DispalyNameEmail { get; set; }
    public string FromEmail { get; set; }
    public string ToEmail { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public bool IsHtml { get; set; }
}