using Sale_Saas.Application.Models.Identity;
using System.Text.Json.Nodes;

namespace Sale_Saas.Application.Features.SaleKitFeature.Dto
{
    public class SalekitParentDto : BaseEntityDto
    {
		public string Name { get; set; } = "";
		public string Description { get; set; } = "";
        public SalekitParentDto Parent { get; set; }
	}
    public class SaleKitDto : BaseEntityDto
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string FolderName { get; set; } = "";
        public string OriginalFileName { get; set; } = "";
        public string FileName { get; set; } = "";
        public string ContentType { get; set; } = "";
        public long FileSize { get; set; } = 0;
        public string FilePath { get; set; } = "";
        public string ServerPath { get; set; } = "";
        public string Extension { get; set; } = "";
        public string Type { get; set; } = "";
		public Guid? ParentId { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid? LastModifiedApplicationUserId { set; get; }
        public CreatedUserDto CreatedUser { get; set; }

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<SaleKit, SaleKitDto>();
            }
        }
    }

    public class SaleKitDownloadDto
    {
        public byte[] Bytes { get; set; }
        public SaleKit SaleKit { get; set; }
    }
}
