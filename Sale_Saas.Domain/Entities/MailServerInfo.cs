namespace Sale_Saas.Domain.Entities
{
    public class MailServerInfo : BaseAuditableEntity
    {
        public MailServerInfo()
        {
            SSL = false;
        }
        public string? Ten { get; set; }
        public string? TenEmailGui { set; get; }
        public string? Hostname { set; get; }
        public string? IP { set; get; }
        public string? Username { set; get; }
        public string? FromEmail { set; get; }
        public string? Password { set; get; }
        public string? Port { set; get; }
        public bool SSL { set; get; }
        public string? MailService { set; get; }
    }
}