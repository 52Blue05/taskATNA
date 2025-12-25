using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Common.Models
{
    public class UpdateStatusRequest
    {
        public Guid Id { set; get; }
        public Guid ApplicationUserId { get; set; }
        public string? Status { get; set; }
        public string? Locale { get; set; } = LocaleEnum.vi_VN.ToString();
    }
}
