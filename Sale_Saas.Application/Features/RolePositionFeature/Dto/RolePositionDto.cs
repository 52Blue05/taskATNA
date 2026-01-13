namespace Sale_Saas.Application.Features.RolePositionFeature.Dto
{
	public class RolePositionDto
	{
		public string Id { set; get; }
		public string? Name { get; set; } = "";
		public int? Level { get; set; }
		private class Mapping : Profile
		{
			public Mapping()
			{
				CreateMap<RolePosition, RolePositionDto>();
			}
		}
	}
}
