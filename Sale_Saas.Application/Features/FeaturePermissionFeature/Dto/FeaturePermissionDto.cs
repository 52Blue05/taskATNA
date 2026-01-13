using Sale_Saas.Application.Features.BenefitStatusFeature.Dto;

namespace Sale_Saas.Application.Features.FeaturePermissionFeature.Dto
{
	public class FeaturePermissionDto
	{
		public string Id { get; set; }
		public string? Name { get; set; }
		public bool? Access { get; set; }
		public int? Sort { set; get; }
        public Guid FMId { get; set; }
    }

	public class MenuWithFeatureDto : BaseEntityDto
	{
		public int SortOrder { set; get; }
		public string? Code { set; get; }
		public string? Name { set; get; }
		public Guid? ParentId { set; get; }
		public List<FeaturePermissionDto> Features { get; set; } = new List<FeaturePermissionDto>();
	}

	public class UserPermissionDto
	{
		public string Menu { get; set; }
		public List<string> Features { get; set; }
	}
}
