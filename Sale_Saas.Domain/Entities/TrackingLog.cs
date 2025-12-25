namespace Sale_Saas.Domain.Entities
{
	public class TrackingLog : BaseAuditableEntity
	{
		public Guid? ApplicationUserId { get; set; }
		public string? FunctionTime { get; set; }
		public string? Method { get; set; }
		public float? ResponseTimeSec { get; set; }
		public float? ResponseTimeMin { get; set; }
		public string? Message { get; set; }
		public string? Status { get; set; }
		public string? Type { get; set; }
		public string? TenantId { get; set; }
	}

	public class TrackingLogResult
	{
		public bool? succeeded { get; set; }
		public string? errorMessage { get; set; }
		public object? data { get; set; }
	}

	public class TrackingLogAverageResult
    {
		public Guid? Id { get; set; }
		public float? Average { get; set; }
		public string? Url { get; set; }
		public DateTime? CreateDate { get; set; }
    }

	public class TrackingLogDto
	{
		public float? Average { get; set; }
		public int? NumberRequest { get; set; }
		public DateTime? FromTime { get; set; }
		public DateTime? ToTime { get; set;}
	}

	public class TrackingLogRequest
	{
        public string? method { get; set; }
		public string? type { get; set; }
        public DateTime? FromTime { get; set; }
		public DateTime? ToTime { get; set; }
    }
}