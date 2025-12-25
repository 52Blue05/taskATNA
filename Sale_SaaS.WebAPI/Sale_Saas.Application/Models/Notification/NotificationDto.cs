
namespace Sale_Saas.Application.Models.Notification
{
	public class NotificationDto : BaseEntityDto
	{
		public string? Title { get; set; }
		public string? Message { get; set; }
		public bool? SeenFlag { get; set; }
		public string? Type { get; set; }
		public string? ActionId { get; set; }
		public DateTime? SeenDate { get; set; }
		public DateTime? ReadDate { get; set; }
		public string? Navigate { get; set; }
		public Guid? ApplicationUserId { get; set; }
		public string? TenantId { get; set; }
		public bool? ReadFlag { get; set; }
		public DateTime? CreatedDate { get; set; }
		private class Mapping : Profile
		{
			public Mapping()
			{
				CreateMap<Sale_Saas.Domain.Entities.Tenant.Notification, NotificationDto>();
			}
		}
	}
}
