
namespace Sale_Saas.Application.Features.MailServerInfoFeature.Dto
{
    public class MailServerInfoDto: BaseEntityDto
    {
		public string? Ten { set; get; }
		public string? TenEmailGui { set; get; }
        public string? Hostname { set; get; }
        public string? IP { set; get; }
        public string? Username { set; get; }
        public string? FromEmail { set; get; }
        public string? Password { set; get; }
        public string? Port { set; get; }
        public bool? SSL { set; get; }
        public string? MailService { set; get; }
        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<MailServerInfo, MailServerInfoDto>();
            }
        }
    }
}
