namespace Sale_Saas.Application.Models.SaleKit
{
    public class SaleKitAddRequest
    {
        public List<IFormFile> files { get; set; } = new List<IFormFile>();
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public Guid? ApplicationUserId { get; set; }
		public Guid? ParentId { get; set; }
		public string? Folder { get; set; }
    }

	public class SaleKitAddRequestController
	{
		public string Name { get; set; } = "";
		public string Type { get; set; } = "";
		public Guid? ParentId { get; set; } = null;
		public List<IFormFile>? Files { get; set;} = new List<IFormFile>();
	}
}
