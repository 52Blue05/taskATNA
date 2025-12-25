namespace Sale_Saas.Application.Models.Notification
{
	public static class NotificationAction
	{
		public static class GoalMsg
		{
			public static string Title { get; set; } = "Thông báo mục tiêu";
			public static string Type { get; set; } = "Goal";

			public static string Create(string name,DateTime? date = null)
			{
				string time = date.HasValue ? $"vào lúc {date.Value.ToString("HH:mm dd-MM")}" : "";
				return $"{name} vừa đề xuất mục tiêu {time} ";
			}
			public static string Modified(string name, DateTime? date = null)
			{
				string time = date.HasValue ? $"vào lúc {date.Value.ToString("HH:mm dd-MM")}" : "";
				return $"{name} vừa cập nhật mục tiêu của bạn {time} ";
			}

			public static string UpdateResult(DateTime? date = null)
			{
				string time = date.HasValue ? $"vào lúc {date.Value.ToString("HH:mm dd-MM")}" : "";
				return $"Mục tiêu của bạn vừa được cập nhật kết quả {time}";
			}

			public static string Expired(DateTime date)
			{
				string time = date.ToString("HH:mm dd-MM-yyyy");
				return $"Mục tiêu của bạn sắp hết hạn vào lúc {time}";
			}
		}

        public static class SyllabusMsg
        {
            public static string Title { get; set; } = "Thông báo thời gian hết hạn chương trình đào tạo";
            public static string Type { get; set; } = "Syllabus";

            public static string Expired(string syllabusName)
            {
                string time = DateTime.Now.AddDays(1).ToString("dd-MM-yyyy");
                return $"Chương trình học {syllabusName} mà bạn tham gia sẽ hết hạn vào {time}. Vui lòng bỏ qua nếu bạn đã hoàn thành chương trình học";
            }
        }
    }

	public class PushNotificationRequest
	{
		public PushNotificationRequest() { }
		public PushNotificationRequest(
			string? title = "",
			string? message = "",
			string? type = "",
			string? actionId = "",
			Guid? ApplicationUserId = null,
			string? TenantId = null)
		{
			this.Title = title;
			this.Message = message;
			this.Type = type;
			this.ActionId = actionId;
			if (ApplicationUserId != null && ApplicationUserId != Guid.Empty) { this.ApplicationUserId = ApplicationUserId; }
			if (!string.IsNullOrEmpty(TenantId)) { this.TenantId = TenantId; }
		}
	
		public string? Title { get; set; } = "";
		public string? Message { get; set; } = "";
		public string? Type { get; set; } = "";
		public string? ActionId { get; set; } = "";
		public string? Navigate { get; set; } = "";
		public Guid? ApplicationUserId { get; set; }
		public string? TenantId { get; set; }
		public Guid? CreatedApplicationUserId { get; set; }
		public Guid? LastModifiedApplicationUserId { get; set; }
	}
}
